using SmartBudget.Server.DTOs.Budgets;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface IBudgetService
    {
        Task<List<BudgetResponseDto>> GetAllAsync();

        Task<List<BudgetResponseDto>> GetByUserIdAsync(int userId);

        Task<BudgetResponseDto?> GetByIdAsync(int id);

        Task<BudgetResponseDto?> GetCurrentBudgetAsync(int userId);

        Task<BudgetResponseDto?> CreateAsync(CreateBudgetDto dto);

        Task<BudgetResponseDto?> UpdateAsync(int id, UpdateBudgetDto dto);

        Task<bool> DeleteAsync(int id);
    }
}