using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("device_baseline")]
    public class DeviceBaseline
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("hub_serial")]
        [MaxLength(50)]
        public string HubSerial { get; set; } = string.Empty;

        [Column("plug_number")]
        public int PlugNumber { get; set; }

        [Column("baseline_wh")]
        public decimal BaselineWh { get; set; } // المعدل الطبيعي بالواط/ساعة

        [Column("calculated_date")]
        public DateTime CalculatedDate { get; set; } = DateTime.UtcNow;
    }
}