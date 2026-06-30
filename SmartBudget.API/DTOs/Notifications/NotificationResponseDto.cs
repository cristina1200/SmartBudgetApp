using SmartBudget.Server.Enums;

namespace SmartBudget.Server.DTOs.Notifications
{
    public class NotificationResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
    }
}