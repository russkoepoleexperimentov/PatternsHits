using Application.Dtos;
using Application.Services.Interfaces;
using Common;
using Common.Enums.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto dto)
    {
        await _authService.RegisterAsync(dto);
        return Ok();
    }


    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleNames.Employee)]
    [HttpPost("block/{userId}")]
    public async Task<IActionResult> BlockUser(Guid userId)
    {
        await _authService.BlockUserAsync(userId);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleNames.Employee)]
    [HttpPost("unblock/{userId}")]
    public async Task<IActionResult> UnblockUser(Guid userId)
    {
        await _authService.UnblockUserAsync(userId);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(UserChangePassword dto)
    {
        var userId = Guid.Parse(
            User.FindFirst("nameid")!.Value);

        await _authService.ChangePasswordAsync(userId, dto);

        return Ok();
    }
}