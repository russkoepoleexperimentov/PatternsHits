using Microsoft.AspNetCore.Mvc;

namespace MonitoringService.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
