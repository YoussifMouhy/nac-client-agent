using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Client.Core.Models
{
    public class Phase2Requirements
    {
        public string RequiredAgentVersion { get; set; } = string.Empty;

        public bool RequireAgentRunning { get; set; } = true;
        public bool RequireHeartbeat { get; set; } = true;
    }
}
