using Microsoft.EntityFrameworkCore;
using User.API.Models;

namespace User.API.Data
{
    public class UserContext : DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }

        public DbSet<UserProperty> UserProperties { get; set; }

        public DbSet<UserTag> UserTags { get; set; }

        public DbSet<BPFile> BpFiles { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AppUser>()
                .ToTable("Users")
                .HasKey(u => u.Id);

            modelBuilder.Entity<UserProperty>().Property(u => u.Key).HasMaxLength(100);
            modelBuilder.Entity<UserProperty>().Property(u => u.Value).HasMaxLength(100);
            modelBuilder.Entity<UserProperty>()
                .ToTable("UserProperties")
                .HasKey(u => new { u.Key, u.AppUserId, u.Value });

            modelBuilder.Entity<UserTag>().ToTable("UserTags")
                .HasKey(b => new { b.UserId, b.Tag });

            modelBuilder.Entity<UserTag>()
                .Property(u => u.Tag).HasMaxLength(100);

            modelBuilder.Entity<BPFile>()
                .ToTable("UserBPFiles")
                .HasKey(p => p.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}