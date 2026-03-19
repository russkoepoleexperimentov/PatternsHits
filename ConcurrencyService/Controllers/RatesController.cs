using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CurrencyService.Data;

namespace CurrencyService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RatesController : ControllerBase
{
    private readonly CurrencyDbContext _context;

    public RatesController(CurrencyDbContext context)
    {
        _context = context;
    }

    [HttpGet("latest")]
    public async Task<ActionResult> GetLatestRates([FromQuery] string @base = "USD")
    {
        var rates = await _context.ExchangeRates
            .Where(r => r.BaseCurrency == @base)
            .ToDictionaryAsync(r => r.TargetCurrency, r => r.Rate);

        return Ok(new { Base = @base, Rates = rates });
    }

    [HttpGet("{baseCurrency}/{targetCurrency}")]
    public async Task<ActionResult> GetRate(string baseCurrency, string targetCurrency)
    {
        var rate = await _context.ExchangeRates
            .FirstOrDefaultAsync(r => r.BaseCurrency == baseCurrency && r.TargetCurrency == targetCurrency);

        if (rate == null)
            return NotFound();

        return Ok(new
        {
            From = baseCurrency,
            To = targetCurrency,
            Rate = rate.Rate,
            LastUpdated = rate.LastUpdated
        });
    }
}