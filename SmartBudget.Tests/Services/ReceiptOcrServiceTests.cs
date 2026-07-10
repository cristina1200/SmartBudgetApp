using FluentAssertions;
using SmartBudget.Server.Services.Implementations;
using System.Reflection;

namespace SmartBudget.Tests.Services
{
    public class ReceiptOcrServiceTests
    {
        [Fact]
        public void ExtractTotalAmount_ShouldReturnPlainTotal_WhenReceiptContainsTotalLine()
        {
            // Arrange
            var receiptText = """
                S.C. LIDL DISCOUNT S.R.L.
                Selectii mini ciocolata      23,99 A
                Cuvertura sort.             38,97 A
                Subtotal                    125,92
                TOTAL                       125,92
                TOTAL TVA                    18,13
                CASH                        201,00
                Rest
                CASH                         75,08-
                """;

            // Act
            var result = InvokeExtractTotalAmount(receiptText);

            // Assert
            result.Should().Be(125.92m);
        }

        [Fact]
        public void ExtractTotalAmount_ShouldCalculateTotal_WhenOnlyCashAndRestAreAvailable()
        {
            // Arrange
            var receiptText = """
                S.C. LIDL DISCOUNT S.R.L.
                Selectii mini ciocolata      23,99 A
                Cuvertura sort.             38,97 A
                TOTAL TVA                    18,13
                CASH                        201,00
                Rest
                CASH                         75,08-
                """;

            // Act
            var result = InvokeExtractTotalAmount(receiptText);

            // Assert
            result.Should().Be(125.92m);
        }

        [Fact]
        public void ExtractTotalAmount_ShouldIgnoreTotalTva_WhenPlainTotalExists()
        {
            // Arrange
            var receiptText = """
                Market receipt
                Product one                  40,00
                Product two                  60,00
                TOTAL                       100,00
                TOTAL TVA                    19,00
                """;

            // Act
            var result = InvokeExtractTotalAmount(receiptText);

            // Assert
            result.Should().Be(100.00m);
        }

        [Fact]
        public void ExtractTotalAmount_ShouldReturnSubtotal_WhenPlainTotalDoesNotExist()
        {
            // Arrange
            var receiptText = """
                Digital receipt
                Product one                  20,00
                Product two                  30,00
                Subtotal                     50,00
                TVA                           9,50
                """;

            // Act
            var result = InvokeExtractTotalAmount(receiptText);

            // Assert
            result.Should().Be(50.00m);
        }

        [Fact]
        public void ExtractTotalAmount_ShouldReturnNull_WhenNoMoneyValueExists()
        {
            // Arrange
            var receiptText = """
                S.C. LIDL DISCOUNT S.R.L.
                Bon fiscal
                No amount here
                """;

            // Act
            var result = InvokeExtractTotalAmount(receiptText);

            // Assert
            result.Should().BeNull();
        }

        private static decimal? InvokeExtractTotalAmount(string text)
        {
            var method = typeof(ReceiptOcrService).GetMethod(
                "ExtractTotalAmount",
                BindingFlags.NonPublic | BindingFlags.Static);

            method.Should().NotBeNull("ExtractTotalAmount should exist in ReceiptOcrService.");

            var result = method!.Invoke(null, new object[] { text });

            return (decimal?)result;
        }
    }
}