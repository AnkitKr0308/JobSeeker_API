using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class AppliedJobs
    {
        [Key]
        public int? ID { get; set; }
        [Required]
        public string? JobId { get; set; }
        
        [Required]
        public string? UserId {  get; set; }
        public int? NoticePeriod { get; set; }
        public string? ReadyToRelocate{ get; set; }
        public string? CurrentLocation { get; set; }
        public string? Status { get; set; }
        public DateOnly? AppliedDate { get; set; }
        public DateTime? InterviewDate { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string? ApplicationId { get; set; }

    }
}
