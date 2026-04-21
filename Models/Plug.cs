using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("plugs")]
    public class Plug
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("hub_serial")]
        [MaxLength(50)]
        public string HubSerial { get; set; } = string.Empty;

        [Column("plug_number")]
        public int PlugNumber { get; set; }  // The "id" field from JSON

        [Column("name")]
        [MaxLength(100)]
        public string? Name { get; set; }  // User-assignable name like "Washing Machine"

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        [ForeignKey(nameof(HubSerial))]
        public virtual Hub? Hub { get; set; }

        // Navigation
        public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
    }
}
