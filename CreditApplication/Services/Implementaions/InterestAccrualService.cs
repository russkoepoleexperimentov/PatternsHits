using CreditInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class InterestAccrualService : BackgroundService
{
    private readonly IServiceProvider _services;

    public InterestAccrualService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AccrueInterestAsync(stoppingToken);
            }
            catch (Exception ex)
            {
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task AccrueInterestAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CreditDbContext>();

        var openCredits = await dbContext.Credits
            .Where(c => c.Status == Common.Enums.CreditStatus.Approved)
            .ToListAsync(cancellationToken);

        if (!openCredits.Any())
        {
            return;
        }

        foreach (var credit in openCredits)
        {
            credit.RemainingDebt *= 1 + credit.Tariff.InterestRate / 100m;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}