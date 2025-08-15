using System.ComponentModel.DataAnnotations;

namespace jobportal_api.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsClear { get; set; }

      

    }
    public class NotificationUpdateRequest
    {
        public List<int>? NotificationIds { get; set; }
        public bool? IsRead { get; set; }
        public bool? IsClear { get; set; }
    }

}
