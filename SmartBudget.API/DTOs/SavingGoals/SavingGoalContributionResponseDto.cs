using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.SavingGoals
{
    public class SavingGoalContributionResponseDto
    {
        public int Id { get; set; }

        public int SavingGoalId { get; set; }

        public decimal Amount { get; set; }

        public decimal OriginalAmount { get; set; }

        public Currency Currency { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}