using SmartBudget.Server.DTOs.SavingGoals;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface ISavingGoalService
    {
        Task<List<SavingGoalResponseDto>> GetAllAsync();

        Task<List<SavingGoalResponseDto>> GetByUserIdAsync(int userId);

        Task<SavingGoalResponseDto?> GetByIdAsync(int id);

        Task<SavingGoalResponseDto?> CreateAsync(CreateSavingGoalDto dto);

        Task<SavingGoalResponseDto?> UpdateAsync(int id, UpdateSavingGoalDto dto);

        Task<SavingGoalResponseDto?> AddMoneyAsync(int id, AddMoneyToGoalDto dto);

        Task<bool> DeleteAsync(int id);
    }
}