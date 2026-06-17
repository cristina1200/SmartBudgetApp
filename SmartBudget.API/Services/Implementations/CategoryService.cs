using SmartBudget.Server.DTOs.Categories;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;

        public CategoryService(
            ICategoryRepository categoryRepository,
            IUserRepository userRepository)
        {
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
        }

        public async Task<List<CategoryResponseDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();

            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type.ToString(),
                UserId = c.UserId
            }).ToList();
        }

        public async Task<List<CategoryResponseDto>> GetByUserIdAsync(int userId)
        {
            var categories = await _categoryRepository.GetByUserIdAsync(userId);

            return categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type.ToString(),
                UserId = c.UserId
            }).ToList();
        }

        public async Task<CategoryResponseDto?> GetByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return null;
            }

            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Type = category.Type.ToString(),
                UserId = category.UserId
            };
        }

        public async Task<CategoryResponseDto?> CreateAsync(CreateCategoryDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return null;
            }

            var category = new Category
            {
                Name = dto.Name,
                Type = dto.Type,
                UserId = dto.UserId
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);

            return new CategoryResponseDto
            {
                Id = createdCategory.Id,
                Name = createdCategory.Name,
                Type = createdCategory.Type.ToString(),
                UserId = createdCategory.UserId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
            {
                return false;
            }

            await _categoryRepository.DeleteAsync(category);

            return true;
        }
    }
}