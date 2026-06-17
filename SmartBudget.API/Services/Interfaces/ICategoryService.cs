using SmartBudget.Server.DTOs.Categories;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllAsync();

        Task<List<CategoryResponseDto>> GetByUserIdAsync(int userId);

        Task<CategoryResponseDto?> GetByIdAsync(int id);

        Task<CategoryResponseDto?> CreateAsync(CreateCategoryDto dto);

        Task<bool> DeleteAsync(int id);
    }
}