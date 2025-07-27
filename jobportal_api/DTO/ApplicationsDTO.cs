using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class ApplicationsDTO
    {
        [Key]
        public int? Id { get; set; }
        public DateOnly? AppliedDate { get; set; }
        public string? JobId {  get; set; }
        public string? JobTitle { get; set; }
        //public string? JobDescription { get; set; }
        public string? SkillsRequired { get; set; }
        //public string? JobLocations {  get; set; }
        //public string? JobQualifications { get; set; }
        public string? UserId { get; set; }
        //public string? Name { get; set; }
        //public string? Email { get; set; }
        //public string? Gender { get; set; }
        //public string? Contact { get; set; }
        //public string? Bio {  get; set; }
        //public string? PortfolioSkills { get; set; }
        //public string? ProjectID { get; set; }
        //public string? ProjectTitle { get; set; }
        //public string? ProjectDescription { get; set; }
        //public string? ProjectSkills {  get; set; }
        //public string? WorkCompany { get; set; }
        //public DateOnly? WorkFromDate { get; set; }
        //public DateOnly? WorkToDate { get; set; }
        //public string? Status { get; set; }
        public int? NoticePeriod { get; set; }
        public string? CurrentLocation { get; set; }
        public string? ReadyToRelocate { get; set; }
    }
}
