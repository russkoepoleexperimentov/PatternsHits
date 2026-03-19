using ConcurrencyService.Services.Interfaces;
using System.Text.Json.Serialization;

namespace CurrencyService.Services;

public class ExchangeRateApiService : ICurrencyApiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ExchangeRateApiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<Dictionary<string, decimal>> GetLatestRatesAsync(string baseCurrency)
    {
        try
        {
            var apiKey = _configuration["CurrencyApi:ApiKey"];
            var requestUrl = $"{apiKey}/latest/{baseCurrency}";
            var response = await _httpClient.GetAsync(requestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<ExchangeRateApiResponse>();
            if (json?.Result == "success" && json.ConversionRates != null)
            {
                return json.ConversionRates;
            }

            return new Dictionary<string, decimal>();
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private class ExchangeRateApiResponse
    {
        [JsonPropertyName("result")]
        public string Result { get; set; } = string.Empty;

        [JsonPropertyName("base_code")]
        public string BaseCode { get; set; } = string.Empty;

        [JsonPropertyName("conversion_rates")]
        public Dictionary<string, decimal> ConversionRates { get; set; } = new();
    }
}