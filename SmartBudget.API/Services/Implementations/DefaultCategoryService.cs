using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class DefaultCategoryService : IDefaultCategoryService
    {
        private readonly SmartBudgetDbContext _context;

        public DefaultCategoryService(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task CreateDefaultCategoriesForUserAsync(int userId)
        {
            var userExists = await _context.Users
                .AnyAsync(user => user.Id == userId);

            if (!userExists)
            {
                return;
            }

            var existingCategories = await _context.Categories
                .Where(category => category.UserId == userId)
                .ToListAsync();

            var defaultCategories = GetDefaultCategories(userId);

            var categoriesToAdd = defaultCategories
                .Where(defaultCategory =>
                    !existingCategories.Any(existingCategory =>
                        existingCategory.Name.ToLower() == defaultCategory.Name.ToLower() &&
                        existingCategory.Type == defaultCategory.Type))
                .ToList();

            if (categoriesToAdd.Count == 0)
            {
                return;
            }

            await _context.Categories.AddRangeAsync(categoriesToAdd);
            await _context.SaveChangesAsync();
        }

        private static List<Category> GetDefaultCategories(int userId)
        {
            return new List<Category>
            {
                // Income categories
                new Category
                {
                    Name = "Salary",
                    Type = TransactionType.Income,
                    UserId = userId
                },
                new Category
                {
                    Name = "Freelance",
                    Type = TransactionType.Income,
                    UserId = userId
                },
                new Category
                {
                    Name = "Gift",
                    Type = TransactionType.Income,
                    UserId = userId
                },
                new Category
                {
                    Name = "Investment",
                    Type = TransactionType.Income,
                    UserId = userId
                },
                new Category
                {
                    Name = "Other income",
                    Type = TransactionType.Income,
                    UserId = userId
                },

                // Expense categories
                new Category
                {
                    Name = "Groceries",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Rent",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Utilities",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Transport",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Health",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Education",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Shopping",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Restaurants",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Entertainment",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Travel",
                    Type = TransactionType.Expense,
                    UserId = userId
                },
                new Category
                {
                    Name = "Other expense",
                    Type = TransactionType.Expense,
                    UserId = userId
                }
            };
        }
    }
}