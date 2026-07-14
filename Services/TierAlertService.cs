using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;  // ← Added

namespace Electric_Power_Monitoring_System.Services
{
    public class TierAlertService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TierAlertService> _logger;
        private readonly TimeZoneInfo _egyptTimeZone;

        public TierAlertService(IServiceScopeFactory scopeFactory, ILogger<TierAlertService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time") ?? TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var egyptNow = TimeZoneInfo.ConvertTime(now, _egyptTimeZone);
                var nextMidnight = egyptNow.Date.AddDays(1);
                //    var delay = nextMidnight - egyptNow;
                var delay = TimeSpan.FromSeconds(30); // TEMP - revert after testing
                _logger.LogInformation("Next tier alert check scheduled at {NextMidnight} Egypt time", nextMidnight);

                await Task.Delay(delay, stoppingToken);

                await RunAlertCheck(stoppingToken);
            }
        }

        private async Task RunAlertCheck(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var tierService = scope.ServiceProvider.GetRequiredService<ITierService>();
            var userDeviceRepo = scope.ServiceProvider.GetRequiredService<IUserDeviceRepository>();
            var fcmSender = scope.ServiceProvider.GetRequiredService<IFcmSender>();
            var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var userIdentifiers = await context.UserHubs
                .Select(uh => uh.UserIdentifier)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIdentifiers)
            {
                try
                {
                    var alertSent = await tierService.CheckAndSendAlertAsync(userId);
                    if (alertSent)
                    {
                        var devices = await userDeviceRepo.GetByUserIdAsync(userId);
                        foreach (var device in devices)
                        {
                            if (device.FcmToken == "linked_hub") continue;

                            var lastNotification = await context.TierNotifications
                                .Where(n => n.UserIdentifier == userId)
                                .OrderByDescending(n => n.SentAt)
                                .FirstOrDefaultAsync();

                            if (lastNotification != null)
                            {
                                // ✅ Fixed line – uses System.Text.Json
                                var tips = JsonSerializer.Deserialize<List<string>>(lastNotification.TipsJson) ?? new List<string>();

                                var body = $"تبقى لك {lastNotification.RemainingKWh} كيلووات فقط، وسعر الكيلووات في الشريحة الجديدة {lastNotification.NextTierPrice} جنيه.\n";
                                body += "نصائح سريعة:\n" + string.Join("\n", tips.Select((t, i) => $"{i + 1}- {t}"));

                                await fcmSender.SendNotificationAsync(
                                    device.FcmToken,
                                    "تنبيه من مبصر - اقتراب شريحة جديدة",
                                    body
                                );
                            }
                        }
                        _logger.LogInformation("Tier alert sent for user {UserId}", userId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tier alert for user {UserId}", userId);
                }
            }
        }
    }
}