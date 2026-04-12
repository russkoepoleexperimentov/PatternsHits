using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringService.Data;
using MonitoringService.Dtos;
using MonitoringService.Entities;

namespace MonitoringService.Controllers
{
    [ApiController]
    [Route("api/traces")]
    public class TracesController : ControllerBase
    {
        private readonly MonitoringDbContext _db;

        public TracesController(MonitoringDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] IncomingTraceDto dto)
        {
            var entity = new TraceEvent
            {
                TraceId = dto.TraceId,
                ServiceName = dto.ServiceName,
                Method = dto.Method,
                Path = dto.Path,
                StatusCode = dto.StatusCode,
                DurationMs = dto.DurationMs,
                IsError = dto.IsError,
                Timestamp = dto.Timestamp == default ? DateTime.UtcNow : dto.Timestamp
            };

            _db.TraceEvents.Add(entity);
            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("recent")]
        public async Task<IActionResult> Recent([FromQuery] int limit = 100, [FromQuery] string? service = null)
        {
            var query = _db.TraceEvents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(service))
                query = query.Where(e => e.ServiceName == service);

            var result = await query
                .OrderByDescending(e => e.Timestamp)
                .Take(Math.Min(limit, 500))
                .Select(e => new
                {
                    e.Id,
                    e.TraceId,
                    e.ServiceName,
                    e.Method,
                    e.Path,
                    e.StatusCode,
                    e.DurationMs,
                    e.IsError,
                    e.Timestamp
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
