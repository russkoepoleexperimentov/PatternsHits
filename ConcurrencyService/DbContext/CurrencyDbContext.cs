using ConcurrencyService.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyService.Data;

public class CurrencyDbContext : DbContext
{
    public CurrencyDbContext(DbContextOptions<CurrencyDbContext> options) : base(options) { }

    public DbSet<ExchangeRate> ExchangeRates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExchangeRate>()
            .HasIndex(r => new { r.BaseCurrency, r.TargetCurrency })
            .IsUnique();
    }
}