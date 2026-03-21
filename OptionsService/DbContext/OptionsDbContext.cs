using Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Context
{
    public class OptionsDbContext : DbContext
    {
        public OptionsDbContext(DbContextOptions<OptionsDbContext> options) : base(options) { }
        public DbSet<UserOptions> UserOptions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserOptions>(entity =>
            {
                entity.HasIndex(e => e.UserId).IsUnique();

                entity.Property(e => e.WebTheme)
                    .HasConversion<string>();

                entity.Property(e => e.MobileTheme)
                    .HasConversion<string>();
            });
        }
    }
}
