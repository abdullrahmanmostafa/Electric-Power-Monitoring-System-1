using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;
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
            var hubRepo = scope.ServiceProvider.GetRequiredService<IHubRepository>();
            var userDeviceRepo = scope.ServiceProvider.GetRequiredService<IUserDeviceRepository>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var fcmSender = scope.ServiceProvider.GetRequiredService<IFcmSender>();

            var egyptNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, _egyptTimeZone);
            var today = egyptNow.Date;
            var yesterday = today.AddDays(-1);
            var dayBefore = yesterday.AddDays(-1);

            // Get all hubs that belong to a user
            var allHubs = await hubRepo.GetAllAsync();
            var userHubs = allHubs.Where(h => !string.IsNullOrEmpty(h.UserId)).ToList();

            if (!userHubs.Any())
            {
                _logger.LogInformation("No hubs linked to users. Skipping alerts.");
                return;
            }

            foreach (var hub in userHubs)
            {
                var plugs = await plugRepo.GetPlugsByHubSerialAsync(hub.Serial);
                foreach (var plug in plugs)
                {
                    // 1. Daily comparison: yesterday vs day before
                    var consumptionYesterday = await readingRepo.GetConsumptionBetweenAsync(
                        hub.Serial, plug.PlugNumber, yesterday, today);
                    var consumptionDayBefore = await readingRepo.GetConsumptionBetweenAsync(
                        hub.Serial, plug.PlugNumber, dayBefore, yesterday);

                    await SendAlertIfChanged(
                        hub.UserId!,
                        hub.Serial,
                        plug.PlugNumber,
                        consumptionYesterday,
                        consumptionDayBefore,
                        "daily",
                        yesterday,
                        today,
                        fcmSender,
                        notificationRepo,
                        userDeviceRepo);

                    // 2. Weekly comparison: only on Sunday (start of week in Egypt)
                    if (egyptNow.DayOfWeek == DayOfWeek.Sunday)
                    {
                        var endOfLastWeek = today; // Sunday (today) is the end of last week? Let's define: last 7 days = previous 7 full days ending yesterday.
                        var startOfLastWeek = endOfLastWeek.AddDays(-7);
                        var startOfPreviousWeek = startOfLastWeek.AddDays(-7);
                        var endOfPreviousWeek = startOfLastWeek;

                        var consumptionLastWeek = await readingRepo.GetConsumptionBetweenAsync(
                            hub.Serial, plug.PlugNumber, startOfLastWeek, endOfLastWeek);
                        var consumptionPrevWeek = await readingRepo.GetConsumptionBetweenAsync(
                            hub.Serial, plug.PlugNumber, startOfPreviousWeek, endOfPreviousWeek);

                        await SendAlertIfChanged(
                            hub.UserId!,
                            hub.Serial,
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

            _logger.LogInformation("Alert checks completed at {Time}", DateTime.UtcNow);
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
                message = $"Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous day.";
            }
            else
            {
                message = $"Plug {plugNumber} consumption {direction} by {changePercentAbs:F1}% compared to the previous week.";
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