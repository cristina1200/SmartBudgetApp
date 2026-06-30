using SmartBudget.Server.DTOs.Transactions;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly INotificationService _notificationService;

        public TransactionService(
            ITransactionRepository transactionRepository,
            IUserRepository userRepository,
            ICategoryRepository categoryRepository,
            INotificationService notificationService)
        {
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _categoryRepository = categoryRepository;
            _notificationService = notificationService;
        }

        public async Task<List<TransactionResponseDto>> GetAllAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();

            return transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                Date = t.Date,
                Type = t.Type.ToString(),
                UserId = t.UserId,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name
            }).ToList();
        }

        public async Task<List<TransactionResponseDto>> GetByUserIdAsync(int userId)
        {
            var transactions = await _transactionRepository.GetByUserIdAsync(userId);

            return transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                Amount = t.Amount,
                Description = t.Description,
                Date = t.Date,
                Type = t.Type.ToString(),
                UserId = t.UserId,
                CategoryId = t.CategoryId,
                CategoryName = t.Category.Name
            }).ToList();
        }

        public async Task<TransactionResponseDto?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction == null)
            {
                return null;
            }

            return new TransactionResponseDto
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                Description = transaction.Description,
                Date = transaction.Date,
                Type = transaction.Type.ToString(),
                UserId = transaction.UserId,
                CategoryId = transaction.CategoryId,
                CategoryName = transaction.Category.Name
            };
        }

        public async Task<TransactionResponseDto?> CreateAsync(CreateTransactionDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return null;
            }

            var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);

            if (category == null)
            {
                return null;
            }

            var transaction = new Transaction
            {
                Amount = dto.Amount,
                Description = dto.Description,
                Date = dto.Date,
                Type = dto.Type,
                UserId = dto.UserId,
                CategoryId = dto.CategoryId
            };

            var createdTransaction = await _transactionRepository.CreateAsync(transaction);

            if (dto.Type == TransactionType.Expense)
            {
                await _notificationService.CheckMonthlyBudgetAsync(dto.UserId);
            }

            var completeTransaction = await _transactionRepository.GetByIdAsync(createdTransaction.Id);

            return new TransactionResponseDto
            {
                Id = completeTransaction!.Id,
                Amount = completeTransaction.Amount,
                Description = completeTransaction.Description,
                Date = completeTransaction.Date,
                Type = completeTransaction.Type.ToString(),
                UserId = completeTransaction.UserId,
                CategoryId = completeTransaction.CategoryId,
                CategoryName = completeTransaction.Category.Name
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);

            if (transaction == null)
            {
                return false;
            }

            await _transactionRepository.DeleteAsync(transaction);

            return true;
        }
    }
}