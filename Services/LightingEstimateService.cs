using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Electric_Power_Monitoring_System.Areas.Identity.Data;
using Electric_Power_Monitoring_System.Models;
using Electric_Power_Monitoring_System.Repositories;

namespace Electric_Power_Monitoring_System.Services
{
    public class LightingEstimateService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<LightingEstimateService> _logger;
        private readonly TimeZoneInfo _egyptTimeZone;

        public LightingEstimateService(IServiceScopeFactory scopeFactory, ILogger<LightingEstimateService> logger)
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
                var delay = nextMidnight - egyptNow;

                _logger.LogInformation("Next lighting estimate check scheduled at {NextMidnight} Egypt time", nextMidnight);

                await Task.Delay(delay, stoppingToken);

                await RunEstimate(stoppingToken);
            }
        }

        private async Task RunEstimate(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var readingRepo = scope.ServiceProvider.GetRequiredService<IReadingRepository>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<LightingEstimateService>>();

            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;
            var day = now.Day;

            // جلب جميع المستخدمين
            var userIdentifiers = await context.UserHubs
                .Select(uh => uh.UserIdentifier)
                .Distinct()
                .ToListAsync();

            foreach (var userId in userIdentifiers)
            {
                try
                {
                    // حساب الاستهلاك المقاس لليوم الحالي
                    var start = now.Date;
                    var end = start.AddDays(1);

                    var hubSerials = await context.UserHubs
                        .Where(uh => uh.UserIdentifier == userId)
                        .Select(uh => uh.HubSerial)
                        .ToListAsync();

                    decimal totalWh = 0;
                    foreach (var serial in hubSerials)
                    {
                        var plugs = await context.Plugs.Where(p => p.HubSerial == serial).ToListAsync();
                        foreach (var plug in plugs)
                        {
                            var consumption = await readingRepo.GetAggregatedConsumptionAsync(serial, plug.PlugNumber, start, end);
                            totalWh += consumption;
                        }
                    }

                    // تقدير الإنارة = 13% من الاستهلاك المقاس (نسبة ثابتة، يمكن تغييرها)
                    var lightingEstimate = totalWh * 0.13m;

                    // تخزين التقدير
                    var estimate = new LightingEstimate
                    {
                        UserIdentifier = userId,
                        Year = year,
                        Month = month,
                        Day = day,
                        EstimatedWh = lightingEstimate,
                        IsCorrected = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    context.LightingEstimates.Add(estimate);
                    logger.LogInformation("Lighting estimate for user {UserId} on {Day}/{Month}: {Estimate} Wh", userId, day, month, lightingEstimate);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error estimating lighting for user {UserId}", userId);
                }
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Lighting estimates completed for {Count} users", userIdentifiers.Count);
        }
    }
}