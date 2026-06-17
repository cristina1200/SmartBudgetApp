using SmartBudget.Server.DTOs.Budgets;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;
        private readonly IUserRepository _userRepository;

        public BudgetService(
            IBudgetRepository budgetRepository,
            IUserRepository userRepository)
        {
            _budgetRepository = budgetRepository;
            _userRepository = userRepository;
        }

        public async Task<List<BudgetResponseDto>> GetAllAsync()
        {
            var budgets = await _budgetRepository.GetAllAsync();

            return budgets.Select(b => new BudgetResponseDto
            {
                Id = b.Id,
                MonthlyLimit = b.MonthlyLimit,
                Month = b.Month,
                Year = b.Year,
                UserId = b.UserId
            }).ToList();
        }

        public async Task<List<BudgetResponseDto>> GetByUserIdAsync(int userId)
        {
            var budgets = await _budgetRepository.GetByUserIdAsync(userId);

            return budgets.Select(b => new BudgetResponseDto
            {
                Id = b.Id,
                MonthlyLimit = b.MonthlyLimit,
                Month = b.Month,
                Year = b.Year,
                UserId = b.UserId
            }).ToList();
        }

        public async Task<BudgetResponseDto?> GetByIdAsync(int id)
        {
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null)
            {
                return null;
            }

            return new BudgetResponseDto
            {
                Id = budget.Id,
                MonthlyLimit = budget.MonthlyLimit,
                Month = budget.Month,
                Year = budget.Year,
                UserId = budget.UserId
            };
        }

        public async Task<BudgetResponseDto?> GetCurrentBudgetAsync(int userId)
        {
            var currentDate = DateTime.Now;

            var budget = await _budgetRepository.GetCurrentBudgetAsync(
                userId,
                currentDate.Month,
                currentDate.Year);

            if (budget == null)
            {
                return null;
            }

            return new BudgetResponseDto
            {
                Id = budget.Id,
                MonthlyLimit = budget.MonthlyLimit,
                Month = budget.Month,
                Year = budget.Year,
                UserId = budget.UserId
            };
        }

        public async Task<BudgetResponseDto?> CreateAsync(CreateBudgetDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return null;
            }

            if (dto.Month < 1 || dto.Month > 12)
            {
                throw new Exception("Month must be between 1 and 12.");
            }

            if (dto.MonthlyLimit <= 0)
            {
                throw new Exception("Monthly limit must be greater than 0.");
            }

            var existingBudget = await _budgetRepository.GetCurrentBudgetAsync(
                dto.UserId,
                dto.Month,
                dto.Year);

            if (existingBudget != null)
            {
                throw new Exception("A budget already exists for this user, month and year.");
            }

            var budget = new Budget
            {
                MonthlyLimit = dto.MonthlyLimit,
                Month = dto.Month,
                Year = dto.Year,
                UserId = dto.UserId
            };

            var createdBudget = await _budgetRepository.CreateAsync(budget);

            return new BudgetResponseDto
            {
                Id = createdBudget.Id,
                MonthlyLimit = createdBudget.MonthlyLimit,
                Month = createdBudget.Month,
                Year = createdBudget.Year,
                UserId = createdBudget.UserId
            };
        }

        public async Task<BudgetResponseDto?> UpdateAsync(int id, UpdateBudgetDto dto)
        {
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null)
            {
                return null;
            }

            if (dto.Month < 1 || dto.Month > 12)
            {
                throw new Exception("Month must be between 1 and 12.");
            }

            if (dto.MonthlyLimit <= 0)
            {
                throw new Exception("Monthly limit must be greater than 0.");
            }

            budget.MonthlyLimit = dto.MonthlyLimit;
            budget.Month = dto.Month;
            budget.Year = dto.Year;

            var updatedBudget = await _budgetRepository.UpdateAsync(budget);

            return new BudgetResponseDto
            {
                Id = updatedBudget.Id,
                MonthlyLimit = updatedBudget.MonthlyLimit,
                Month = updatedBudget.Month,
                Year = updatedBudget.Year,
                UserId = updatedBudget.UserId
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var budget = await _budgetRepository.GetByIdAsync(id);

            if (budget == null)
            {
                return false;
            }

            await _budgetRepository.DeleteAsync(budget);

            return true;
        }
    }
}