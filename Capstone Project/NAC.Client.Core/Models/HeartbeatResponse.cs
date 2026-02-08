using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Client.Core.Models
{
    public sealed class HeartbeatResponse
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
    }
}
