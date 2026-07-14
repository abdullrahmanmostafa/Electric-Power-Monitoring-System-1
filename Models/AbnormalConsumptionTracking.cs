using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("abnormal_consumption_tracking")]
    public class AbnormalConsumptionTracking
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("hub_serial")]
        [MaxLength(50)]
        public string HubSerial { get; set; } = string.Empty;

        [Column("plug_number")]
        public int PlugNumber { get; set; }

        [Column("stage")]
        public int Stage { get; set; } = 0; // 0=طبيعي, 1=تنبيه أول, 2=متابعة, 3=فحص فني, 4=تذكير دوري

        [Column("stage_start_date")]
        public DateTime? StageStartDate { get; set; }

        [Column("last_alert_date")]
        public DateTime? LastAlertDate { get; set; }

        [Column("daily_consumption_wh")]
        [Precision(20, 10)]
        public decimal? DailyConsumptionWh { get; set; }

        [Column("is_resolved")]
        public bool IsResolved { get; set; } = false;

        [Column("resolved_date")]
        public DateTime? ResolvedDate { get; set; }
    }
}