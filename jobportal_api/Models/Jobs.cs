using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class Jobs
    {
        [Key]
        public int Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string JobId { get;  set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string SkillsRequired { get; set; }
        public string Qualifications {  get; set; }
        [Required]
        public string Role {get; set; }
        public string Locations { get; set; }
        public string Type { get; set; }
        public string Experience { get; set; }
        [Required]
        public string CreatedBy { get; set; }
    }
}
