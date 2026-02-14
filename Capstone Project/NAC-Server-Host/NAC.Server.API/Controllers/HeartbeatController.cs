using Microsoft.AspNetCore.Mvc;
using NAC.Server.Core.Interfaces;
using NAC.Server.Core.Models;

namespace NAC.Server.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // URL: https://server:5000/api/heartbeat
    public class HeartbeatController : ControllerBase
    {
        private readonly ISessionManager _sessionManager;

        // Constructor: We ask the system to give us the SessionManager
        public HeartbeatController(ISessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        [HttpPost]
        public IActionResult ReceiveHeartbeat([FromBody] HeartbeatRequest request)
        {
            if (string.IsNullOrEmpty(request.SessionToken))
            {
                return BadRequest("Missing Session Token");
            }

            // Use DeviceName (or DeviceId if you prefer) to track the session
            string deviceId = request.DeviceInfo?.DeviceName ?? "UnknownDevice";

            // Update the "Last Seen" time so they don't get kicked off
            _sessionManager.UpdateHeartbeat(deviceId);

            return Ok(new { IsValid = true, Message = "Heartbeat Acknowledged" });
        }
    }
}