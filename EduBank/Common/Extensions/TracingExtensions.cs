using Common.Options;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Extensions
{
    public static class TracingExtensions
    {
        public static IServiceCollection AddTracing(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MonitoringOptions>(configuration.GetSection("Monitoring"));

            var baseUrl = configuration["Monitoring:BaseUrl"] ?? "http://monitoringweb:8080";

            services.AddHttpClient("monitoring", client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(3);
            });

            services.AddSingleton<ITracingClient, TracingClient>();

            return services;
        }
    }
}
