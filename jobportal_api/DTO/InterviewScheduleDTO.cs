using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class InterviewScheduleDTO
    {
        [Key]
        public int Id { get; set; }
        public string JobId { get; set; }
        public DateTime InterviewDate { get; set; }
        public string UserId { get; set; }
        public string? ApplicationId { get; set; }

    }
}
