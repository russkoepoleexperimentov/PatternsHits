using ConcurrencyService.Models;
using ConcurrencyService.Services.Interfaces;
using CurrencyService.Data;
using CurrencyService.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace CurrencyService.Jobs;

public class ExchangeRateUpdateJob : IJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public ExchangeRateUpdateJob(IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CurrencyDbContext>();
        var apiService = scope.ServiceProvider.GetRequiredService<ICurrencyApiService>();

        var supportedCurrencies = _configuration.GetSection("CurrencyApi:SupportedCurrencies").Get<List<string>>();
        if (supportedCurrencies == null || supportedCurrencies.Count == 0)
        {
            return;
        }

        foreach (var baseCurrency in supportedCurrencies)
        {
            try
            {
                var rates = await apiService.GetLatestRatesAsync(baseCurrency);

                var ratesToUpdate = rates
                    .Where(rate => supportedCurrencies.Contains(rate.Key) && rate.Key != baseCurrency)
                    .ToList();

                foreach (var (targetCurrency, rateValue) in ratesToUpdate)
                {
                    var existing = await dbContext.ExchangeRates
                        .FirstOrDefaultAsync(r => r.BaseCurrency == baseCurrency && r.TargetCurrency == targetCurrency);

                    if (existing == null)
                    {
                        dbContext.ExchangeRates.Add(new ExchangeRate
                        {
                            BaseCurrency = baseCurrency,
                            TargetCurrency = targetCurrency,
                            Rate = rateValue,
                            LastUpdated = DateTime.UtcNow
                        });
                    }
                    else
                    {
                        existing.Rate = rateValue;
                        existing.LastUpdated = DateTime.UtcNow;
                    }
                }

                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
            }
        }
    }
}