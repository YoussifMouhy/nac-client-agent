using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NAC.Server.Core.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NAC.Server.Services.Session
{
    public class SessionCleanupService : BackgroundService
    {
        private readonly ISessionManager _sessionManager;
        private readonly IFirewallService _firewallService;

        // Use a logger to see what's happening in the console
        private readonly ILogger<SessionCleanupService> _logger;

        // How often to check (every 30 seconds)
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);
        // How long before we kick them out (2 minutes)
        private readonly TimeSpan _sessionTimeout = TimeSpan.FromMinutes(2);

        public SessionCleanupService(ISessionManager sessionManager, IFirewallService firewallService, ILogger<SessionCleanupService> logger)
        {
            _sessionManager = sessionManager;
            _firewallService = firewallService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Session Cleanup Service is running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Ask SessionManager who has expired
                    var expiredIps = _sessionManager.GetExpiredSessions(_sessionTimeout);

                    foreach (var ip in expiredIps)
                    {
                        // 2. Revoke Firewall Access
                        _logger.LogWarning($"[Cleanup] Session expired for {ip}. Revoking access.");
                        _firewallService.RevokeClientAccess(ip);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during session cleanup");
                }

                // 3. Wait 30 seconds before checking again
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}