using SmartBudget.Server.Enums;

namespace SmartBudget.Server.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        public ICollection<Budget> Budgets { get; set; } = new List<Budget>();

        public ICollection<SavingGoal> SavingGoals { get; set; } = new List<SavingGoal>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    }
}