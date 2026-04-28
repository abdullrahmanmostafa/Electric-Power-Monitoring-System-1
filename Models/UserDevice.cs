// UserDevice.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("user_devices")]
    public class UserDevice
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Column("fcm_token")]
        public string FcmToken { get; set; } = string.Empty;

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        [Column("first_name")]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [Column("email")]
        [MaxLength(100)]
        public string? Email { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("password")]
        [MaxLength(255)]
        public string? Password { get; set; }  // تخزين كلمات المرور كما هي (أو مشفرة)
    }
}