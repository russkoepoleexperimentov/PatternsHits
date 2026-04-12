using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Data;

namespace MonitoringService.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    public class MetricsController : ControllerBase
    {
        private readonly MonitoringDbContext _db;

        public MetricsController(MonitoringDbContext db)
        {
            _db = db;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] int minutes = 60)
        {
            var from = DateTime.UtcNow.AddMinutes(-minutes);

            var data = await _db.TraceEvents
                .Where(e => e.Timestamp >= from)
                .GroupBy(e => e.ServiceName)
                .Select(g => new
                {
                    ServiceName = g.Key,
                    TotalRequests = g.Count(),
                    Errors = g.Count(e => e.IsError),
                    AvgDurationMs = g.Average(e => (double)e.DurationMs),
                    MaxDurationMs = g.Max(e => e.DurationMs)
                })
                .ToListAsync();

            var result = data.Select(d => new
            {
                d.ServiceName,
                d.TotalRequests,
                d.Errors,
                ErrorRatePercent = d.TotalRequests == 0 ? 0.0 : Math.Round((double)d.Errors / d.TotalRequests * 100, 2),
                AvgDurationMs = Math.Round(d.AvgDurationMs, 1),
                d.MaxDurationMs
            });

            return Ok(result);
        }

        [HttpGet("timeline")]
        public async Task<IActionResult> Timeline([FromQuery] int minutes = 60)
        {
            var from = DateTime.UtcNow.AddMinutes(-minutes);

            var raw = await _db.TraceEvents
                .Where(e => e.Timestamp >= from)
                .Select(e => new
                {
                    e.ServiceName,
                    e.IsError,
                    e.DurationMs,
                    e.Timestamp
                })
                .ToListAsync();

            var grouped = raw
                .GroupBy(e => new
                {
                    Minute = new DateTime(e.Timestamp.Year, e.Timestamp.Month, e.Timestamp.Day,
                                         e.Timestamp.Hour, e.Timestamp.Minute, 0, DateTimeKind.Utc),
                    e.ServiceName
                })
                .Select(g => new
                {
                    Minute = g.Key.Minute,
                    Service = g.Key.ServiceName,
                    Count = g.Count(),
                    Errors = g.Count(e => e.IsError),
                    ErrorRatePercent = Math.Round((double)g.Count(e => e.IsError) / g.Count() * 100, 2),
                    AvgDurationMs = Math.Round(g.Average(e => (double)e.DurationMs), 1)
                })
                .OrderBy(x => x.Minute)
                .ThenBy(x => x.Service)
                .ToList();

            return Ok(grouped);
        }

        [HttpGet("services")]
        public async Task<IActionResult> Services()
        {
            var services = await _db.TraceEvents
                .Select(e => e.ServiceName)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            return Ok(services);
        }
    }
}
