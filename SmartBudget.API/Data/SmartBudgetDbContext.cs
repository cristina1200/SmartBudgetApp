using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Models;

namespace SmartBudget.Server.Data
{
    public class SmartBudgetDbContext : DbContext
    {
        public SmartBudgetDbContext(DbContextOptions<SmartBudgetDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<Budget> Budgets { get; set; }

        public DbSet<SavingGoal> SavingGoals { get; set; }

        public DbSet<SavingGoalContribution> SavingGoalContributions { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Categories)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Budgets)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.SavingGoals)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Category>()
                .HasMany(c => c.Transactions)
                .WithOne(t => t.Category)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SavingGoal>()
                .HasMany(s => s.Contributions)
                .WithOne(c => c.SavingGoal)
                .HasForeignKey(c => c.SavingGoalId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Budget>()
                .Property(b => b.MonthlyLimit)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SavingGoal>()
                .Property(s => s.TargetAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SavingGoal>()
                .Property(s => s.CurrentAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SavingGoalContribution>()
                .Property(c => c.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<SavingGoalContribution>()
                .Property(c => c.OriginalAmount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<User>()
                .HasMany(u => u.Notifications)
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.ReferenceKey });
        }
    }
}