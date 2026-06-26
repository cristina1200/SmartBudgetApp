using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.SavingGoals
{
    public class AddMoneyToGoalDto
    {
        public decimal Amount { get; set; }

        public SmartBudget.Server.Enums.Currency Currency { get; set; } = SmartBudget.Server.Enums.Currency.RON;
    }
}