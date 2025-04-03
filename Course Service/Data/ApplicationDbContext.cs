using Course_Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Course_Service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Course> Courses { get; set; }  
    }
}
