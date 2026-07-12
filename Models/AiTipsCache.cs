using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Electric_Power_Monitoring_System.Models
{
    [Table("ai_tips_cache")]
    public class AiTipsCache
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("tip_text")]
        public string TipText { get; set; } = string.Empty;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}