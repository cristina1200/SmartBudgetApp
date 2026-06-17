namespace SmartBudget.Server.DTOs.Budgets
{
    public class CreateBudgetDto
    {
        public decimal MonthlyLimit { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public int UserId { get; set; }
    }
}