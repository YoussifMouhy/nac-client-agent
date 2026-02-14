using Microsoft.AspNetCore.Mvc;
using NAC.Server.Core.Interfaces;
using NAC.Server.Core.Models;
using System;

namespace NAC.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IFirewallService _firewallService;
        private readonly ISessionManager _sessionManager;
        private const int MAX_ATTEMPTS = 3;
        private string usrname = "admin";
        private string password = "admin";

        public AuthController(IFirewallService firewallService, ISessionManager sessionManager)
        {
            _firewallService = firewallService;
            _sessionManager = sessionManager;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // ---------------------------------------------------------
            // 1. GET & CLEAN THE IP ADDRESS
            // ---------------------------------------------------------
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Fix IPv6 Localhost
            if (clientIp == "::1") clientIp = "127.0.0.1";

            // Fix "IPv4 mapped to IPv6" (::ffff:192.168.x.x)
            if (!string.IsNullOrEmpty(clientIp) && clientIp.StartsWith("::ffff:"))
            {
                clientIp = clientIp.Substring(7);
            }

            // OPTIONAL: If running in a VM, the Server sees the Gateway IP (e.g., 192.168.180.1).
            // This is actually CORRECT for the firewall. 
            // If you want to force the LAN IP, uncomment the block below, 
            // but usually 192.168.180.1 is the one that needs to be allowed.
            
            /* if (request.DeviceInfo != null && !string.IsNullOrEmpty(request.DeviceInfo.IpAddress) 
                && request.DeviceInfo.IpAddress != "Unknown" 
                && request.DeviceInfo.IpAddress != "127.0.0.1")
            {
                clientIp = request.DeviceInfo.IpAddress;
            } 
            */

            Console.WriteLine($"[Login] Processing Request from: {clientIp}");

            // ---------------------------------------------------------
            // 2. CHECK BLOCK STATUS
            // ---------------------------------------------------------
            if (_sessionManager.IsClientBlocked(clientIp))
            {
                return Ok(new LoginResponse
                {
                    Success = false,
                    IsBlocked = true,
                    RemainingAttempts = 0,
                    Message = "Your IP is temporarily blocked due to multiple failed attempts."
                });
            }

            // ---------------------------------------------------------
            // 3. VALIDATE CREDENTIALS
            // ---------------------------------------------------------
            if (request.Username == usrname && request.Password == password)
            {
                // A. Reset Failures
                _sessionManager.ResetLoginFailures(clientIp);

                // B. TRY TO OPEN FIREWALL
                // We use the cleaned IP here. If this returns false, we stop.
                bool firewallOpened = _firewallService.AllowClientAccess(clientIp);

                if (!firewallOpened)
                {
                    // ❌ SERVER ERROR (Permissions or PowerShell crash)
                    return StatusCode(500, new LoginResponse 
                    { 
                        Success = false, 
                        Message = $"Login Verified, but Server failed to open Firewall for {clientIp}. (Is Server running as Admin?)" 
                    });
                }

                // C. Register Session (Only if firewall worked)
                string token = Guid.NewGuid().ToString();
                _sessionManager.RegisterSession(request.Username, clientIp, request.DeviceId ?? "Unknown");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login Successful",
                    SessionToken = token,
                    AssignedIp = clientIp,
                    RequiresPhase2 = false,
                    RemainingAttempts = MAX_ATTEMPTS,
                    IsBlocked = false
                });
            }

            // ---------------------------------------------------------
            // 4. HANDLE FAILURE (Wrong Password)
            // ---------------------------------------------------------
            int currentFailures = _sessionManager.RegisterLoginFailure(clientIp);
            int remaining = Math.Max(0, MAX_ATTEMPTS - currentFailures);

            Console.WriteLine($"[Login] Failed attempt {currentFailures}/{MAX_ATTEMPTS} for {clientIp}");

            return Ok(new LoginResponse
            {
                Success = false,
                Message = "Invalid Credentials",
                RemainingAttempts = remaining,
                IsBlocked = (remaining == 0)
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            string clientIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            
            // Clean IP for logout too
            if (clientIp == "::1") clientIp = "127.0.0.1";
            if (!string.IsNullOrEmpty(clientIp) && clientIp.StartsWith("::ffff:")) clientIp = clientIp.Substring(7);

            _firewallService.RevokeClientAccess(clientIp);
            
            // Optional: Remove session
            // _sessionManager.RemoveSession(clientIp); 

            Console.WriteLine($"[Auth] Access REVOKED for {clientIp}");
            return Ok(new { Status = "LoggedOut", Message = "Network Access Revoked" });
        }
    }
}