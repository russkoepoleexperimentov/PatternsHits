using Microsoft.EntityFrameworkCore;
using MonitoringService.Entities;

namespace MonitoringService.Data
{
    public class MonitoringDbContext : DbContext
    {
        public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options) { }

        public DbSet<TraceEvent> TraceEvents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TraceEvent>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Id).ValueGeneratedOnAdd();
                e.HasIndex(x => x.Timestamp);
                e.HasIndex(x => x.ServiceName);
                e.HasIndex(x => new { x.ServiceName, x.Timestamp });
            });
        }
    }
}
