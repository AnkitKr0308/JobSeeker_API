using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.DTO
{
    public class AppliedJobsDTO
    {
        [Key]
        public int? Id { get; set; }
        public string? JobId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Role { get; set; }
        public string? SkillsRequired { get; set; }
        public string? Locations {  get; set; }
        public string? Qualifications { get; set; }
        public string? Type { get; set; }
        public string? Experience { get; set; }
        public string? Status { get; set; }
        public string? UserId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? InterviewDate { get; set; }
        
        public string? ApplicationId { get; set; }
    }
}
