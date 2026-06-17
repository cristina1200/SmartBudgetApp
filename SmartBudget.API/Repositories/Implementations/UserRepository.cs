using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;

namespace SmartBudget.Server.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly SmartBudgetDbContext _context;

        public UserRepository(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task DeleteAsync(User user)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}