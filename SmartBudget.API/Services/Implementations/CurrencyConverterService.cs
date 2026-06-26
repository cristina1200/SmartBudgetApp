using System.Xml.Linq;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class CurrencyConverterService : ICurrencyConverterService
    {
        private readonly HttpClient _httpClient;

        public CurrencyConverterService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency)
        {
            if (amount <= 0)
            {
                throw new Exception("Amount must be greater than 0.");
            }

            if (fromCurrency == toCurrency)
            {
                return amount;
            }

            var rates = await GetBnrRatesAsync();

            var amountInRon = ConvertToRon(amount, fromCurrency, rates);

            var convertedAmount = ConvertFromRon(amountInRon, toCurrency, rates);

            return Math.Round(convertedAmount, 2);
        }

        private async Task<Dictionary<Currency, decimal>> GetBnrRatesAsync()
        {
            var url = "https://www.bnr.ro/nbrfxrates.xml";

            var xmlContent = await _httpClient.GetStringAsync(url);

            var document = XDocument.Parse(xmlContent);

            var rates = new Dictionary<Currency, decimal>
            {
                { Currency.RON, 1m }
            };

            var rateElements = document
                .Descendants()
                .Where(x => x.Name.LocalName == "Rate");

            foreach (var rateElement in rateElements)
            {
                var currencyAttribute = rateElement.Attribute("currency");

                if (currencyAttribute == null)
                {
                    continue;
                }

                var currencyCode = currencyAttribute.Value;

                if (!Enum.TryParse<Currency>(currencyCode, out var currency))
                {
                    continue;
                }

                var multiplierAttribute = rateElement.Attribute("multiplier");

                var multiplier = multiplierAttribute == null
                    ? 1m
                    : decimal.Parse(
                        multiplierAttribute.Value,
                        System.Globalization.CultureInfo.InvariantCulture);

                var value = decimal.Parse(
                    rateElement.Value,
                    System.Globalization.CultureInfo.InvariantCulture);

                var rateInRon = value / multiplier;

                rates[currency] = rateInRon;
            }

            return rates;
        }

        private static decimal ConvertToRon(
            decimal amount,
            Currency fromCurrency,
            Dictionary<Currency, decimal> rates)
        {
            if (!rates.ContainsKey(fromCurrency))
            {
                throw new Exception($"Currency {fromCurrency} is not supported by BNR.");
            }

            return amount * rates[fromCurrency];
        }

        private static decimal ConvertFromRon(
            decimal amountInRon,
            Currency toCurrency,
            Dictionary<Currency, decimal> rates)
        {
            if (!rates.ContainsKey(toCurrency))
            {
                throw new Exception($"Currency {toCurrency} is not supported by BNR.");
            }

            return amountInRon / rates[toCurrency];
        }
    }
}