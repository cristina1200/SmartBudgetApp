using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.Categories
{
    public class CreateCategoryDto
    {
        public string Name { get; set; } = string.Empty;

        public TransactionType Type { get; set; }

        public int UserId { get; set; }
    }
}