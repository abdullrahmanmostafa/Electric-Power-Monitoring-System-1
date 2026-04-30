using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class HubsController : ControllerBase
{
    private readonly IHubRepository _hubRepo;
    private readonly IPlugRepository _plugRepo;
    private readonly IReadingRepository _readingRepo;
    private readonly AppDbContext _context;

    public HubsController(IHubRepository hubRepo, IPlugRepository plugRepo, IReadingRepository readingRepo, AppDbContext context)
    {
        _hubRepo = hubRepo;
        _plugRepo = plugRepo;
        _readingRepo = readingRepo;
        _context = context;
    }

    [HttpPost("link")]
    public async Task<IActionResult> LinkHubToUser([FromBody] LinkHubRequestDto request)
    {
        var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdentifier))
            return Unauthorized("X-User-Id header is required");

        var hub = await _hubRepo.GetBySerialAsync(request.Serial);
        if (hub == null)
            return NotFound($"Hub with serial {request.Serial} not found");

        // Check if already linked
        var exists = await _context.UserHubs.AnyAsync(uh => uh.UserIdentifier == userIdentifier && uh.HubSerial == request.Serial);
        if (exists)
            return Ok(new { message = "Hub already linked to this user" });

        var userHub = new UserHub
        {
            UserIdentifier = userIdentifier,
            HubSerial = request.Serial
        };
        _context.UserHubs.Add(userHub);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Hub linked successfully" });
    }

    [HttpGet("my-plugs")]
    public async Task<IActionResult> GetMyPlugs()
    {
        var userIdentifier = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(userIdentifier))
            return Unauthorized("User ID missing");

        // Get all hub serials linked to this user
        var hubSerials = await _context.UserHubs
            .Where(uh => uh.UserIdentifier == userIdentifier)
            .Select(uh => uh.HubSerial)
            .ToListAsync();

        var result = new List<PlugResponseDto>();
        foreach (var serial in hubSerials)
        {
            var plugs = await _plugRepo.GetPlugsByHubSerialAsync(serial);
            foreach (var plug in plugs)
            {
                var lastReading = await _readingRepo.GetLastReadingAsync(serial, plug.PlugNumber);
                result.Add(new PlugResponseDto
                {
                    PlugNumber = plug.PlugNumber,
                    Name = plug.Name,
                    LastEnergyWh = lastReading?.CumulativeEnergyWh,
                    LastState = lastReading?.State,
                    LastReadingTime = lastReading?.Timestamp
                });
            }
        }
        return Ok(result);
    }
}