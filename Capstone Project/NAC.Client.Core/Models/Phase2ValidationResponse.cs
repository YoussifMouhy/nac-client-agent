using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAC.Client.Core.Models
{
    public class Phase2ValidationResponse
    {
        public bool Passed { get; set; }

        public string? Message { get; set; }

        public bool Success { get; set; }

        public string? AccessToken { get; set; }
    }
}
