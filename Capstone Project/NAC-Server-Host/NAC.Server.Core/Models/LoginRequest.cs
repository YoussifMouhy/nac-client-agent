namespace NAC.Server.Core.Models
{
    public sealed class LoginRequest
    {
        public string Username { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string? DeviceId { get; init; }
        public int AssignedPort { get; set; }
    }
}