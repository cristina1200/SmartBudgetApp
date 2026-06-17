using SmartBudget.Server.Enums;

namespace SmartBudget.Server.Models
{
    public class Transaction
    {
        public int Id { get; set; }

        public decimal Amount { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public TransactionType Type { get; set; }

        public int UserId { get; set; }

        public User User { get; set; } = null!;

        public int CategoryId { get; set; }

        public Category Category { get; set; } = null!;
    }
}