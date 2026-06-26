using SmartBudget.Server.Enums;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface ICurrencyConverterService
    {
        Task<decimal> ConvertAsync(decimal amount, Currency fromCurrency, Currency toCurrency);
    }
}