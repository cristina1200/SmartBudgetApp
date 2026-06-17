using SmartBudget.Server.DTOs.Transactions;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<List<TransactionResponseDto>> GetAllAsync();

        Task<List<TransactionResponseDto>> GetByUserIdAsync(int userId);

        Task<TransactionResponseDto?> GetByIdAsync(int id);

        Task<TransactionResponseDto?> CreateAsync(CreateTransactionDto dto);

        Task<bool> DeleteAsync(int id);
    }
}