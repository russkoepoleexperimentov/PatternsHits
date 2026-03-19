using Common.Enums;
using CreditDomain.Entities;
using CreditInfrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace CreditService.Jobs;

public class OverduePaymentsJob : IJob
{
    private readonly IServiceProvider _serviceProvider;

    public OverduePaymentsJob(IServiceProvider serviceProvider, ILogger<OverduePaymentsJob> logger)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CreditDbContext>();
        var now = DateTime.UtcNow;

        var credits = await dbContext.Credits
            .Where(c => c.Status == CreditStatus.Approved)
            .ToListAsync(context.CancellationToken);

        foreach (var credit in credits)
        {
            var lastPayment = await dbContext.Payments
                .Where(p => p.CreditId == credit.Id)
                .OrderByDescending(p => p.DueDate)
                .FirstOrDefaultAsync(context.CancellationToken);

            if (lastPayment == null) continue;

            if (lastPayment.Status == PaymentStatus.Pending && lastPayment.DueDate < now)
            {
                lastPayment.Status = PaymentStatus.Overdue;

                var createdCount = await dbContext.Payments
                    .CountAsync(p => p.CreditId == credit.Id && (p.Status == PaymentStatus.Processed || p.Status == PaymentStatus.Overdue), context.CancellationToken);

                if (createdCount < credit.TermDays)
                {
                    int remainingDays = credit.TermDays - createdCount;
                    decimal nextAmount = (remainingDays == 1)
                        ? credit.RemainingDebt
                        : Math.Round(credit.RemainingDebt / remainingDays, 2);

                    var nextPayment = new Payment
                    {
                        CreditId = credit.Id,
                        Amount = nextAmount,
                        DueDate = now.AddHours(1),
                        Status = PaymentStatus.Pending,
                        CreateDateTime = now
                    };
                    dbContext.Payments.Add(nextPayment);
                }
            }
        }

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}