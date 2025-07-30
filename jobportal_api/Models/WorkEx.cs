using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class WorkEx
    {
        [Key]
        public int Id { get; set; }

        public string WorkExID { get; set; }
        public string UserId { get; set; }    

        public string Company { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
