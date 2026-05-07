using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JitsiBackend.API.Application.Common;

namespace JitsiBackend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JaasController : ControllerBase
    {
        private readonly JaasTokenService _jaasService;

        public JaasController()
        {
            _jaasService = new JaasTokenService();
        }

        [HttpGet("token")]
        public IActionResult GetToken([FromQuery] string? room = "*", [FromQuery] string? userName = "nguyen.long.ts.bn")
        {
            try
            {
                var jwt = _jaasService.GenerateToken(room, userName);
                return Ok(new { token = jwt });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        [Authorize]
        [HttpPost("generate-token")]
        public IActionResult GenerateToken([FromBody] GenerateTokenRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RoomName))
                {
                    return BadRequest(new { error = "Room name is required" });
                }

                if (string.IsNullOrWhiteSpace(request.UserName))
                {
                    return BadRequest(new { error = "User name is required" });
                }

                var token = _jaasService.GenerateToken(
                    roomName: request.RoomName,
                    userName: request.UserName,
                    email: request.Email,
                    avatarUrl: request.AvatarUrl,
                    isModerator: request.IsModerator,
                    expiresInMinutes: request.ExpiresInMinutes ?? 120
                );

                var config = _jaasService.GetConfig();
                var meetingUrl = _jaasService.GetMeetingUrl(request.RoomName);

                return Ok(new GenerateTokenResponse
                {
                    Success = true,
                    Token = token,
                    AppId = config.AppId,
                    RoomName = request.RoomName,
                    MeetingUrl = meetingUrl,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(request.ExpiresInMinutes ?? 120)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    error = "Failed to generate token",
                    message = ex.Message
                });
            }
        }
    }

    public class GenerateTokenRequest
    {

        public string RoomName { get; set; }


        public string UserName { get; set; }




        public string Email { get; set; }


        public string AvatarUrl { get; set; }


        public bool IsModerator { get; set; } = false;

        public int? ExpiresInMinutes { get; set; } = 120;
    }

    public class GenerateTokenResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string AppId { get; set; }
        public string RoomName { get; set; }
        public string MeetingUrl { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
