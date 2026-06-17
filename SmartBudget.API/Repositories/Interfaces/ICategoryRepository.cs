using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllAsync();

        Task<List<Category>> GetByUserIdAsync(int userId);

        Task<Category?> GetByIdAsync(int id);

        Task<Category> CreateAsync(Category category);

        Task DeleteAsync(Category category);
    }
}