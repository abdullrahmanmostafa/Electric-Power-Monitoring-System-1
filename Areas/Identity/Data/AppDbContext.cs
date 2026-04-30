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
        }
    }
}