using Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace Core.Infrastructure
{
    public class CoreDbContext : DbContext
    {
        public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Настройки для отношений и индексов
            modelBuilder.Entity<Account>()
                .HasIndex(a => a.UserId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.FromAccountId);

            // Преобразование enum в строку
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Type)
                .HasConversion<string>();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>();
        }
    }
}
