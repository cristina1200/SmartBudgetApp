using SmartBudget.Server.Enums;

namespace SmartBudget.Server.Models
{
    public class SavingGoal
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public DateTime? Deadline { get; set; }

        public Currency Currency { get; set; } = Currency.RON;

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public ICollection<SavingGoalContribution> Contributions { get; set; } =
            new List<SavingGoalContribution>();
    }
}