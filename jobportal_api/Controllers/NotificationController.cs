using jobportal_api.Hubs;
using jobportal_api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace jobportal_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsClear)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(new { success = true, notifications });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotificationStatus([FromBody] NotificationUpdateRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (request.NotificationIds == null || !request.NotificationIds.Any())
                return BadRequest(new { success = false, message = "No notifications specified" });

            var notifications = await _context.Notifications
                .Where(n => request.NotificationIds.Contains(n.Id) && n.UserId == userId)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = request.IsRead ?? n.IsRead;
                n.IsClear = request.IsClear ?? n.IsClear;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Notification updated successfully" });
        }
    }
}
