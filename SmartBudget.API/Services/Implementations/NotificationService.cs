using SmartBudget.Server.DTOs.Notifications;
using SmartBudget.Server.Enums;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IBudgetRepository _budgetRepository;
        private readonly ITransactionRepository _transactionRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IBudgetRepository budgetRepository,
            ITransactionRepository transactionRepository)
        {
            _notificationRepository = notificationRepository;
            _budgetRepository = budgetRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<List<NotificationResponseDto>> GetByUserIdAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            return notifications.Select(ToResponseDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadCountAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);

            if (notification == null)
            {
                return false;
            }

            await _notificationRepository.MarkAsReadAsync(notification);

            return true;
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task CheckMonthlyBudgetAsync(int userId)
        {
            var today = DateTime.Now;
            var month = today.Month;
            var year = today.Year;

            var budget = await _budgetRepository.GetCurrentBudgetAsync(
                userId,
                month,
                year);

            if (budget == null || budget.MonthlyLimit <= 0)
            {
                return;
            }

            var transactions = await _transactionRepository.GetByUserIdAsync(userId);

            var currentMonthExpenses = transactions
                .Where(t =>
                    t.Type == TransactionType.Expense &&
                    t.Date.Month == month &&
                    t.Date.Year == year)
                .Sum(t => t.Amount);

            if (currentMonthExpenses <= 0)
            {
                return;
            }

            var percentage = budget.MonthlyLimit <= 0
                ? 0
                : Math.Round((currentMonthExpenses / budget.MonthlyLimit) * 100, 2);

            var monthKey = $"{year}-{month:00}";

            if (percentage >= 100)
            {
                var referenceKey = $"monthly-budget-danger-{userId}-{monthKey}";

                var alreadyExists = await _notificationRepository.ExistsByReferenceKeyAsync(
                    userId,
                    referenceKey);

                if (alreadyExists)
                {
                    return;
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Monthly budget exceeded",
                    Message = $"You exceeded your monthly budget. Expenses: {currentMonthExpenses:0.00} RON / Budget: {budget.MonthlyLimit:0.00} RON.",
                    Type = NotificationType.Danger,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ReferenceKey = referenceKey
                };

                await _notificationRepository.CreateAsync(notification);

                return;
            }

            if (percentage >= 80)
            {
                var referenceKey = $"monthly-budget-warning-{userId}-{monthKey}";

                var alreadyExists = await _notificationRepository.ExistsByReferenceKeyAsync(
                    userId,
                    referenceKey);

                if (alreadyExists)
                {
                    return;
                }

                var notification = new Notification
                {
                    UserId = userId,
                    Title = "Budget warning",
                    Message = $"Careful: you used {percentage:0}% of your monthly budget. Expenses: {currentMonthExpenses:0.00} RON / Budget: {budget.MonthlyLimit:0.00} RON.",
                    Type = NotificationType.Warning,
                    IsRead = false,
                    CreatedAt = DateTime.Now,
                    ReferenceKey = referenceKey
                };

                await _notificationRepository.CreateAsync(notification);
            }
        }

        private static NotificationResponseDto ToResponseDto(Notification notification)
        {
            return new NotificationResponseDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                UserId = notification.UserId
            };
        }
    }
}