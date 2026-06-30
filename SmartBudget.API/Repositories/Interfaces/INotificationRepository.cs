using SmartBudget.Server.Models;

namespace SmartBudget.Server.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<List<Notification>> GetByUserIdAsync(int userId);

        Task<int> GetUnreadCountAsync(int userId);

        Task<Notification?> GetByIdAsync(int id);

        Task<Notification> CreateAsync(Notification notification);

        Task<bool> ExistsByReferenceKeyAsync(int userId, string referenceKey);

        Task MarkAsReadAsync(Notification notification);

        Task MarkAllAsReadAsync(int userId);
    }
}