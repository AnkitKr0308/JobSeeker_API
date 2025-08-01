using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class ApplicationsDTO
    {
        [Key]
        public int? Id { get; set; }
        public DateOnly? AppliedDate { get; set; }
        public string? JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobDescription { get; set; }
        
        public string? SkillsRequired { get; set; }
        public string? JobLocations {  get; set; }
        public string? JobQualifications { get; set; }
        public string? JobExperience { get; set; }
        public string? UserId { get; set; }
        public string? Status { get; set; }
        public int? NoticePeriod { get; set; }
        public string? CurrentLocation { get; set; }
        public string? ReadyToRelocate { get; set; }
    }
}