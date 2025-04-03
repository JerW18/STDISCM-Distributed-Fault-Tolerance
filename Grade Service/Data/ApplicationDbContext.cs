using Grade_Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Grade_Service.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<GradeModel> Grades { get; set; }
    }
}
