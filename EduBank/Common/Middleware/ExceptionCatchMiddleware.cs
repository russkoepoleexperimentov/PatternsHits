using System;
using System.Collections.Generic;
using Common.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Common.Middlewares
{
    public class ExceptionCatchMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionCatchMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (StatusCodeException ex)
            {
                await HandleExceptionAsync(context, ex);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception is StatusCodeException statusCodeException ?
                statusCodeException.StatusCode :
                StatusCodes.Status500InternalServerError;

            var response = new { message = exception.Message };
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
