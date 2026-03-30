using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using Common.Services;

namespace Common.Middlewares
{
    public class IdempotencyMiddleware
    {
        private readonly RequestDelegate _next;

        public IdempotencyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            if (!HttpMethods.IsPost(request.Method) &&
                !HttpMethods.IsPut(request.Method) &&
                !HttpMethods.IsDelete(request.Method))
            {
                await _next(context);
                return;
            }
            if (!request.Headers.TryGetValue("Idempotency-Key", out var keyValues))
            {
                await _next(context);
                return;
            }

            var key = keyValues.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                await _next(context);
                return;
            }

            var idempotencyService = context.RequestServices.GetRequiredService<IIdempotencyService>();
            var path = request.Path.ToString();
            var method = request.Method;

            var existing = await idempotencyService.GetResponseAsync(key, path, method);
            if (existing != null)
            {
                context.Response.StatusCode = existing.StatusCode;
                if (!string.IsNullOrEmpty(existing.ResponseBody))
                {
                    await context.Response.WriteAsync(existing.ResponseBody);
                }
                return;
            }

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            await _next(context);

            var statusCode = context.Response.StatusCode;
            memoryStream.Seek(0, SeekOrigin.Begin);
            var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

            await idempotencyService.StoreResponseAsync(key, path, method, statusCode, responseBody);

            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalBodyStream);
        }
    }
}