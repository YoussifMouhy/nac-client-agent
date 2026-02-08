using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAC.Client.Core.Models;

namespace NAC.Client.Core.Interfaces
{
    public interface INacService
    {
        Task<LoginResponse> LoginAsync(
            LoginRequest request,
            CancellationToken cancellationToken = default
        );

        Task<Phase2ValidationResponse> ValidatePhase2Async(
            Phase2ValidationRequest request,
            CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);
        Task StartHeartbeatAsync(CancellationToken cancellationToken);
        Task StopHeartbeatAsync();

        bool IsConnected { get; }
    }
}