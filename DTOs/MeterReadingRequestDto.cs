namespace Electric_Power_Monitoring_System.DTOs
{
    public class MeterReadingRequestDto
    {
        public decimal ReadingValueWh { get; set; } // القيمة التراكمية للعداد بالواط/ساعة
        public decimal? BalanceEgp { get; set; } // الرصيد (اختياري)
    }
}