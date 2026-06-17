using SmartBudget.Server.DTOs.Dashboard;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly ITransactionRepository _transactionRepository;

        public DashboardService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<DashboardSummaryDto> GetSummaryAsync(int userId)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);

            var totalIncome = transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpenses = transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            return new DashboardSummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                Balance = totalIncome - totalExpenses,
                TransactionsCount = transactions.Count
            };
        }
    }
}