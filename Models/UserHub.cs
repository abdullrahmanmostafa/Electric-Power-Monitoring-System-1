using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("user_hubs")]
    public class UserHub
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("user_identifier")]
        [MaxLength(100)]
        public string UserIdentifier { get; set; } = string.Empty;

        [Column("hub_serial")]
        [MaxLength(50)]
        public string HubSerial { get; set; } = string.Empty;
    }
}