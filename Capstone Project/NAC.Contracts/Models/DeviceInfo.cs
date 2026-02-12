using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Contracts.Models
{
    public sealed class DeviceInfo
    {
        public string DeviceName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public int VpnPort { get; set; }
        public DateTime ConnectedAt { get; set; }
    }

}
