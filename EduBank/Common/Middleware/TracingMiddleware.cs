using Common.Contracts.MonitoringContracts;
using Common.Options;
using Common.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Common.Middlewares
{
    public class TracingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITracingClient _tracingClient;
        private readonly string _serviceName;

        public TracingMiddleware(
            RequestDelegate next,
            ITracingClient tracingClient,
            IOptions<MonitoringOptions> options)
        {
            _next = next;
            _tracingClient = tracingClient;
            _serviceName = options.Value.ServiceName;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var traceId = context.Request.Headers["X-Trace-Id"].FirstOrDefault()
                          ?? Activity.Current?.TraceId.ToString()
                          ?? Guid.NewGuid().ToString("N");

            context.Response.Headers["X-Trace-Id"] = traceId;

            var sw = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var dto = new TraceEventDto
                {
                    TraceId = traceId,
                    ServiceName = _serviceName,
                    Method = context.Request.Method,
                    Path = path,
                    StatusCode = context.Response.StatusCode,
                    DurationMs = sw.ElapsedMilliseconds,
                    IsError = context.Response.StatusCode >= 500,
                    Timestamp = DateTime.UtcNow
                };
                _ = _tracingClient.SendAsync(dto);
            }
        }
    }
}
