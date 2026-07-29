using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("lighting_estimates")]
    public class LightingEstimate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_identifier")]
        [MaxLength(100)]
        public string UserIdentifier { get; set; } = string.Empty;

        [Column("year")]
        public int Year { get; set; }

        [Column("month")]
        public int Month { get; set; }

        [Column("day")]
        public int Day { get; set; }

        [Column("estimated_wh")]
        [Precision(20, 10)]
        public decimal EstimatedWh { get; set; } // التقدير اليومي (واط/ساعة)

        [Column("actual_wh")]
        [Precision(20, 10)]
        public decimal? ActualWh { get; set; } // القيمة الفعلية بعد التصحيح
        [Column("actual_percentage")]
        [Precision(20, 10)]
        public decimal? ActualPercentage { get; set; } // النسبة الفعلية بعد التصحيح
        [Column("is_corrected")]
        public bool IsCorrected { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}