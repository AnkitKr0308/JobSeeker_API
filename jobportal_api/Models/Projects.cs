using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class Projects
    {
        [Key]
        public int Id { get; set; }

        public string ProjectID { get; set; }
        public string UserId { get; set; }    // Link to Users.UserId (string)

        public string Title { get; set; }
        public string Description { get; set; }
        public string Skills { get; set; }
    }

}
