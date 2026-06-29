using SmartBudget.Server.Enums;

namespace SmartBudget.Server.Models
{
    public class SavingGoalContribution
    {
        public int Id { get; set; }

        public int SavingGoalId { get; set; }

        public decimal Amount { get; set; }

        public decimal OriginalAmount { get; set; }

        public Currency Currency { get; set; } = Currency.RON;

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public SavingGoal SavingGoal { get; set; } = null!;
    }
}