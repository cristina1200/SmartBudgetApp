using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface IBudgetRepository
    {
        Task<List<Budget>> GetAllAsync();

        Task<List<Budget>> GetByUserIdAsync(int userId);

        Task<Budget?> GetByIdAsync(int id);

        Task<Budget?> GetCurrentBudgetAsync(int userId, int month, int year);

        Task<Budget> CreateAsync(Budget budget);

        Task<Budget> UpdateAsync(Budget budget);

        Task DeleteAsync(Budget budget);
    }
}