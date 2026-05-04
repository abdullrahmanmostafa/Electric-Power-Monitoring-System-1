using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.DTOs;
using Electric_Power_Monitoring_System.Repositories;
using System.Diagnostics;
namespace Electric_Power_Monitoring_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IReadingRepository _readingRepo;
        private readonly IConfiguration _config;
        private static readonly DateTime _startTime = DateTime.UtcNow;
        public AdminController(AppDbContext context, IReadingRepository readingRepo, IConfiguration config)
        {
            _context = context;
            _readingRepo = readingRepo;
            _config = config;
        }

        // GET api/admin/users
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var result = new List<UserInfoDto>();
            foreach (var user in users)
            {
                // Find all hubs linked to this user (using UserIdentifier)
                var hubSerials = await _context.UserHubs
    .Where(uh => uh.UserIdentifier == user.UserIdentifier)
    .Select(uh => uh.HubSerial)
    .ToListAsync();

                result.Add(new UserInfoDto
                {
                    Id = user.Id,
                    UserIdentifier = user.UserIdentifier,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Phone = user.Phone,
                    CreatedAt = user.CreatedAt,
                    HubSerials = hubSerials
                });
            }

            return Ok(result);
        }

        // DELETE api/admin/users/{email}
        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUserByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = $"User with email '{email}' not found." });

            // Delete related data
            var userHubs = _context.UserHubs.Where(uh => uh.UserIdentifier == user.UserIdentifier);
       
            _context.UserHubs.RemoveRange(userHubs);

            var userDevices = _context.UserDevices.Where(ud => ud.UserId == user.UserIdentifier);
            _context.UserDevices.RemoveRange(userDevices);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"User '{email}' and all associated data deleted successfully." });
        }


        public class AdminController : ControllerBase
        {
            private readonly AppDbContext _context;
            private readonly IReadingRepository _readingRepo;
            private readonly IConfiguration _config;
            private static readonly DateTime _startTime = DateTime.UtcNow;

            public AdminController(AppDbContext context, IReadingRepository readingRepo, IConfiguration config)
            {
              
            }

            // Simple API key protection (optional)
            private bool IsAdminRequest()
            {
                var adminKey = _config["Admin:ApiKey"];
                if (string.IsNullOrEmpty(adminKey)) return true; // no key configured -> allow all
                var providedKey = Request.Headers["X-Admin-Key"].FirstOrDefault();
                return providedKey == adminKey;
            }

            // 2. Dashboard Statistics
            [HttpGet("statistics")]
            public async Task<IActionResult> GetStatistics()
            {
                if (!IsAdminRequest()) return Unauthorized("Invalid or missing admin key");

                var totalUsers = await _context.Users.CountAsync();
                var totalHubs = await _context.Hubs.CountAsync();
                var totalPlugs = await _context.Plugs.CountAsync();
                var totalReadings = await _context.Readings.CountAsync();
                var totalNotifications = await _context.Notifications.CountAsync();

                // Get hub-user counts from UserHub junction table
                var hubUserCounts = await _context.UserHubs
                    .GroupBy(uh => uh.HubSerial)
                    .Select(g => new { HubSerial = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(k => k.HubSerial, v => v.Count);

                return Ok(new AdminStatisticsDto
                {
                    TotalUsers = totalUsers,
                    TotalHubs = totalHubs,
                    TotalPlugs = totalPlugs,
                    TotalReadings = totalReadings,
                    TotalNotifications = totalNotifications,
                    HubUserCounts = hubUserCounts
                });
            }

            // 3. Hub-User Relationship (alternative to statistics field)
            [HttpGet("hubs")]
            public async Task<IActionResult> GetHubUserRelations()
            {
                if (!IsAdminRequest()) return Unauthorized();

                var hubs = await _context.UserHubs
                    .GroupBy(uh => uh.HubSerial)
                    .Select(g => new { Serial = g.Key, LinkedUsers = g.Count() })
                    .ToListAsync();

                return Ok(hubs);
            }

            // 4. Admin Consumption – Day
            [HttpGet("consumption/day")]
            public async Task<IActionResult> GetDayConsumptionAdmin(
                [FromQuery] string hubSerial,
                [FromQuery] int plugNumber,
                [FromQuery] DateTime date)
            {
                if (!IsAdminRequest()) return Unauthorized();

                if (string.IsNullOrWhiteSpace(hubSerial))
                    return BadRequest("hubSerial is required");

                var start = date.Date;
                var end = start.AddDays(1);
                var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

                return Ok(new
                {
                    hubSerial,
                    plugNumber,
                    startDate = start,
                    endDate = end,
                    totalConsumptionWh = total,
                    periodType = "day"
                });
            }

            // Admin Consumption – Week
            [HttpGet("consumption/week")]
            public async Task<IActionResult> GetWeekConsumptionAdmin(
                [FromQuery] string hubSerial,
                [FromQuery] int plugNumber,
                [FromQuery] DateTime weekStart) // Sunday as first day
            {
                if (!IsAdminRequest()) return Unauthorized();

                if (string.IsNullOrWhiteSpace(hubSerial))
                    return BadRequest("hubSerial is required");

                var start = weekStart.Date;
                var end = start.AddDays(7);
                var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

                return Ok(new
                {
                    hubSerial,
                    plugNumber,
                    startDate = start,
                    endDate = end,
                    totalConsumptionWh = total,
                    periodType = "week"
                });
            }

            // Admin Consumption – Month
            [HttpGet("consumption/month")]
            public async Task<IActionResult> GetMonthConsumptionAdmin(
                [FromQuery] string hubSerial,
                [FromQuery] int plugNumber,
                [FromQuery] int year,
                [FromQuery] int month)
            {
                if (!IsAdminRequest()) return Unauthorized();

                if (string.IsNullOrWhiteSpace(hubSerial))
                    return BadRequest("hubSerial is required");
                if (month < 1 || month > 12)
                    return BadRequest("Month must be between 1 and 12");

                var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);
                var total = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, start, end);

                return Ok(new
                {
                    hubSerial,
                    plugNumber,
                    startDate = start,
                    endDate = end,
                    totalConsumptionWh = total,
                    periodType = "month"
                });
            }

            // Admin Consumption – Hourly
            [HttpGet("consumption/hourly")]
            public async Task<IActionResult> GetHourlyConsumptionAdmin(
                [FromQuery] string hubSerial,
                [FromQuery] int plugNumber,
                [FromQuery] DateTime start,
                [FromQuery] DateTime end)
            {
                if (!IsAdminRequest()) return Unauthorized();

                if (string.IsNullOrWhiteSpace(hubSerial))
                    return BadRequest("hubSerial is required");

                start = new DateTime(start.Year, start.Month, start.Day, start.Hour, 0, 0, DateTimeKind.Utc);
                end = new DateTime(end.Year, end.Month, end.Day, end.Hour, 0, 0, DateTimeKind.Utc);

                var periods = new List<object>();
                var current = start;

                while (current < end)
                {
                    var periodEnd = current.AddHours(1);
                    var consumption = await _readingRepo.GetHourlyConsumptionAsync(hubSerial, plugNumber, current, periodEnd);
                    periods.Add(new
                    {
                        start = current,
                        end = periodEnd,
                        consumptionWh = consumption
                    });
                    current = periodEnd;
                }

                return Ok(new
                {
                    hubSerial,
                    plugNumber,
                    periods
                });
            }

            // Admin Consumption – Compare two periods
            [HttpGet("consumption/compare")]
            public async Task<IActionResult> ComparePeriodsAdmin(
                [FromQuery] string hubSerial,
                [FromQuery] int plugNumber,
                [FromQuery] DateTime period1Start,
                [FromQuery] DateTime period1End,
                [FromQuery] DateTime period2Start,
                [FromQuery] DateTime period2End)
            {
                if (!IsAdminRequest()) return Unauthorized();

                if (string.IsNullOrWhiteSpace(hubSerial))
                    return BadRequest("hubSerial is required");

                var consumption1 = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, period1Start, period1End);
                var consumption2 = await _readingRepo.GetAggregatedConsumptionAsync(hubSerial, plugNumber, period2Start, period2End);

                decimal percentChange = 0;
                if (consumption2 != 0)
                    percentChange = ((consumption1 - consumption2) / consumption2) * 100;
                else if (consumption1 != 0)
                    percentChange = 100;

                var increase = consumption1 > consumption2;

                return Ok(new
                {
                    consumptionPeriod1Wh = consumption1,
                    consumptionPeriod2Wh = consumption2,
                    percentChange = Math.Round(percentChange, 2),
                    increase
                });
            }

            // 5. Recent notifications for all users
            [HttpGet("notifications")]
            public async Task<IActionResult> GetAllNotifications([FromQuery] int limit = 100, [FromQuery] int offset = 0)
            {
                if (!IsAdminRequest()) return Unauthorized();

                var notifications = await _context.Notifications
                    .OrderByDescending(n => n.SentAt)
                    .Skip(offset)
                    .Take(limit)
                    .Select(n => new AdminNotificationDto
                    {
                        Id = n.Id,
                        UserIdentifier = n.UserId,
                        HubSerial = n.HubSerial,
                        PlugNumber = n.PlugNumber,
                        Type = n.Type,
                        Message = n.Message,
                        SentAt = n.SentAt
                    })
                    .ToListAsync();

                return Ok(notifications);
            }

            // 6. Server status / metrics
            [HttpGet("status")]
            public async Task<IActionResult> GetStatus()
            {
                if (!IsAdminRequest()) return Unauthorized();

                var uptime = DateTime.UtcNow - _startTime;
                var memory = GC.GetTotalMemory(false) / (1024 * 1024); // MB
                bool dbConnected = false;
                try
                {
                    dbConnected = await _context.Database.CanConnectAsync();
                }
                catch { dbConnected = false; }

                return Ok(new AdminStatusDto
                {
                    UptimeSeconds = uptime.TotalSeconds,
                    MemoryUsageMB = memory,
                    DatabaseConnected = dbConnected,
                    ApiVersion = "1.0"
                });
            }
        }
    }
}
}