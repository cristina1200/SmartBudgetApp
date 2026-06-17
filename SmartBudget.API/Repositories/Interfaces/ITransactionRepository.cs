using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface ITransactionRepository
    {
        Task<List<Transaction>> GetAllAsync();

        Task<List<Transaction>> GetByUserIdAsync(int userId);

        Task<Transaction?> GetByIdAsync(int id);

        Task<Transaction> CreateAsync(Transaction transaction);

        Task DeleteAsync(Transaction transaction);
    }
}