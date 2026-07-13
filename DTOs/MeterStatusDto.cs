namespace Electric_Power_Monitoring_System.DTOs
{
    public class MeterStatusDto
    {
        public bool IsPhotoRequired { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int DaysRemaining { get; set; }
    }
}