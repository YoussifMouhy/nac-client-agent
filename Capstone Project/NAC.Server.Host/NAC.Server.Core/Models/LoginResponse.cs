namespace NAC.Server.Core.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int RemainingAttempts { get; set; }
        public bool IsBlocked { get; set; }
        public string? AssignedIp { get; set; }
        public int? AssignedPort { get; set; }
        public bool RequiresPhase2 { get; set; }
        // public Phase2Requirements? Phase2Requirements { get; set; } // Uncomment if you add that class too
        public string? SessionToken { get; set; }
    }
}