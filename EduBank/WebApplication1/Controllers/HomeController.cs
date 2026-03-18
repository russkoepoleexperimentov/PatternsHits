using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Route("home")]
    public class HomeController : Controller
    {
        [HttpGet("error")]
        public IActionResult Error(string errorId)
        {
            return Content($"Error: {errorId}");
        }
    }
}