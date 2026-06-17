namespace SmartBudget.Server.DTOs.Categories
{
    public class CategoryResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public int UserId { get; set; }
    }
}