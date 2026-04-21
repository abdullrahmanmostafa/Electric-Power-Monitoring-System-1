using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsumptionController : ControllerBase
    {
        private readonly IReadingRepository _readingRepo;

        public ConsumptionController(IReadingRepository readingRepo)
        {
            _readingRepo = readingRepo;
        }

        [HttpGet("hourly")]
        public async Task<IActionResult> GetHourlyConsumption(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            if (string.IsNullOrWhiteSpace(hubSerial))
                return BadRequest("hubSerial is required");

            // Ensure start and end are aligned to hour boundaries (optional)
            start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
            end = new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0, DateTimeKind.Utc);

            var periods = new List<ConsumptionPeriodDto>();
            var current = start;

            while (current < end)
            {
                var periodEnd = current.AddHours(1);
                var consumption = await _readingRepo.GetConsumptionBetweenAsync(hubSerial, plugNumber, current, periodEnd);
                periods.Add(new ConsumptionPeriodDto
                {
                    Start = current,
                    End = periodEnd,
                    ConsumptionWh = consumption
                });
                current = periodEnd;
            }

            return Ok(new ConsumptionResponseDto
            {
                HubSerial = hubSerial,
                PlugNumber = plugNumber,
                Periods = periods
            });
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailyConsumption(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber,
            [FromQuery] DateTime start,
            [FromQuery] DateTime end)
        {
            // Similar to hourly but aggregates by day
            // You can reuse GetConsumptionBetweenAsync for each day
            // For brevity, I'll leave the implementation pattern similar to hourly.
            // (Full implementation can be added similarly.)
            return Ok("Not implemented in this example, but pattern same as hourly");
        }
    }
}