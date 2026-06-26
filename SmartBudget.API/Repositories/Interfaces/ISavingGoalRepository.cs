using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface ISavingGoalRepository
    {
        Task<List<SavingGoal>> GetAllAsync();

        Task<List<SavingGoal>> GetByUserIdAsync(int userId);

        Task<SavingGoal?> GetByIdAsync(int id);

        Task<SavingGoal> CreateAsync(SavingGoal savingGoal);

        Task<SavingGoal> UpdateAsync(SavingGoal savingGoal);

        Task DeleteAsync(SavingGoal savingGoal);
    }
}