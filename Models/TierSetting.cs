using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("tier_settings")]
    public class TierSetting
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tier_name")]
        [MaxLength(50)]
        public string TierName { get; set; } = string.Empty;

        [Column("min_kwh")]
        public decimal MinKWh { get; set; }

        [Column("max_kwh")]
        public decimal MaxKWh { get; set; }

        [Column("price_per_kwh")]
        public decimal PricePerKWh { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}