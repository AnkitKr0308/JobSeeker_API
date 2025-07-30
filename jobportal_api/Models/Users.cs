using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }            

        public string UserId { get; set; }    

        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public string Gender { get; set; }
        public string Contact { get; set; }

        public string? Bio {  get; set; }
        public string? Skills { get; set; }
        public DateTime CreatedAt { get; set; }

    }
}
