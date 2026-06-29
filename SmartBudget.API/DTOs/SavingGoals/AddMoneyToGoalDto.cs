using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.SavingGoals
{
    public class AddMoneyToGoalDto
    {
        public decimal Amount { get; set; }

        public Currency Currency { get; set; } = Currency.RON;

        public string? Note { get; set; }
    }
}