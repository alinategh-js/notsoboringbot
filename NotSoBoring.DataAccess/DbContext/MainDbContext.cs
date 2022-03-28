using NotSoBoring.Core.Models;
using Microsoft.EntityFrameworkCore;
using NotSoBoring.Domain.Models;

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
            modelBuilder.Entity<Contact>()
                .HasOne(c => c.ContactUser)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithOne()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contact>().HasKey(c => new { c.UserId, c.ContactId });
        }
       
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    }
}
