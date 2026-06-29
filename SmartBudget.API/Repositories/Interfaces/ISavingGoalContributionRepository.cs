using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface ISavingGoalContributionRepository
    {
        Task<List<SavingGoalContribution>> GetBySavingGoalIdAsync(int savingGoalId);

        Task<SavingGoalContribution> CreateAsync(SavingGoalContribution contribution);
    }
}