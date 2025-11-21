using CMCSWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CMCSWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Claim> Claims => Set<Claim>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany(u => u.Claims)
                .HasForeignKey(c => c.UserId);

            // Seed data if needed (you'll need to use UserManager in practice)
        }
    }
}