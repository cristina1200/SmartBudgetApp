using SmartBudget.Server.DTOs.SavingGoals;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class SavingGoalService : ISavingGoalService
    {
        private readonly ISavingGoalRepository _savingGoalRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrencyConverterService _currencyConverterService;

        public SavingGoalService(
            ISavingGoalRepository savingGoalRepository,
            IUserRepository userRepository,
            ICurrencyConverterService currencyConverterService)
        {
            _savingGoalRepository = savingGoalRepository;
            _userRepository = userRepository;
            _currencyConverterService = currencyConverterService;
        }

        public async Task<List<SavingGoalResponseDto>> GetAllAsync()
        {
            var goals = await _savingGoalRepository.GetAllAsync();

            return goals.Select(ToResponseDto).ToList();
        }

        public async Task<List<SavingGoalResponseDto>> GetByUserIdAsync(int userId)
        {
            var goals = await _savingGoalRepository.GetByUserIdAsync(userId);

            return goals.Select(ToResponseDto).ToList();
        }

        public async Task<SavingGoalResponseDto?> GetByIdAsync(int id)
        {
            var goal = await _savingGoalRepository.GetByIdAsync(id);

            if (goal == null)
            {
                return null;
            }

            return ToResponseDto(goal);
        }

        public async Task<SavingGoalResponseDto?> CreateAsync(CreateSavingGoalDto dto)
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return null;
            }

            if (dto.TargetAmount <= 0)
            {
                throw new Exception("Target amount must be greater than 0.");
            }

            if (dto.CurrentAmount < 0)
            {
                throw new Exception("Current amount cannot be negative.");
            }

            var goal = new SavingGoal
            {
                Name = dto.Name,
                TargetAmount = dto.TargetAmount,
                CurrentAmount = dto.CurrentAmount,
                Deadline = dto.Deadline,
                UserId = dto.UserId,
                Currency = dto.Currency
            };

            var createdGoal = await _savingGoalRepository.CreateAsync(goal);

            return ToResponseDto(createdGoal);
        }

        public async Task<SavingGoalResponseDto?> UpdateAsync(int id, UpdateSavingGoalDto dto)
        {
            var goal = await _savingGoalRepository.GetByIdAsync(id);

            if (goal == null)
            {
                return null;
            }

            if (dto.TargetAmount <= 0)
            {
                throw new Exception("Target amount must be greater than 0.");
            }

            if (dto.CurrentAmount < 0)
            {
                throw new Exception("Current amount cannot be negative.");
            }

            goal.Name = dto.Name;
            goal.TargetAmount = dto.TargetAmount;
            goal.CurrentAmount = dto.CurrentAmount;
            goal.Deadline = dto.Deadline;
            goal.Currency = dto.Currency;

            var updatedGoal = await _savingGoalRepository.UpdateAsync(goal);

            return ToResponseDto(updatedGoal);
        }

        public async Task<SavingGoalResponseDto?> AddMoneyAsync(int id, AddMoneyToGoalDto dto)
        {
            var goal = await _savingGoalRepository.GetByIdAsync(id);

            if (goal == null)
            {
                return null;
            }

            if (dto.Amount <= 0)
            {
                throw new Exception("Amount must be greater than 0.");
            }

            var convertedAmount = await _currencyConverterService.ConvertAsync(
                dto.Amount,
                dto.Currency,
                goal.Currency);

            goal.CurrentAmount += convertedAmount;

            if (goal.CurrentAmount > goal.TargetAmount)
            {
                goal.CurrentAmount = goal.TargetAmount;
            }

            var updatedGoal = await _savingGoalRepository.UpdateAsync(goal);

            return ToResponseDto(updatedGoal);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var goal = await _savingGoalRepository.GetByIdAsync(id);

            if (goal == null)
            {
                return false;
            }

            await _savingGoalRepository.DeleteAsync(goal);

            return true;
        }

        private static SavingGoalResponseDto ToResponseDto(SavingGoal goal)
        {
            var progress = goal.TargetAmount == 0
                ? 0
                : Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2);

            return new SavingGoalResponseDto
            {
                Id = goal.Id,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                ProgressPercentage = progress,
                Deadline = goal.Deadline,
                UserId = goal.UserId,
                Currency = goal.Currency
            };
        }
    }
}