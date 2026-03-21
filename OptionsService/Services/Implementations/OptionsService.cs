using Context;
using Data;
using Dtos;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using System.Text.Json;

namespace Services.Implementations;

public class OptionService : IOptionsService
{
    private readonly OptionsDbContext _context;

    public OptionService(OptionsDbContext context)
    {
        _context = context;
    }

    private async Task<UserOptions> GetOrCreateOptionsAsync(Guid userId)
    {
        var opts = await _context.UserOptions.FirstOrDefaultAsync(o => o.UserId == userId);
        if (opts == null)
        {
            opts = new UserOptions
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WebTheme = WebTheme.Light,
                MobileTheme = MobileTheme.Red,
                HiddenAccountsJson = "[]",
                LastUpdated = DateTime.UtcNow
            };
            _context.UserOptions.Add(opts);
            await _context.SaveChangesAsync();
        }
        return opts;
    }

    public async Task<OptionsDto> GetOptionsAsync(Guid userId)
    {
        var opts = await GetOrCreateOptionsAsync(userId);
        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = JsonSerializer.Deserialize<List<Guid>>(opts.HiddenAccountsJson) ?? new()
        };
    }

    public async Task<OptionsDto> UpdateOptionsAsync(Guid userId, OptionsDto dto)
    {
        if (!Enum.TryParse<WebTheme>(dto.WebTheme, true, out var webTheme))
            throw new ArgumentException("Invalid web theme. Allowed: Light, Dark");
        if (!Enum.TryParse<MobileTheme>(dto.MobileTheme, true, out var mobileTheme))
            throw new ArgumentException("Invalid mobile theme. Allowed: Light, Red, Yellow, Blue");

        var opts = await GetOrCreateOptionsAsync(userId);
        opts.WebTheme = webTheme;
        opts.MobileTheme = mobileTheme;
        opts.HiddenAccountsJson = JsonSerializer.Serialize(dto.HiddenAccounts);
        opts.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = dto.HiddenAccounts
        };
    }

    public async Task<OptionsDto> UpdateWebThemeAsync(Guid userId, string theme)
    {
        if (!Enum.TryParse<WebTheme>(theme, true, out var webTheme))
            throw new ArgumentException("Invalid web theme. Allowed: Light, Dark");

        var opts = await GetOrCreateOptionsAsync(userId);
        opts.WebTheme = webTheme;
        opts.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = JsonSerializer.Deserialize<List<Guid>>(opts.HiddenAccountsJson) ?? new()
        };
    }

    public async Task<OptionsDto> UpdateMobileThemeAsync(Guid userId, string theme)
    {
        if (!Enum.TryParse<MobileTheme>(theme, true, out var mobileTheme))
            throw new ArgumentException("Invalid mobile theme. Allowed: Light, Red, Yellow, Blue");

        var opts = await GetOrCreateOptionsAsync(userId);
        opts.MobileTheme = mobileTheme;
        opts.LastUpdated = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = JsonSerializer.Deserialize<List<Guid>>(opts.HiddenAccountsJson) ?? new()
        };
    }

    public async Task<OptionsDto> AddHiddenAccountAsync(Guid userId, Guid accountId)
    {
        var opts = await GetOrCreateOptionsAsync(userId);
        var hidden = JsonSerializer.Deserialize<List<Guid>>(opts.HiddenAccountsJson) ?? new();
        if (!hidden.Contains(accountId))
        {
            hidden.Add(accountId);
            opts.HiddenAccountsJson = JsonSerializer.Serialize(hidden);
            opts.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = hidden
        };
    }

    public async Task<OptionsDto> RemoveHiddenAccountAsync(Guid userId, Guid accountId)
    {
        var opts = await GetOrCreateOptionsAsync(userId);
        var hidden = JsonSerializer.Deserialize<List<Guid>>(opts.HiddenAccountsJson) ?? new();
        if (hidden.Remove(accountId))
        {
            opts.HiddenAccountsJson = JsonSerializer.Serialize(hidden);
            opts.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return new OptionsDto
        {
            WebTheme = opts.WebTheme.ToString(),
            MobileTheme = opts.MobileTheme.ToString(),
            HiddenAccounts = hidden
        };
    }
}