using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Contracts.Models
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
        public Phase2Requirements? Phase2Requirements { get; set; }
        public string? SessionToken { get; set; }
    }
}
