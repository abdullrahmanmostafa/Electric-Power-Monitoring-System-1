using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.Services;
using Electric_Power_Monitoring_System.DTOs;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly IAbnormalConsumptionService _abnormalService;

        public DeviceController(IAbnormalConsumptionService abnormalService)
        {
            _abnormalService = abnormalService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetDeviceStatus()
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            var status = await _abnormalService.GetUserDevicesStatusAsync(userIdentifier);
            return Ok(status);
        }

        [HttpGet("baseline")]
        public async Task<IActionResult> GetDeviceBaseline(
            [FromQuery] string hubSerial,
            [FromQuery] int plugNumber)
        {
            var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userIdentifier))
                return Unauthorized("X-User-Id header is required");

            var baseline = await _abnormalService.GetDeviceBaselineAsync(hubSerial, plugNumber);
            if (baseline == null)
                return NotFound(new { message = "Device baseline not found" });

            return Ok(baseline);
        }
    }
}