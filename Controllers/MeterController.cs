using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Services;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MeterController : ControllerBase
    {
        private readonly ILightingService _lightingService;

        public MeterController(ILightingService lightingService)
        {
            _lightingService = lightingService;
        }

        [HttpPost("reading")]
        public async Task<IActionResult> SubmitReading([FromBody] MeterReadingRequestDto request)
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            if (request.ReadingValueWh <= 0)
                return BadRequest("Reading value must be greater than zero");

            var result = await _lightingService.SubmitMeterReadingAsync(userIdentifier, request);
            if (!result)
                return StatusCode(500, "Failed to submit meter reading");

            return Ok(new { message = "Meter reading submitted successfully" });
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            var status = await _lightingService.GetMandatoryStatusAsync(userIdentifier);
            return Ok(status);
        }

        [HttpGet("consumption")]
        public async Task<IActionResult> GetLightingConsumption(
            [FromQuery] int year,
            [FromQuery] int month)
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            if (year < 2000 || year > 2100 || month < 1 || month > 12)
                return BadRequest("Invalid year or month");

            var consumption = await _lightingService.GetLightingConsumptionAsync(userIdentifier, year, month);
            return Ok(consumption);
        }
    }
}