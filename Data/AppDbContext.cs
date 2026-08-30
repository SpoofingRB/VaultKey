using Microsoft.EntityFrameworkCore;
using appointmentapi.Models.AuthEntity;
using appointmentapi.Models.VaultEntity;

namespace appointmentapi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<VaultEntry> VaultEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<VaultEntry>()
                .HasOne(v => v.User)
                .WithMany(u => u.VaultEntries)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}