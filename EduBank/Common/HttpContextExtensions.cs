using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Common
{
    public static class HttpContextExtensions
    {
        public static Guid? GetUserId(this HttpContext context)
        {
            var sub = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return sub != null ? Guid.Parse(sub) : null;
        }
    }
}
