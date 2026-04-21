namespace Electric_Power_Monitoring_System.DTOs
{
    public class IngestRequestDto
    {
        public string Serial { get; set; } = string.Empty;  // hub serial
        public int Id { get; set; }                         // plug number
        public decimal Energy { get; set; }                 // cumulative energy (assumed Wh for now)
        public bool State { get; set; }                     // true = on, false = off
    }
}