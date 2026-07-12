using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("tier_notifications")]
    public class TierNotification
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_identifier")]
        [MaxLength(100)]
        public string UserIdentifier { get; set; } = string.Empty;

        [Column("remaining_kwh")]
        public decimal RemainingKWh { get; set; }

        [Column("next_tier_price")]
        public decimal NextTierPrice { get; set; }

        [Column("tips")]
        public string TipsJson { get; set; } = string.Empty; // سنخزن الـ 3 نصائح كـ JSON

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        [Column("is_read")]
        public bool IsRead { get; set; } = false;
    }
}