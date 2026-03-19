using CreditApplication.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace CreditApplication.Services.Implementations
{
    public class CurrencyRateService : ICurrencyRateService
    {
        private readonly HttpClient _httpClient;

        public CurrencyRateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == toCurrency) return 1m;

            try
            {
                var response = await _httpClient.GetAsync($"/api/rates/{fromCurrency}/{toCurrency}");
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<RateResponse>();
                return result.Rate;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Unable to get exchange rate from {fromCurrency} to {toCurrency}");
            }
        }

        private class RateResponse
        {
            public decimal Rate { get; set; }
            public DateTime LastUpdated { get; set; }
        }
    }
}