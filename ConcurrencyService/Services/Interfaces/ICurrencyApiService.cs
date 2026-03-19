namespace ConcurrencyService.Services.Interfaces
{
    public interface ICurrencyApiService
    {
        Task<Dictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency);
    }
}
