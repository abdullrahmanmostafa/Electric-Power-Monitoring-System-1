using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReadingsController : ControllerBase
    {
        private readonly IHubRepository _hubRepo;
        private readonly IPlugRepository _plugRepo;
        private readonly IReadingRepository _readingRepo;

        public ReadingsController(IHubRepository hubRepo, IPlugRepository plugRepo, IReadingRepository readingRepo)
        {
            _hubRepo = hubRepo;
            _plugRepo = plugRepo;
            _readingRepo = readingRepo;
        }

        [HttpPost]
        public async Task<IActionResult> IngestReading([FromBody] IngestRequestDto request)
        {
            // Validate
            if (string.IsNullOrWhiteSpace(request.Serial))
                return BadRequest("Serial is required");

            // 1. Get or create hub
            var hub = await _hubRepo.GetBySerialAsync(request.Serial);
            if (hub == null)
            {
                hub = new Hub { Serial = request.Serial };
                await _hubRepo.AddAsync(hub);
            }
            else
            {
                await _hubRepo.UpdateLastSeenAsync(request.Serial);
            }

            // 2. Ensure plug exists
            var plug = await _plugRepo.GetByHubAndPlugNumberAsync(request.Serial, request.Id);
            if (plug == null)
            {
                plug = new Plug { HubSerial = request.Serial, PlugNumber = request.Id };
                await _plugRepo.AddAsync(plug);
            }

            // 3. Save reading
            var reading = new Reading
            {
                HubSerial = request.Serial,
                PlugNumber = request.Id,
                CumulativeEnergyWh = request.Energy,
                State = request.State,
                Timestamp = DateTime.UtcNow
            };
            await _readingRepo.AddAsync(reading);

            return Ok(new { message = "Reading saved successfully" });
        }
    }
}