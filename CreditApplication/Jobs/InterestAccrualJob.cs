using CreditInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CreditService.Jobs;

public class InterestAccrualJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InterestAccrualJob> _logger;

    public InterestAccrualJob(IServiceProvider serviceProvider, ILogger<InterestAccrualJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CreditDbContext>();

        var openCredits = await dbContext.Credits
            .Include(c => c.Tariff)
            .Where(c => c.Status == Common.Enums.CreditStatus.Approved)
            .ToListAsync(context.CancellationToken);

        if (!openCredits.Any())
        {
            return;
        }

        foreach (var credit in openCredits)
        {
            credit.RemainingDebt *= 1 + credit.Tariff.InterestRate / 100m;
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}