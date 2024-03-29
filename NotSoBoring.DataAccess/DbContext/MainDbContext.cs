﻿using NotSoBoring.Core.Models;
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
            modelBuilder.Entity<ApplicationContact>().HasOne(x => x.User).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationContact>().HasOne(x => x.ContactUser).WithMany().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ApplicationContact>().HasKey(c => new { c.UserId, c.ContactId });
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ApplicationContact> Contacts { get; set; }
    }
}
