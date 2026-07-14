using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("meter_readings")]
    public class MeterReading
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_identifier")]
        [MaxLength(100)]
        public string UserIdentifier { get; set; } = string.Empty;

        [Column("reading_value_wh")]
        [Precision(20, 10)]
        public decimal ReadingValueWh { get; set; } // القيمة التراكمية للعداد بالواط/ساعة

        [Column("balance_egp")]
        [Precision(20, 10)]
        public decimal? BalanceEgp { get; set; } // الرصيد المتبقي (اختياري)

        [Column("reading_date")]
        public DateTime ReadingDate { get; set; } = DateTime.UtcNow;

        [Column("month")]
        public int Month { get; set; }

        [Column("year")]
        public int Year { get; set; }
    }
}