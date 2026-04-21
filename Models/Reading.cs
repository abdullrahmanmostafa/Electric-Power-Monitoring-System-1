using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Electric_Power_Monitoring_System.Models
{

    [Table("readings")]
    public class Reading
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("hub_serial")]
        [MaxLength(50)]
        public string HubSerial { get; set; } = string.Empty;

        [Column("plug_number")]
        public int PlugNumber { get; set; }

        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;  // Server-generated

        [Column("cumulative_energy_wh")]
        public decimal CumulativeEnergyWh { get; set; }  // Watt-hours

        [Column("state")]
        public bool State { get; set; }  // true = on, false = off

        // Foreign keys (composite FK to Plug)
        [ForeignKey(nameof(HubSerial))]
        public virtual Hub? Hub { get; set; }

    }
}
