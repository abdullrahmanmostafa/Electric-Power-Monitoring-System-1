namespace Electric_Power_Monitoring_System.DTOs
{
    public class UnifiedNotificationDto
    {
        public string Type { get; set; } = string.Empty;
        // قيم محتملة: "old", "tier_alert", "abnormal_alert", "mandatory_photo", "correction_confirmed"
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime? Timestamp { get; set; }
        public string? HubSerial { get; set; }
        public int? PlugNumber { get; set; }
        public int? Stage { get; set; } // خاص بالميزة 2 (مرحلة التنبيه)
        public decimal? RemainingKWh { get; set; } // خاص بالميزة 1
        public decimal? NextTierPrice { get; set; } // خاص بالميزة 1
        public bool? IsMandatory { get; set; } // خاص بالميزة 3
        public bool IsRead { get; set; } = false; // خاص بالإشعارات القديمة
        public long? NotificationId { get; set; } // للرجوع إلى الجدول الأصلي إن لزم
    }
}