using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Electric_Power_Monitoring_System.Models
{
   
        [Table("hubs")]
        public class Hub
        {
            [Key]
            [Column("serial")]
            [MaxLength(50)]
            public string Serial { get; set; } = string.Empty;


            [Column("created_at")]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            [Column("last_seen")]
            public DateTime LastSeen { get; set; } = DateTime.UtcNow;

            // Navigation properties
            public virtual ICollection<Plug> Plugs { get; set; } = new List<Plug>();
            public virtual ICollection<Reading> Readings { get; set; } = new List<Reading>();
        }
    }

