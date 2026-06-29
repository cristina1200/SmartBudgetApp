using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;

namespace SmartBudget.Server.Repositories.Implementations
{
    public class SavingGoalContributionRepository : ISavingGoalContributionRepository
    {
        private readonly SmartBudgetDbContext _context;

        public SavingGoalContributionRepository(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task<List<SavingGoalContribution>> GetBySavingGoalIdAsync(int savingGoalId)
        {
            return await _context.SavingGoalContributions
                .Where(c => c.SavingGoalId == savingGoalId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<SavingGoalContribution> CreateAsync(SavingGoalContribution contribution)
        {
            await _context.SavingGoalContributions.AddAsync(contribution);
            await _context.SaveChangesAsync();

            return contribution;
        }
    }
}