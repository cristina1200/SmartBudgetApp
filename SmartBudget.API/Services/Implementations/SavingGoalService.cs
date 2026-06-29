using SmartBudget.Server.DTOs.SavingGoals;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class SavingGoalService : ISavingGoalService
    {
        private readonly ISavingGoalRepository _savingGoalRepository;
        private readonly ISavingGoalContributionRepository _contributionRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICurrencyConverterService _currencyConverterService;

        public SavingGoalService(
            ISavingGoalRepository savingGoalRepository,
            ISavingGoalContributionRepository contributionRepository,
            IUserRepository userRepository,
            ICurrencyConverterService currencyConverterService)
        {
            _savingGoalRepository = savingGoalRepository;
            _contributionRepository = contributionRepository;
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

        public async Task<SavingGoalDetailsDto?> GetDetailsAsync(int id)
        {
            var goal = await _savingGoalRepository.GetByIdAsync(id);

            if (goal == null)
            {
                return null;
            }

            var contributions = await _contributionRepository.GetBySavingGoalIdAsync(id);

            return ToDetailsDto(goal, contributions);
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

            if (dto.CurrentAmount > dto.TargetAmount)
            {
                dto.CurrentAmount = dto.TargetAmount;
            }

            var goal = new SavingGoal
            {
                Name = dto.Name.Trim(),
                TargetAmount = dto.TargetAmount,
                CurrentAmount = dto.CurrentAmount,
                Deadline = dto.Deadline,
                UserId = dto.UserId,
                Currency = dto.Currency
            };

            var createdGoal = await _savingGoalRepository.CreateAsync(goal);

            if (createdGoal.CurrentAmount > 0)
            {
                var initialContribution = new SavingGoalContribution
                {
                    SavingGoalId = createdGoal.Id,
                    Amount = createdGoal.CurrentAmount,
                    OriginalAmount = createdGoal.CurrentAmount,
                    Currency = createdGoal.Currency,
                    Note = "Initial amount",
                    CreatedAt = DateTime.Now
                };

                await _contributionRepository.CreateAsync(initialContribution);
            }

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

            if (dto.CurrentAmount > dto.TargetAmount)
            {
                dto.CurrentAmount = dto.TargetAmount;
            }

            goal.Name = dto.Name.Trim();
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

            var remainingAmount = goal.TargetAmount - goal.CurrentAmount;

            if (remainingAmount <= 0)
            {
                throw new Exception("This saving goal is already completed.");
            }

            if (convertedAmount > remainingAmount)
            {
                convertedAmount = remainingAmount;
            }

            goal.CurrentAmount += convertedAmount;

            if (goal.CurrentAmount > goal.TargetAmount)
            {
                goal.CurrentAmount = goal.TargetAmount;
            }

            var contribution = new SavingGoalContribution
            {
                SavingGoalId = goal.Id,
                Amount = convertedAmount,
                OriginalAmount = dto.Amount,
                Currency = dto.Currency,
                Note = dto.Note,
                CreatedAt = DateTime.Now
            };

            await _contributionRepository.CreateAsync(contribution);

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
            var progress = goal.TargetAmount <= 0
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

        private static SavingGoalDetailsDto ToDetailsDto(
            SavingGoal goal,
            List<SavingGoalContribution> contributions)
        {
            var remainingAmount = Math.Max(0, goal.TargetAmount - goal.CurrentAmount);

            var progress = goal.TargetAmount <= 0
                ? 0
                : Math.Round((goal.CurrentAmount / goal.TargetAmount) * 100, 2);

            var orderedContributions = contributions
                .OrderBy(c => c.CreatedAt)
                .ToList();

            var totalContributed = orderedContributions.Sum(c => c.Amount);

            decimal averageWeeklySaving = 0;
            decimal averageMonthlySaving = 0;

            if (orderedContributions.Count > 0)
            {
                var firstDate = orderedContributions.First().CreatedAt.Date;
                var today = DateTime.Now.Date;

                var daysPassed = Math.Max(1, (today - firstDate).Days + 1);
                var weeksPassed = Math.Max(1, daysPassed / 7m);

                averageWeeklySaving = Math.Round(totalContributed / weeksPassed, 2);
                averageMonthlySaving = Math.Round(averageWeeklySaving * 4.345m, 2);
            }

            int? estimatedWeeksToComplete = null;
            DateTime? estimatedCompletionDate = null;

            if (remainingAmount <= 0)
            {
                estimatedWeeksToComplete = 0;
                estimatedCompletionDate = DateTime.Now.Date;
            }
            else if (averageWeeklySaving > 0)
            {
                estimatedWeeksToComplete =
                    (int)Math.Ceiling(remainingAmount / averageWeeklySaving);

                estimatedCompletionDate =
                    DateTime.Now.Date.AddDays(estimatedWeeksToComplete.Value * 7);
            }

            decimal? recommendedWeeklyContribution = null;

            if (goal.Deadline.HasValue && remainingAmount > 0)
            {
                var daysUntilDeadline =
                    Math.Max(1, (goal.Deadline.Value.Date - DateTime.Now.Date).Days);

                var weeksUntilDeadline =
                    Math.Max(1, daysUntilDeadline / 7m);

                recommendedWeeklyContribution =
                    Math.Round(remainingAmount / weeksUntilDeadline, 2);
            }

            return new SavingGoalDetailsDto
            {
                Id = goal.Id,
                Name = goal.Name,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = goal.CurrentAmount,
                RemainingAmount = remainingAmount,
                ProgressPercentage = progress,
                Deadline = goal.Deadline,
                Currency = goal.Currency,
                UserId = goal.UserId,
                TotalContributed = totalContributed,
                AverageWeeklySaving = averageWeeklySaving,
                AverageMonthlySaving = averageMonthlySaving,
                EstimatedWeeksToComplete = estimatedWeeksToComplete,
                EstimatedCompletionDate = estimatedCompletionDate,
                RecommendedWeeklyContribution = recommendedWeeklyContribution,
                SmartMessage = BuildSmartMessage(
                    goal,
                    remainingAmount,
                    progress,
                    averageWeeklySaving,
                    estimatedWeeksToComplete),
                Contributions = contributions
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new SavingGoalContributionResponseDto
                    {
                        Id = c.Id,
                        SavingGoalId = c.SavingGoalId,
                        Amount = c.Amount,
                        OriginalAmount = c.OriginalAmount,
                        Currency = c.Currency,
                        Note = c.Note,
                        CreatedAt = c.CreatedAt
                    })
                    .ToList()
            };
        }

        private static string BuildSmartMessage(
            SavingGoal goal,
            decimal remainingAmount,
            decimal progress,
            decimal averageWeeklySaving,
            int? estimatedWeeks)
        {
            if (remainingAmount <= 0)
            {
                return $"Congratulations! You completed your goal: {goal.Name}.";
            }

            if (averageWeeklySaving <= 0)
            {
                return $"You completed {progress:0}% of this goal. Add a few contributions to receive a better prediction.";
            }

            if (estimatedWeeks.HasValue)
            {
                return $"At your current pace, you can complete this goal in about {estimatedWeeks.Value} week(s).";
            }

            return $"You still need to save {remainingAmount:0.00} {goal.Currency}. Keep going!";
        }
    }
}