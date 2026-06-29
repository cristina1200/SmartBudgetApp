using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.SavingGoals
{
    public class SavingGoalDetailsDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal RemainingAmount { get; set; }

        public decimal ProgressPercentage { get; set; }

        public DateTime? Deadline { get; set; }

        public Currency Currency { get; set; }

        public int UserId { get; set; }

        public decimal TotalContributed { get; set; }

        public decimal AverageWeeklySaving { get; set; }

        public decimal AverageMonthlySaving { get; set; }

        public int? EstimatedWeeksToComplete { get; set; }

        public DateTime? EstimatedCompletionDate { get; set; }

        public decimal? RecommendedWeeklyContribution { get; set; }

        public string SmartMessage { get; set; } = string.Empty;

        public List<SavingGoalContributionResponseDto> Contributions { get; set; } = new();
    }
}