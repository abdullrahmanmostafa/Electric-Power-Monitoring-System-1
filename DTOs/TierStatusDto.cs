namespace Electric_Power_Monitoring_System.DTOs
{
    public class TierStatusDto
    {
        public decimal CurrentConsumptionKWh { get; set; }
        public string CurrentTierName { get; set; } = string.Empty;
        public decimal CurrentTierPrice { get; set; }
        public decimal? RemainingKWh { get; set; }  // إذا كان في آخر شريحة، يكون null
        public string? NextTierName { get; set; }
        public decimal? NextTierPrice { get; set; }
        public decimal? NextTierThreshold { get; set; } // الحد الأدنى للشريحة التالية
        public bool IsThresholdReached { get; set; } // إذا كان المتبقي <= 20 كيلووات
        public List<string>? Tips { get; set; } // النصائح (إذا تم توليدها)
    }
}