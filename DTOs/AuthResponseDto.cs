namespace Electric_Power_Monitoring_System.DTOs
{
    public class AuthResponseDto
    {
        public string UserIdentifier { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<string> HubSerials { get; set; } = new List<string>();
        public string Message { get; set; } = string.Empty;
    }
}