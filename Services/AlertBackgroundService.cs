using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Electric_Power_Monitoring_System.Services
{
    public class AlertBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AlertBackgroundService> _logger;
        private readonly TimeZoneInfo _egyptTimeZone;

        public AlertBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AlertBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _egyptTimeZone = GetEgyptTimeZone();
        }

        private TimeZoneInfo GetEgyptTimeZone()
        {
            try
            {
                // Try Windows ID first
                return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                try
                {
                    // Try Linux/IANA ID
                    return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
                }
                catch (TimeZoneNotFoundException)
                {
                    // Fallback to UTC + 2 (Eastern European Time, no DST adjustment – Egypt no longer uses DST as of 2014)
                    // But we'll use a fixed offset to be safe
                    _logger.LogWarning("Egypt timezone not found, using UTC+2 as fallback");
                    return TimeZoneInfo.CreateCustomTimeZone(
                        "Egypt Standard Time",
                        TimeSpan.FromHours(2),
                        "Egypt (UTC+2)",
                        "Egypt (UTC+2)");
                }
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var egyptNow = TimeZoneInfo.ConvertTime(now, _egyptTimeZone);
                var nextMidnight = egyptNow.Date.AddDays(1); // tomorrow at 00:00 Egypt time
                var delay = nextMidnight - egyptNow;

                _logger.LogInformation("Next alert check scheduled at {NextMidnight} Egypt time", nextMidnight);

                await Task.Delay(delay, stoppingToken);

                // Run the alert checks
                await RunAlertChecks(stoppingToken);
            }
        }

        private async Task RunAlertChecks(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var readingRepo = scope.ServiceProvider.GetRequiredService<IReadingRepository>();
            var plugRepo = scope.ServiceProvider.GetRequiredService<IPlugRepository>();
            var userDeviceRepo = scope.ServiceProvider.GetRequiredService<IUserDeviceRepository>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var fcmSender = scope.ServiceProvider.GetRequiredService<IFcmSender>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Get all distinct hub serials that have at least one user linked
            var hubSerials = await context.UserHubs
                .Select(uh => uh.HubSerial)
                .Distinct()
                .ToListAsync();

            if (!hubSerials.Any())
            {
                _logger.LogInformation("No hubs linked to any user.");
                return;
            }

            var egyptNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _egyptTimeZone);
            var today = egyptNow.Date;
            var yesterday = today.AddDays(-1);
            var dayBefore = yesterday.AddDays(-1);

            foreach (var hubSerial in hubSerials)
            {
                // Get all user identifiers for this hub
                var userIdentifiers = await context.UserHubs
                    .Where(uh => uh.HubSerial == hubSerial)
                    .Select(uh => uh.UserIdentifier)
                    .Distinct()
                    .ToListAsync();

                var plugs = await plugRepo.GetPlugsByHubSerialAsync(hubSerial);
                foreach (var plug in plugs)
                {
                    // Daily comparison
                    var consumptionYesterday = await readingRepo.GetAggregatedConsumptionAsync(
                        hubSerial, plug.PlugNumber, yesterday, today);
                    var consumptionDayBefore = await readingRepo.GetAggregatedConsumptionAsync(
                        hubSerial, plug.PlugNumber, dayBefore, yesterday);

                    await SendAlertToAllUsers(
                        userIdentifiers,
                        hubSerial,
                        plug.PlugNumber,
                        consumptionYesterday,
                        consumptionDayBefore,
                        "daily",
                        yesterday,
                        today,
                        fcmSender,
                        notificationRepo,
                        userDeviceRepo);

                    // Weekly comparison on Sunday
                    if (egyptNow.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var endOfLastWeek = today;
                        var startOfLastWeek = endOfLastWeek.AddDays(-7);
                        var startOfPreviousWeek = startOfLastWeek.AddDays(-7);
                        var endOfPreviousWeek = startOfLastWeek;

                        var consumptionLastWeek = await readingRepo.GetAggregatedConsumptionAsync(
                            hubSerial, plug.PlugNumber, startOfLastWeek, endOfLastWeek);
                        var consumptionPrevWeek = await readingRepo.GetAggregatedConsumptionAsync(
                            hubSerial, plug.PlugNumber, startOfPreviousWeek, endOfPreviousWeek);

                        await SendAlertToAllUsers(
                            userIdentifiers,
                            hubSerial,
                            plug.PlugNumber,
                            consumptionLastWeek,
                            consumptionPrevWeek,
                            "weekly",
                            startOfLastWeek,
                            endOfLastWeek,
                            fcmSender,
                            notificationRepo,
                            userDeviceRepo);
                    }
                }
            }
        }
        private async Task SendAlertToAllUsers(
    List<string> userIdentifiers,
    string hubSerial,
    int plugNumber,
    decimal currentConsumption,
    decimal previousConsumption,
    string periodType,
    DateTime periodStart,
    DateTime periodEnd,
    IFcmSender fcmSender,
    INotificationRepository notificationRepo,
    IUserDeviceRepository userDeviceRepo)
        {
            if (currentConsumption == previousConsumption)
                return;

            var percentChange = previousConsumption == 0
                ? (currentConsumption > 0 ? 100 : 0)
                : ((currentConsumption - previousConsumption) / previousConsumption) * 100;

            var direction = currentConsumption > previousConsumption ? "increased" : "decreased";
            var changePercentAbs = Math.Abs(percentChange);
            var message = periodType == "daily"
                ? $"Hub {hubSerial}: Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous day."
                : $"Hub {hubSerial}: Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous week.";

            var title = "Energy Alert";

            foreach (var userId in userIdentifiers)
            {
                // Get FCM tokens for this user (skip placeholder tokens)
                var devices = await userDeviceRepo.GetByUserIdAsync(userId);
                foreach (var device in devices)
                {
                    if (device.FcmToken == "linked_hub") continue;
                    var sent = await fcmSender.SendNotificationAsync(device.FcmToken, title, message);
                    if (sent)
                    {
                        _logger.LogInformation("Alert sent to user {UserId} for plug {PlugNumber}", userId, plugNumber);
                    }
                }

                // Store notification in database for this user
                var notification = new Notification
                {
                    UserId = userId,
                    HubSerial = hubSerial,
                    PlugNumber = plugNumber,
                    Type = periodType == "daily" ? "daily_alert" : "weekly_alert",
                    Message = message,
                    SentAt = DateTime.UtcNow,
                    FcmResponse = "Sent"
                };
                await notificationRepo.AddAsync(notification);
            }
        }
        private async Task SendAlertIfChanged(
            string userId,
            string hubSerial,
            int plugNumber,
            decimal currentConsumption,
            decimal previousConsumption,
            string periodType,
            DateTime periodStart,
            DateTime periodEnd,
            IFcmSender fcmSender,
            INotificationRepository notificationRepo,
            IUserDeviceRepository userDeviceRepo)
        {
            // No change or zero consumption on both sides? Skip if exactly equal
            if (currentConsumption == previousConsumption)
                return;

            var percentChange = previousConsumption == 0
                ? (currentConsumption > 0 ? 100 : 0)
                : ((currentConsumption - previousConsumption) / previousConsumption) * 100;

            var direction = currentConsumption > previousConsumption ? "increased" : "decreased";
            var changePercentAbs = Math.Abs(percentChange);

            string message;
            if (periodType == "daily")
            {
                message = $"Hub {hubSerial}: Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous day.";
            }
            else
            {
                message = $"Hub {hubSerial}: Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous week.";
            }
            var title = "Energy Alert";

            // Get user's FCM tokens
            var devices = await userDeviceRepo.GetByUserIdAsync(userId);
            if (!devices.Any())
            {
                _logger.LogWarning("No FCM tokens for user {UserId}", userId);
                return;
            }

            // Send notification to each device
            foreach (var device in devices)
            {
                var sent = await fcmSender.SendNotificationAsync(device.FcmToken, title, message);
                if (sent)
                {
                    _logger.LogInformation("Alert sent to user {UserId} for plug {PlugNumber}: {Message}", userId, plugNumber, message);
                }
                else
                {
                    _logger.LogWarning("Failed to send alert to user {UserId} token {Token}", userId, device.FcmToken);
                }
            }

            // Store notification in database
            var notification = new Notification
            {
                UserId = userId,
                HubSerial = hubSerial,
                PlugNumber = plugNumber,
                Type = periodType == "daily" ? "daily_alert" : "weekly_alert",
                Message = message,
                SentAt = DateTime.UtcNow,
                FcmResponse = "Sent"
            };
            await notificationRepo.AddAsync(notification);
        }
    }
}