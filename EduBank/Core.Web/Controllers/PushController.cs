using System.Security.Claims;
using Common;
using Core.Application.Services.Interfaces;
using Core.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core.Web.Controllers
{
    [ApiController]
    [Route("api/push")]
    [Authorize]
    public class PushController : ControllerBase
    {
        private IPushService _service;

        public PushController(IPushService service)
        {
            _service = service;
        }

        [HttpPost("device")]
        [Authorize]
        public async Task<IActionResult> RegisterDevice(string fcmToken, DeviceType deviceType)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            await _service.RegisterDevice(currentUserId, fcmToken, deviceType);
            return Ok();
        }

        [HttpPost("ping")]
        [Authorize]
        public async Task<IActionResult> RequestPing()
        {
            var currentUserId = HttpContext.GetUserId()!.Value;

            await _service.SendPushToAllCustomerDevices(currentUserId, "pong", "Если ты это видишь, значит push-уведомления работают");
            return Ok();
        }
    }
}
