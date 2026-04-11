using Common.Contracts.MonitoringContracts;
using System.Net.Http.Json;

namespace Common.Services
{
    public class TracingClient : ITracingClient
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public TracingClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendAsync(TraceEventDto dto)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("monitoring");
                await client.PostAsJsonAsync("/api/traces", dto);
            }
            catch
            {
            }
        }
    }
}
