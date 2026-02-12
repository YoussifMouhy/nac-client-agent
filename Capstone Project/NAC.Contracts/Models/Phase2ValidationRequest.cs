using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Contracts.Models
{
    public class Phase2ValidationRequest
    {
        public string DeviceId { get; set; } = string.Empty;

        public string ClientVersion { get; set; } = string.Empty;

        public DeviceInfo DeviceInfo { get; set; } = null!;

        public string AgentVersion { get; set; } = string.Empty;

        public string SessionToken { get; set; } = string.Empty;
        public bool Success { get; set; }

        public string? Message { get; set; }
    }
}
