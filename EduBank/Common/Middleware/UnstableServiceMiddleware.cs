using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Common.Middlewares
{
    public class UnstableServiceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly double _normalErrorRate;
        private readonly double _evenMinuteErrorRate;

        public UnstableServiceMiddleware(
            RequestDelegate next,
            double normalErrorRate = 0.3,
            double evenMinuteErrorRate = 0.7)
        {
            _next = next;
            _normalErrorRate = normalErrorRate;
            _evenMinuteErrorRate = evenMinuteErrorRate;
        }

        private static readonly string[] _skipPrefixes =
        [
            "/health",
            "/connect",      // IdentityServer: /connect/token, /connect/authorize, etc.
            "/account",      // Login / Logout views
            "/.well-known",  // OIDC discovery
            "/api/auth",     // AuthController endpoints
            "/signin",       // OIDC callback
            "/signout",      // OIDC logout callback
        ];

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            foreach (var prefix in _skipPrefixes)
            {
                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
            }

            var now = DateTime.UtcNow;
            bool isEvenMinute = now.Minute % 2 == 0;
            double errorRate = isEvenMinute ? _evenMinuteErrorRate : _normalErrorRate;

            var random = new Random();
            if (random.NextDouble() < errorRate)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Simulated service instability");
                return;
            }

            await _next(context);
        }
    }
}