using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;

namespace SmartBudget.Server.Repositories.Implementations
{
    public class SavingGoalRepository : ISavingGoalRepository
    {
        private readonly SmartBudgetDbContext _context;

        public SavingGoalRepository(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task<List<SavingGoal>> GetAllAsync()
        {
            return await _context.SavingGoals
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<List<SavingGoal>> GetByUserIdAsync(int userId)
        {
            return await _context.SavingGoals
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }

        public async Task<SavingGoal?> GetByIdAsync(int id)
        {
            return await _context.SavingGoals
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SavingGoal> CreateAsync(SavingGoal savingGoal)
        {
            await _context.SavingGoals.AddAsync(savingGoal);
            await _context.SaveChangesAsync();

            return savingGoal;
        }

        public async Task<SavingGoal> UpdateAsync(SavingGoal savingGoal)
        {
            _context.SavingGoals.Update(savingGoal);
            await _context.SaveChangesAsync();

            return savingGoal;
        }

        public async Task DeleteAsync(SavingGoal savingGoal)
        {
            _context.SavingGoals.Remove(savingGoal);
            await _context.SaveChangesAsync();
        }
    }
}