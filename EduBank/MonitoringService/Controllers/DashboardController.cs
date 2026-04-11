using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringService.Controllers
{
    [Authorize(Roles = "Employee")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.UserName = User.FindFirst("name")?.Value
                            ?? User.Identity?.Name
                            ?? "Unknown";
            ViewBag.UserEmail = User.FindFirst("email")?.Value ?? "";
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/access-denied")]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost("/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties { RedirectUri = "/" });
            return Redirect("/");
        }
    }
}
