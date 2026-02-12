using Microsoft.AspNetCore.Mvc;
using NAC.Contracts.Models;

namespace NAC.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            // TODO: LDAP validation (later)
            bool validCredentials =
                request.Username == "admin" &&
                request.Password == "admin";

            if (!validCredentials)
            {
                return Unauthorized(new LoginResponse
                {
                    Success = false,
                    Message = "Invalid username or password"
                });
            }

            return Ok(new LoginResponse
            {
                Success = true,
                Message = "Phase 1 passed"
            });
        }

        [HttpPost("phase2")]
        public ActionResult<Phase2ValidationResponse> Phase2([FromBody] Phase2ValidationRequest request)
        {
            bool versionOk = request.ClientVersion == "1.0.0";
            bool deviceOk = request.DeviceInfo != null;

            if (!versionOk || !deviceOk)
            {
                return BadRequest(new Phase2ValidationResponse
                {
                    Success = false,
                    Message = "Device does not meet security requirements"
                });
            }

            return Ok(new Phase2ValidationResponse
            {
                Success = true,
                Message = "Phase 2 passed"
            });
        }

        [HttpPost("heartbeat")]
        public ActionResult<HeartbeatResponse> Heartbeat([FromBody] HeartbeatRequest request)
        {
            // TODO: verify token from DB later
            bool tokenValid = !string.IsNullOrWhiteSpace(request.SessionToken);

            if (!tokenValid)
            {
                return Unauthorized(new HeartbeatResponse
                {
                    Success = false,
                    Message = "Session expired"
                });
            }

            return Ok(new HeartbeatResponse
            {
                Success = true,
                Message = "Session valid"
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // TODO: invalidate token in DB
            return Ok();
        }
    }
}
    