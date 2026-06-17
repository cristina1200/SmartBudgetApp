using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.Transactions
{
    public class CreateTransactionDto
    {
        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }

        public int UserId { get; set; }

        public int CategoryId { get; set; }
    }
}