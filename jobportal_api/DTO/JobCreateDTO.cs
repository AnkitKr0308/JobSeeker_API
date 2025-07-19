using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class JobCreateDTO
    {

      
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string SkillsRequired { get; set; }
        public string Qualifications { get; set; }
    }
}
