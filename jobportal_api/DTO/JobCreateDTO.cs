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
     
        [Required]
        public string Role { get; set; }
        public string Locations { get; set; }
        public string Type { get; set; }
        public string Experience { get; set; }
        //[Required]
        //public string CreatedBy { get; set; }
    }
}
