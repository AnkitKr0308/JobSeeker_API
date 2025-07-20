using System.ComponentModel.DataAnnotations;

namespace jobportal_api.DTO
{
    public class UserCreateDTO
    {
     
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        [Required]
        public string Gender { get; set; }
        [Phone]
        public string Contact { get; set; }
    }
}
