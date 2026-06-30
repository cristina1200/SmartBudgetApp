using SmartBudget.Server.DTOs.Notifications;

namespace SmartBudget.Server.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDto>> GetByUserIdAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task<bool> MarkAsReadAsync(int id);

        Task MarkAllAsReadAsync(int userId);

        Task CheckMonthlyBudgetAsync(int userId);
    }
}