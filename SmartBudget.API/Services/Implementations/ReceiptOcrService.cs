using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SmartBudget.Server.DTOs.Receipts;
using SmartBudget.Server.Services.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using Tesseract;

namespace SmartBudget.Server.Services.Implementations
{
    public class ReceiptOcrService : IReceiptOcrService
    {
        private readonly IWebHostEnvironment _environment;

        public ReceiptOcrService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<ReceiptScanResponseDto> ScanReceiptAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new ReceiptScanResponseDto
                {
                    Success = false,
                    Message = "No file was uploaded."
                };
            }

            if (file.Length > 8 * 1024 * 1024)
            {
                return new ReceiptScanResponseDto
                {
                    Success = false,
                    Message = "The receipt image is too large. Maximum size is 8 MB."
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff"
            };

            if (!allowedExtensions.Contains(extension))
            {
                return new ReceiptScanResponseDto
                {
                    Success = false,
                    Message = "Invalid file type. Please upload an image."
                };
            }

            var uploadFolder = Path.Combine(_environment.ContentRootPath, "receipt_uploads");

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadFolder, fileName);

            try
            {
                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var tessdataPath = Path.Combine(_environment.ContentRootPath, "tessdata");

                if (!Directory.Exists(tessdataPath))
                {
                    return new ReceiptScanResponseDto
                    {
                        Success = false,
                        Message = $"Tessdata folder was not found at: {tessdataPath}"
                    };
                }

                using var engine = new TesseractEngine(
                    tessdataPath,
                    "ron+eng",
                    EngineMode.Default);

                using var image = Pix.LoadFromFile(filePath);
                using var page = engine.Process(image);

                var rawText = page.GetText() ?? string.Empty;
                var confidence = page.GetMeanConfidence();

                var amount = ExtractTotalAmount(rawText);
                var date = ExtractDate(rawText);
                var merchant = ExtractMerchant(rawText);

                var description = string.IsNullOrWhiteSpace(merchant)
                    ? "Receipt expense"
                    : $"Receipt - {merchant}";

                return new ReceiptScanResponseDto
                {
                    Success = true,
                    SuggestedAmount = amount,
                    SuggestedDate = date ?? DateTime.Now,
                    SuggestedMerchant = merchant,
                    SuggestedDescription = description,
                    RawText = rawText,
                    Confidence = confidence,
                    Message = amount.HasValue
                        ? "Receipt scanned successfully."
                        : "Receipt scanned, but the total amount could not be detected automatically."
                };
            }
            catch (Exception ex)
            {
                return new ReceiptScanResponseDto
                {
                    Success = false,
                    Message = $"OCR failed: {ex.Message}"
                };
            }
            finally
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }

        private static decimal? ExtractTotalAmount(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            /*
             * PRIORITATE 1:
             * Cautam TOTAL simplu, de jos in sus.
             * Nu acceptam TOTAL TVA, SUBTOTAL, TOTAL PLATIT etc.
             */
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var normalized = Normalize(lines[i]);

                if (IsPlainTotalLine(normalized))
                {
                    var totalValue = ExtractAmountAroundPlainTotal(lines, i);

                    if (totalValue.HasValue)
                    {
                        return totalValue.Value;
                    }
                }
            }

            /*
             * PRIORITATE 2:
             * Pentru bonuri unde totalul nu este citit bine,
             * dar exista plata si rest:
             *
             * CASH 201,00
             * REST 75,08-
             *
             * total = 201.00 - 75.08 = 125.92
             */
            var paidAmount = ExtractAmountNearKeywords(lines, new[]
            {
                "TOTAL PLATIT",
                "TOTAL PLATITE",
                "TOTAL PLATA",
                "TOTAL DE PLATA",
                "TOTAL ACHITAT",
                "TOTAL DE ACHITAT",
                "NUMERAR",
                "CASH",
                "CARD"
            });

            var changeAmount = ExtractAmountNearKeywords(lines, new[]
            {
                "REST",
                "REST INAPOIAT",
                "RESTITUIT"
            });

            if (paidAmount.HasValue && changeAmount.HasValue && paidAmount.Value > changeAmount.Value)
            {
                return Math.Round(paidAmount.Value - changeAmount.Value, 2);
            }

            /*
             * PRIORITATE 3:
             * Daca nu gasim TOTAL, cautam SUBTOTAL in partea de jos.
             * Pe multe bonuri digitale SUBTOTAL = TOTAL inainte de TVA.
             */
            for (var i = lines.Count - 1; i >= 0; i--)
            {
                var normalized = Normalize(lines[i]);

                if (normalized.Contains("SUBTOTAL") || normalized.Contains("SUB TOTAL"))
                {
                    var values = ExtractMoneyValues(lines[i]);

                    if (values.Count > 0)
                    {
                        return values.Max();
                    }

                    for (var j = i + 1; j <= i + 2 && j < lines.Count; j++)
                    {
                        var nextValues = ExtractMoneyValues(lines[j]);

                        if (nextValues.Count > 0)
                        {
                            return nextValues.Max();
                        }
                    }
                }
            }

            /*
             * PRIORITATE 4:
             * Cautam cea mai mare suma din partea de jos,
             * dar ignoram CASH, REST, TVA, date fiscale.
             */
            var bottomStartIndex = Math.Max(0, lines.Count - 30);
            var bottomCandidates = new List<decimal>();

            for (var i = bottomStartIndex; i < lines.Count; i++)
            {
                var line = lines[i];
                var normalized = Normalize(line);

                if (IsBadLineForTotalCandidate(normalized))
                {
                    continue;
                }

                if (LooksLikeProductQuantityLine(normalized))
                {
                    continue;
                }

                var values = ExtractMoneyValues(line)
                    .Where(value => value > 0 && value < 100000)
                    .ToList();

                bottomCandidates.AddRange(values);
            }

            if (bottomCandidates.Count > 0)
            {
                return bottomCandidates.Max();
            }

            /*
             * PRIORITATE 5:
             * Fallback final. Cea mai mare suma pozitiva din tot OCR-ul,
             * ignorand linii clare de plata/rest/TVA.
             */
            var allCandidates = new List<decimal>();

            foreach (var line in lines)
            {
                var normalized = Normalize(line);

                if (IsBadLineForTotalCandidate(normalized))
                {
                    continue;
                }

                if (LooksLikeProductQuantityLine(normalized))
                {
                    continue;
                }

                var values = ExtractMoneyValues(line)
                    .Where(value => value > 0 && value < 100000)
                    .ToList();

                allCandidates.AddRange(values);
            }

            if (allCandidates.Count > 0)
            {
                return allCandidates.Max();
            }

            return null;
        }

        private static decimal? ExtractAmountAroundPlainTotal(List<string> lines, int totalLineIndex)
        {
            /*
             * Cazuri posibile OCR:
             *
             * 1) TOTAL 125,92
             * 2) TOTAL
             *    125,92
             * 3) 125,92
             *    TOTAL
             *
             * Ne uitam strict in jurul liniei TOTAL,
             * dar ne oprim daca intram in zona TVA/CASH/REST.
             */

            var sameLineValues = ExtractMoneyValues(lines[totalLineIndex]);

            if (sameLineValues.Count > 0)
            {
                return sameLineValues.Max();
            }

            /*
             * Dupa TOTAL, cautam suma, dar ne oprim la TVA/CASH/REST.
             */
            for (var i = totalLineIndex + 1; i <= totalLineIndex + 5 && i < lines.Count; i++)
            {
                var normalized = Normalize(lines[i]);

                if (IsPaymentOrTaxOrRestLine(normalized))
                {
                    break;
                }

                var values = ExtractMoneyValues(lines[i]);

                if (values.Count > 0)
                {
                    return values.Max();
                }
            }

            /*
             * Inainte de TOTAL, util cand OCR imparte coloanele ciudat.
             */
            for (var i = totalLineIndex - 1; i >= totalLineIndex - 5 && i >= 0; i--)
            {
                var normalized = Normalize(lines[i]);

                if (IsPaymentOrTaxOrRestLine(normalized))
                {
                    continue;
                }

                if (LooksLikeProductQuantityLine(normalized))
                {
                    continue;
                }

                var values = ExtractMoneyValues(lines[i]);

                if (values.Count > 0)
                {
                    return values.Max();
                }
            }

            return null;
        }

        private static decimal? ExtractAmountNearKeywords(List<string> lines, string[] keywords)
        {
            for (var i = 0; i < lines.Count; i++)
            {
                var normalized = Normalize(lines[i]);

                if (!keywords.Any(keyword => normalized.Contains(keyword)))
                {
                    continue;
                }

                var sameLineValues = ExtractMoneyValues(lines[i]);

                if (sameLineValues.Count > 0)
                {
                    return sameLineValues.Max();
                }

                /*
                 * Uneori apare:
                 * Rest
                 * CASH 75,08-
                 */
                for (var j = i + 1; j <= i + 2 && j < lines.Count; j++)
                {
                    var nextValues = ExtractMoneyValues(lines[j]);

                    if (nextValues.Count > 0)
                    {
                        return nextValues.Max();
                    }
                }
            }

            return null;
        }

        private static bool IsPlainTotalLine(string normalizedLine)
        {
            var compact = normalizedLine
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace(":", "")
                .Replace("|", "I");

            var looksLikeTotal =
                compact.Contains("TOTAL") ||
                compact.Contains("T0TAL") ||
                compact.Contains("TQTAL") ||
                compact.Contains("TOTAI") ||
                compact.Contains("TOTA1") ||
                compact.Contains("T0TA1") ||
                compact.Contains("T0TAI") ||
                compact.Contains("T0T4L");

            if (!looksLikeTotal)
            {
                return false;
            }

            /*
             * Excludem liniile care contin TOTAL dar nu sunt totalul final.
             */
            var forbidden = new[]
            {
                "SUBTOTAL",
                "SUB TOTAL",
                "TOTAL TVA",
                "TOTAL T.V.A",
                "TVA",
                "T.V.A",
                "TOTAL PLATIT",
                "TOTAL PLATITE",
                "TOTAL PLATA",
                "TOTAL DE PLATA",
                "TOTAL ACHITAT",
                "TOTAL DE ACHITAT",
                "TOTAL ARTICOLE",
                "TOTAL PRODUSE"
            };

            return !forbidden.Any(keyword => normalizedLine.Contains(keyword));
        }

        private static bool IsPaymentOrTaxOrRestLine(string normalizedLine)
        {
            var keywords = new[]
            {
                "TOTAL TVA",
                "TVA",
                "T.V.A",
                "CASH",
                "NUMERAR",
                "CARD",
                "REST",
                "REST INAPOIAT",
                "RESTITUIT"
            };

            return keywords.Any(keyword => normalizedLine.Contains(keyword));
        }

        private static bool IsBadLineForTotalCandidate(string normalizedLine)
        {
            var badKeywords = new[]
            {
                "TVA",
                "T.V.A",
                "A =",
                "DISCOUNT",
                "REDUCERE",
                "REST",
                "REST INAPOIAT",
                "RESTITUIT",
                "CASH",
                "NUMERAR",
                "CARD",
                "CASIER",
                "CIF",
                "COD",
                "NR POS",
                "NR TRZ",
                "BON FISCAL",
                "FOOTER",
                "DATA",
                "ORA",
                "ARTICOL",
                "COTA",
                "TAXA"
            };

            return badKeywords.Any(keyword => normalizedLine.Contains(keyword));
        }

        private static bool LooksLikeProductQuantityLine(string normalizedLine)
        {
            /*
             * Exemple:
             * 1,000 BUC x 23,99
             * 3,000 BUC x 12,99
             * 1 x 6.99
             */
            return normalizedLine.Contains(" X ") ||
                   normalizedLine.Contains("BUC X") ||
                   normalizedLine.Contains("1X") ||
                   normalizedLine.Contains("2X") ||
                   normalizedLine.Contains("3X") ||
                   normalizedLine.Contains("1,000") ||
                   normalizedLine.Contains("2,000") ||
                   normalizedLine.Contains("3,000") ||
                   normalizedLine.Contains("1.000") ||
                   normalizedLine.Contains("2.000") ||
                   normalizedLine.Contains("3.000");
        }

        private static List<decimal> ExtractMoneyValues(string input)
        {
            var values = new List<decimal>();

            var cleanedInput = PrepareNumericOcrText(input);

            /*
             * Acceptam:
             * 125,92
             * 125.92
             * 6-50
             * 75,08-
             * 141.6I -> 141.61
             */
            var regex = new Regex(
                @"(?<!\d)(-?\d{1,6})\s*([.,-])\s*(\d{2})(?!\d)",
                RegexOptions.Compiled);

            var matches = regex.Matches(cleanedInput);

            foreach (Match match in matches)
            {
                var wholePart = match.Groups[1].Value;
                var decimalPart = match.Groups[3].Value;

                if (wholePart.StartsWith("-"))
                {
                    continue;
                }

                var valueText = $"{wholePart}.{decimalPart}";

                if (decimal.TryParse(
                        valueText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    values.Add(value);
                }
            }

            /*
             * Extra: daca OCR citeste 125 92 fara virgula/punct.
             */
            var spacedRegex = new Regex(
                @"(?<!\d)(\d{1,6})\s+(\d{2})(?!\d)",
                RegexOptions.Compiled);

            var spacedMatches = spacedRegex.Matches(cleanedInput);

            foreach (Match match in spacedMatches)
            {
                var wholePart = match.Groups[1].Value;
                var decimalPart = match.Groups[2].Value;

                var valueText = $"{wholePart}.{decimalPart}";

                if (decimal.TryParse(
                        valueText,
                        NumberStyles.Number,
                        CultureInfo.InvariantCulture,
                        out var value))
                {
                    values.Add(value);
                }
            }

            return values;
        }

        private static string PrepareNumericOcrText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            var value = input;

            value = Regex.Replace(value, @"(?<=\d)[Il|](?=\d|\s|$)", "1");
            value = Regex.Replace(value, @"(?<=\d)[Oo](?=\d)", "0");
            value = Regex.Replace(value, @"(?<=\d)[Ss](?=\d)", "5");
            value = Regex.Replace(value, @"(?<=\d)[B](?=\d)", "8");

            return value;
        }

        private static DateTime? ExtractDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var cleanedText = PrepareNumericOcrText(text);

            var regex = new Regex(
                @"\b(\d{1,2})[./-](\d{1,2})[./-](\d{2,4})\b",
                RegexOptions.Compiled);

            var match = regex.Match(cleanedText);

            if (!match.Success)
            {
                return null;
            }

            var day = int.Parse(match.Groups[1].Value);
            var month = int.Parse(match.Groups[2].Value);
            var year = int.Parse(match.Groups[3].Value);

            if (year < 100)
            {
                year += 2000;
            }

            try
            {
                return new DateTime(year, month, day);
            }
            catch
            {
                return null;
            }
        }

        private static string ExtractMerchant(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var lines = text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => line.Length >= 3)
                .Take(10)
                .ToList();

            foreach (var line in lines)
            {
                var normalized = Normalize(line);

                var skip = normalized.Contains("BON") ||
                           normalized.Contains("FISCAL") ||
                           normalized.Contains("CIF") ||
                           normalized.Contains("NR") ||
                           normalized.Contains("DATA") ||
                           normalized.Contains("CASA") ||
                           normalized.Contains("TOTAL") ||
                           normalized.Contains("TVA");

                if (!skip && line.Any(char.IsLetter))
                {
                    return CleanMerchant(line);
                }
            }

            return string.Empty;
        }

        private static string CleanMerchant(string merchant)
        {
            merchant = Regex.Replace(merchant, @"[^a-zA-Z0-9ăâîșțĂÂÎȘȚ .,&-]", "");
            merchant = Regex.Replace(merchant, @"\s+", " ");

            return merchant.Trim();
        }

        private static string Normalize(string value)
        {
            return value
                .ToUpperInvariant()
                .Replace("Ă", "A")
                .Replace("Â", "A")
                .Replace("Î", "I")
                .Replace("Ș", "S")
                .Replace("Ş", "S")
                .Replace("Ț", "T")
                .Replace("Ţ", "T");
        }
    }
}