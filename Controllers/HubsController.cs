using Microsoft.AspNetCore.Mvc;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HubsController : ControllerBase
    {
        private readonly IHubRepository _hubRepo;

        public HubsController(IHubRepository hubRepo)
        {
            _hubRepo = hubRepo;
        }

        [HttpPost("link")]
        public async Task<IActionResult> LinkHubToUser([FromBody] LinkHubRequestDto request)
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("X-User-Id header is required");

            var hub = await _hubRepo.GetBySerialAsync(request.Serial);
            if (hub == null)
                return NotFound($"Hub with serial {request.Serial} not found");

            // إذا كان الـ hub مرتبطًا بالفعل بمستخدم مختلف
            if (!string.IsNullOrEmpty(hub.UserId) && hub.UserId != userId)
            {
                return BadRequest($"Hub {request.Serial} is already linked to another user. Cannot relink.");
            }

            // إذا كان غير مرتبط أو مرتبط بنفس المستخدم (تكرار المحاولة)
            hub.UserId = userId;
            await _hubRepo.UpdateAsync(hub);

            return Ok(new { message = "Hub linked successfully" });
        }

        [HttpGet("my-plugs")]
        public async Task<IActionResult> GetMyPlugs()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("User ID missing");

            var hubs = await _hubRepo.GetHubsByUserIdAsync(userId);
            if (!hubs.Any())
                return Ok(new List<PlugResponseDto>());

            // Get all plugs for all hubs of this user
            var plugRepo = HttpContext.RequestServices.GetRequiredService<IPlugRepository>();
            var allPlugs = new List<PlugResponseDto>();
            foreach (var hub in hubs)
            {
                var plugs = await plugRepo.GetPlugsByHubSerialAsync(hub.Serial);
                foreach (var plug in plugs)
                {
                    var lastReading = await HttpContext.RequestServices.GetRequiredService<IReadingRepository>()
                        .GetLastReadingAsync(hub.Serial, plug.PlugNumber);
                    allPlugs.Add(new PlugResponseDto
                    {
                        PlugNumber = plug.PlugNumber,
                        Name = plug.Name,
                        LastEnergyWh = lastReading?.CumulativeEnergyWh,
                        LastState = lastReading?.State,
                        LastReadingTime = lastReading?.Timestamp
                    });
                }
            }
            return Ok(allPlugs);
        }
    }
}