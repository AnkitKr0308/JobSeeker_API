using Microsoft.EntityFrameworkCore;
using jobportal_api.Models;
using jobportal_api.DTO;


namespace jobportal_api
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<AppliedJobs> AppliedJobs { get; set; }

    }
    
}
