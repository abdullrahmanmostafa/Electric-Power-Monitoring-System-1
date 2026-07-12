using Electric_Power_Monitoring_System.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric_Power_Monitoring_System.Areas.Identity.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Hub> Hubs { get; set; }
        public DbSet<Plug> Plugs { get; set; }
        public DbSet<Reading> Readings { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }   // New table for FCM tokens
        public DbSet<User> Users { get; set; }
        public DbSet<UserHub> UserHubs { get; set; }
        public DbSet<TierSetting> TierSettings { get; set; }
        public DbSet<UserConsumptionTracking> UserConsumptionTracking { get; set; }
        public DbSet<AiTipsCache> AiTipsCache { get; set; }
        public DbSet<TierNotification> TierNotifications { get; set; }
        public DbSet<DeviceBaseline> DeviceBaselines { get; set; }
        public DbSet<AbnormalConsumptionTracking> AbnormalConsumptionTrackings { get; set; }
        // Inside OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<UserHub>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserIdentifier, e.HubSerial })
                      .IsUnique()
                      .HasDatabaseName("IX_user_hubs_user_hub");
            });
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserIdentifier).IsUnique().HasDatabaseName("IX_users_user_identifier");
                entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("IX_users_email");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            // Hub configuration
            modelBuilder.Entity<Hub>(entity =>
            {
                entity.HasKey(e => e.Serial);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastSeen).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Plug configuration
            modelBuilder.Entity<Plug>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.HubSerial, e.PlugNumber })
                      .IsUnique()
                      .HasDatabaseName("IX_plugs_hub_serial_plug_number");

                entity.HasOne(e => e.Hub)
                      .WithMany(h => h.Plugs)
                      .HasForeignKey(e => e.HubSerial)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Reading configuration
            modelBuilder.Entity<Reading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.HubSerial, e.PlugNumber, e.Timestamp })
                      .HasDatabaseName("IX_readings_hub_plug_timestamp");
                entity.HasIndex(e => e.Timestamp)
                      .HasDatabaseName("IX_readings_timestamp");

                entity.HasOne(e => e.Hub)
                      .WithMany(h => h.Readings)
                      .HasForeignKey(e => e.HubSerial)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Notification configuration
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_notifications_user_id");
                entity.HasIndex(e => e.SentAt).HasDatabaseName("IX_notifications_sent_at");

                entity.HasOne(e => e.Hub)
                      .WithMany()
                      .HasForeignKey(e => e.HubSerial)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // UserDevice configuration (new)
            modelBuilder.Entity<UserDevice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_user_devices_user_id");
                entity.HasIndex(e => e.FcmToken).HasDatabaseName("IX_user_devices_fcm_token");
                entity.Property(e => e.RegisteredAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
            modelBuilder.Entity<UserConsumptionTracking>(entity =>
            {
                entity.HasIndex(e => new { e.UserIdentifier, e.Year, e.Month })
                      .IsUnique()
                      .HasDatabaseName("IX_user_consumption_tracking_user_year_month");
            });

            modelBuilder.Entity<TierNotification>(entity =>
            {
                entity.HasIndex(e => e.UserIdentifier).HasDatabaseName("IX_tier_notifications_user");
                entity.HasIndex(e => e.SentAt).HasDatabaseName("IX_tier_notifications_sent_at");
            });
            // شرائح الكهرباء في مصر (عداد مسبق الدفع) - مثال
            modelBuilder.Entity<TierSetting>().HasData(
                new TierSetting { Id = 1, TierName = "شريحة 1", MinKWh = 0, MaxKWh = 50, PricePerKWh = 0.68m, IsActive = true },
                new TierSetting { Id = 2, TierName = "شريحة 2", MinKWh = 51, MaxKWh = 100, PricePerKWh = 0.78m, IsActive = true },
                new TierSetting { Id = 3, TierName = "شريحة 3", MinKWh = 101, MaxKWh = 200, PricePerKWh = 0.95m, IsActive = true },
                new TierSetting { Id = 4, TierName = "شريحة 4", MinKWh = 201, MaxKWh = 350, PricePerKWh = 1.20m, IsActive = true },
                new TierSetting { Id = 5, TierName = "شريحة 5", MinKWh = 351, MaxKWh = 650, PricePerKWh = 1.45m, IsActive = true },
                new TierSetting { Id = 6, TierName = "شريحة 6", MinKWh = 651, MaxKWh = 1000, PricePerKWh = 1.60m, IsActive = true },
                new TierSetting { Id = 7, TierName = "شريحة 7", MinKWh = 1001, MaxKWh = decimal.MaxValue, PricePerKWh = 1.85m, IsActive = true }
            );

            // نصائح احتياطية
            modelBuilder.Entity<AiTipsCache>().HasData(
                new AiTipsCache { Id = 1, TipText = "أطفئ الأجهزة غير المستخدمة من الفيشة لتوفير الكهرباء.", IsActive = true },
                new AiTipsCache { Id = 2, TipText = "استخدم لمبات LED بدلاً من اللمبات العادية.", IsActive = true },
                new AiTipsCache { Id = 3, TipText = "اضبط درجة حرارة الثلاجة على المستوى المتوسط.", IsActive = true },
                new AiTipsCache { Id = 4, TipText = "شغل الغسالة والغسالة الأطباق في أوقات الذروة المنخفضة (بعد 10 مساءً).", IsActive = true },
                new AiTipsCache { Id = 5, TipText = "قلل من استخدام المكواة الكهربائية أو استخدمها لفترات قصيرة.", IsActive = true },
                new AiTipsCache { Id = 6, TipText = "استخدم سخان المياه الشمسي بدلاً من الكهربائي.", IsActive = true },
                new AiTipsCache { Id = 7, TipText = "افصل شاحن الهاتف بعد شحن البطارية.", IsActive = true, },
                    new AiTipsCache { Id = 8, TipText = "افحص باب الثلاجة للتأكد من إغلاقه بإحكام.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 9, TipText = "نظف المكثف الخلفي للثلاجة من الأتربة.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 10, TipText = "تأكد من عدم وجود تسريب في غاز التبريد.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 11, TipText = "افحص منظم الحرارة واضبطه على درجة حرارة مناسبة.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 12, TipText = "تأكد من عدم وجود ثلج متراكم داخل الفريزر.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 13, TipText = "افصل الثلاجة لمدة ساعة ثم أعد تشغيلها.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 14, TipText = "تأكد من عدم وجود فجوات في عازل الباب.", Type = "abnormal", IsActive = true },
                    new AiTipsCache { Id = 15, TipText = "قم بقياس درجة الحرارة داخل الثلاجة للتأكد من أنها مناسبة.", Type = "abnormal", IsActive = true }
            );
                modelBuilder.Entity<DeviceBaseline>(entity =>
                {
                    entity.HasIndex(e => new { e.HubSerial, e.PlugNumber })
                          .IsUnique()
                          .HasDatabaseName("IX_device_baseline_hub_plug");
                });

            modelBuilder.Entity<AbnormalConsumptionTracking>(entity =>
            {
                entity.HasIndex(e => new { e.HubSerial, e.PlugNumber })
                      .IsUnique()
                      .HasDatabaseName("IX_abnormal_tracking_hub_plug");
                entity.HasIndex(e => e.Stage).HasDatabaseName("IX_abnormal_tracking_stage");
            });
        }
    }
}