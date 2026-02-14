namespace NAC.Server.Core.Models
{
    public sealed class HeartbeatRequest
    {
        public string SessionToken { get; set; } = string.Empty;
        public DeviceInfo DeviceInfo { get; set; } = null!;
    }
}