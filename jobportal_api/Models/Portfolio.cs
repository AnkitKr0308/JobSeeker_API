using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace jobportal_api.Models
{
    public class Portfolio
    {
        [Key]
        public int Id { get; set; }
        public string? PortfolioId { get; set; }

        public string? UserId { get; set; }   

        public string? Bio { get; set; }
        public string? Skills { get; set; }
    }


}
