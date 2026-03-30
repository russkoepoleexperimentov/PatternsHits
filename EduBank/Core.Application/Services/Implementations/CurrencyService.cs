using Common.Policies;
using Core.Application.Services.Interfaces;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Core.Application.Services.Implementations;

public class CurrencyRateService : ICurrencyRateService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private string _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    private static readonly AsyncPolicy<HttpResponseMessage> _policy = HttpPolicy.GetCombinedPolicy();

    public CurrencyRateService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    private async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken != null && _tokenExpiry > DateTime.UtcNow.AddSeconds(30))
            return _cachedToken;

        var authority = _configuration["IdentityServer:Authority"];
        var clientId = _configuration["IdentityServer:ClientId"];
        var clientSecret = _configuration["IdentityServer:ClientSecret"];
        var scope = _configuration["IdentityServer:Scope"];

        var tokenEndpoint = $"{authority}/connect/token";

        var tokenResponse = await _httpClient.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = tokenEndpoint,
                ClientId = clientId,
                ClientSecret = clientSecret,
                Scope = scope
            });

        if (tokenResponse.IsError)
            throw new Exception(tokenResponse.Error);

        _cachedToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
        return _cachedToken;
    }

    public async Task<decimal> GetExchangeRateAsync(string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency) return 1m;

        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var url = $"/api/rates/{fromCurrency}/{toCurrency}";

        var response = await _policy.ExecuteAsync(async () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var resp = await _httpClient.SendAsync(request);
            if (!resp.IsSuccessStatusCode)
                return resp;
            return resp;
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<RateResponse>();
        return result.Rate;
    }

    private class RateResponse
    {
        public decimal Rate { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}