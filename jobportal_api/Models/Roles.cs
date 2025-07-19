using System.ComponentModel.DataAnnotations;

namespace jobportal_api.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }
        public string Role { get; set; }
    }
}
