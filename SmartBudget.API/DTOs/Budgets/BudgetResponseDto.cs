namespace SmartBudget.Server.DTOs.Budgets
{
    public class BudgetResponseDto
    {
        public int Id { get; set; }

        public decimal MonthlyLimit { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public int UserId { get; set; }
    }
}