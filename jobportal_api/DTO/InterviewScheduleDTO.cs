namespace jobportal_api.DTO
{
    public class InterviewScheduleDTO
    {
        public int ApplicationId { get; set; }
        public string JobId { get; set; }
        public DateTime InterviewDate { get; set; }
        public string UserId { get; set; }
        
    }
}
