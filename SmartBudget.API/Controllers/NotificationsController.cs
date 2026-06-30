using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var notifications = await _notificationService.GetByUserIdAsync(userId);

            return Ok(notifications);
        }

        [HttpGet("unread-count/{userId}")]
        public async Task<IActionResult> GetUnreadCount(int userId)
        {
            var count = await _notificationService.GetUnreadCountAsync(userId);

            return Ok(new
            {
                unreadCount = count
            });
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var marked = await _notificationService.MarkAsReadAsync(id);

            if (!marked)
            {
                return NotFound("Notification not found.");
            }

            return NoContent();
        }

        [HttpPatch("user/{userId}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            await _notificationService.MarkAllAsReadAsync(userId);

            return NoContent();
        }

        [HttpPost("check-budget/{userId}")]
        public async Task<IActionResult> CheckMonthlyBudget(int userId)
        {
            await _notificationService.CheckMonthlyBudgetAsync(userId);

            return Ok("Monthly budget checked.");
        }
    }
}