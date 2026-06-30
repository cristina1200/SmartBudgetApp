using Microsoft.EntityFrameworkCore;
using SmartBudget.Server.Data;
using SmartBudget.Server.Models;
using SmartBudget.Server.Repositories.Interfaces;

namespace SmartBudget.Server.Repositories.Implementations
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SmartBudgetDbContext _context;

        public NotificationRepository(SmartBudgetDbContext context)
        {
            _context = context;
        }

        public async Task<List<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task<Notification> CreateAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<bool> ExistsByReferenceKeyAsync(int userId, string referenceKey)
        {
            return await _context.Notifications
                .AnyAsync(n =>
                    n.UserId == userId &&
                    n.ReferenceKey == referenceKey);
        }

        public async Task MarkAsReadAsync(Notification notification)
        {
            notification.IsRead = true;

            _context.Notifications.Update(notification);

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}