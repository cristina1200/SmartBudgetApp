using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;

namespace SmartBudget.Server.Repositories.Implementations
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly SmartBudgetDbContext _context;

        public BudgetRepository(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Budget>> GetAllAsync()
        {
            return await _context.Budgets
                .Include(b => b.User)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<List<Budget>> GetByUserIdAsync(int userId)
        {
            return await _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToListAsync();
        }

        public async Task<Budget?> GetByIdAsync(int id)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<Budget?> GetCurrentBudgetAsync(int userId, int month, int year)
        {
            return await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Month == month &&
                    b.Year == year);
        }

        public async Task<Budget> CreateAsync(Budget budget)
        {
            await _context.Budgets.AddAsync(budget);
            await _context.SaveChangesAsync();

            return budget;
        }

        public async Task<Budget> UpdateAsync(Budget budget)
        {
            _context.Budgets.Update(budget);
            await _context.SaveChangesAsync();

            return budget;
        }

        public async Task DeleteAsync(Budget budget)
        {
            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();
        }
    }
}