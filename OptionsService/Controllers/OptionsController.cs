using Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Security.Claims;

namespace Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OptionsController : ControllerBase
{
    private readonly IOptionsService _optionsService;

    public OptionsController(IOptionsService optionsService)
    {
        _optionsService = optionsService;
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(sub))
            throw new UnauthorizedAccessException("User identifier not found in token.");
        return Guid.Parse(sub);
    }

    [HttpGet]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> GetOptions()
    {
        var userId = GetUserId();
        var options = await _optionsService.GetOptionsAsync(userId);
        return Ok(options);
    }

    [HttpPut]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> UpdateOptions(OptionsDto dto)
    {
        try
        {
            var userId = GetUserId();
            var updated = await _optionsService.UpdateOptionsAsync(userId, dto);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("web-theme")]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> UpdateWebTheme([FromBody] string theme)
    {
        try
        {
            var userId = GetUserId();
            var updated = await _optionsService.UpdateWebThemeAsync(userId, theme);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("mobile-theme")]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> UpdateMobileTheme([FromBody] string theme)
    {
        try
        {
            var userId = GetUserId();
            var updated = await _optionsService.UpdateMobileThemeAsync(userId, theme);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("hidden-accounts/{accountId}")]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> AddHiddenAccount(Guid accountId)
    {
        try
        {
            var userId = GetUserId();
            var updated = await _optionsService.AddHiddenAccountAsync(userId, accountId);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("hidden-accounts/{accountId}")]
    [ProducesResponseType(typeof(OptionsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OptionsDto>> RemoveHiddenAccount(Guid accountId)
    {
        try
        {
            var userId = GetUserId();
            var updated = await _optionsService.RemoveHiddenAccountAsync(userId, accountId);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}