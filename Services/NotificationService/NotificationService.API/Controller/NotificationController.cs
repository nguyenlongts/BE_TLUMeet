using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.API.Services;
using System.Security.Claims;

namespace NotificationService.API.Controllers
{
    [ApiController]
    [Route("api/notification")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationAppService _service;

        public NotificationController(INotificationAppService service)
        {
            _service = service;
        }

        private string? GetEmail() =>
            User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var email = GetEmail();
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var result = await _service.GetByEmailAsync(email);
            return Ok(result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var email = GetEmail();
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var success = await _service.MarkAsReadAsync(id, email);
            return success ? NoContent() : NotFound();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var email = GetEmail();
            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            await _service.MarkAllAsReadAsync(email);
            return NoContent();
        }
    }
}