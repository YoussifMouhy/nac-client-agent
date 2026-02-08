using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Client.Core.Models
{
    public sealed class HeartbeatRequest
    {
        public string SessionToken { get; set; } = string.Empty;
        public DeviceInfo DeviceInfo { get; set; } = null!;
    }
}