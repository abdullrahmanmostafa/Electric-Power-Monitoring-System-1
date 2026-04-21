using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("notifications")]
    public class Notification
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_id")]
        [MaxLength(100)]
        public string UserId { get; set; } = string.Empty;

        [Column("hub_serial")]
        [MaxLength(50)]
        public string? HubSerial { get; set; }

        [Column("plug_number")]
        public int? PlugNumber { get; set; }

        [Column("type")]
        [MaxLength(50)]
        public string Type { get; set; } = "high_consumption";

        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("fcm_response")]
        public string? FcmResponse { get; set; }

        [ForeignKey(nameof(HubSerial))]
        public virtual Hub? Hub { get; set; }
    }
}
