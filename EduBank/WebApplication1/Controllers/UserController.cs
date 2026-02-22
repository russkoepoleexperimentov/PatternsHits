using Application.Dtos;
using Application.Services.Interfaces;
using Common;
using Common.Enums.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _userService.GetByIdAsync(id));
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [HttpPut]
    public async Task<IActionResult> UpdateSelf(UserUpdateDto dto)
    {
        var id = HttpContext.GetUserId();
        await _userService.UpdateAsync(id.Value, dto);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
    [HttpGet]
    public async Task<IActionResult> GetMe()
    {
        var id = HttpContext.GetUserId();
        return Ok(await _userService.GetByIdAsync(id.Value));
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Admin}")]
    [HttpPost("{userId}/role")]
    public async Task<IActionResult> GiveRole(Guid userId, string role)
    {
        await _userService.GiveUserRoleAsync(userId, role);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleNames.Admin)]
    [HttpDelete("{userId}/role")]
    public async Task<IActionResult> RemoveRole(Guid userId, string role)
    {
        await _userService.RemoveUserRoleAsync(userId, role);
        return Ok();
    }

    [Authorize(AuthenticationSchemes = "Bearer", Roles = RoleNames.Admin)]
    [ProducesResponseType<List<UserDto>>(StatusCodes.Status200OK)]
    [HttpGet("search")]
    public async Task<IActionResult> searchUsers(string? query)
    {
        var res = await _userService.GetAllUsersAsync(query);
        return Ok(res);
    }
}