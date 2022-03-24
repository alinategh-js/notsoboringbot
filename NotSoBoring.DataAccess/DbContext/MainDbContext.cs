using NotSoBoring.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace NotSoBoring.DataAccess
{
    public class MainDbContext : DbContext
    {
        public MainDbContext() { }

        public MainDbContext(DbContextOptions<MainDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
       
        public DbSet<ApplicationUser> Users { get; set; }
    }
}
