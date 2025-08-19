using jobportal_api.Hubs;
using jobportal_api.Models;
using Microsoft.AspNetCore.SignalR;

namespace jobportal_api.Services
{
    public class NotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task SendNotification(string userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                CreatedAt = DateTime.Now,
                IsRead = false,
                IsClear = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send real-time via SignalR
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", message);
        }
    }

}