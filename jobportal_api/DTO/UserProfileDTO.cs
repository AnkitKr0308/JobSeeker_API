using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class UserProfileDTO
    {
        public string? UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Contact { get; set; }
        public string? Role { get; set; }
        public string? Bio { get; set; }
        public string? Skills { get; set; }

        public List<ProjectDTO> Projects { get; set; }
        public List<WorkExDTO> WorkExperiences { get; set; }
    }

    public class ProjectDTO
    {
        public string? ProjectID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Skills { get; set; }
    }

    public class WorkExDTO
    {
        public string? WorkExID { get; set; }
        public string Company { get; set; }
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
    }

}
