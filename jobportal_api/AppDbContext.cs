using Microsoft.EntityFrameworkCore;
using jobportal_api.Models;


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

    }
    
}
