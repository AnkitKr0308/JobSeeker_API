using Microsoft.EntityFrameworkCore;
using jobportal_api.Models;
using jobportal_api.DTO;
using jobportal_api.Hubs;


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
        public DbSet<ApplicationsDTO> Applications { get; set; }
        public DbSet<AppliedJobsDTO> AppliedJobsDTO { get; set; }
        public DbSet<Portfolio> Portfolio { get; set; }      
        public DbSet<Projects> Projects { get; set; }         
        public DbSet<WorkEx> WorkEx { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        

    }
    
}
