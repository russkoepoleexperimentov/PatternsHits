using Common;
using CreditDomain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CreditInfrastructure
{
    public class CreditDbContext : DbContext
    {
        public DbSet<Tariff> Tariffs { get; set; }
        public DbSet<Credit> Credits { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public CreditDbContext(DbContextOptions<CreditDbContext> options) : base(options) { }


        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreateDateTime = DateTime.UtcNow;
                }
                entry.Entity.ModifyDateTime = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
