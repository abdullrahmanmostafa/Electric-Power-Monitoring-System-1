using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("user_mandatory_state")]
    public class UserMandatoryState
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_identifier")]
        [MaxLength(100)]
        public string UserIdentifier { get; set; } = string.Empty;

        [Column("is_mandatory")]
        public bool IsMandatory { get; set; } = false;

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; } // تاريخ انتهاء الإلزام (نهاية الشهر الحالي)

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}