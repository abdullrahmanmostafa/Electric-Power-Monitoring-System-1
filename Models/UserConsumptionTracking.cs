using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("user_consumption_tracking")]
    public class UserConsumptionTracking
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

        [Column("cumulative_consumption_wh")]
        [Precision(20, 10)]
        public decimal CumulativeConsumptionWh { get; set; }

        [Column("last_updated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}