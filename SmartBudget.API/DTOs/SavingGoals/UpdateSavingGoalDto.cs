using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.SavingGoals
{
    public class UpdateSavingGoalDto
    {
        public string Name { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }

        public decimal CurrentAmount { get; set; }

        public DateTime? Deadline { get; set; }

        public SmartBudget.Server.Enums.Currency Currency { get; set; } = SmartBudget.Server.Enums.Currency.RON;
    }
}