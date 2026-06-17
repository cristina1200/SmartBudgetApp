namespace SmartBudget.Server.DTOs.Budgets
{
    public class UpdateBudgetDto
    {
        public decimal MonthlyLimit { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }
    }
}