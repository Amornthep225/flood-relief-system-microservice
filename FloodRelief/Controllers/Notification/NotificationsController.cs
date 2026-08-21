using FloodRelief.Services.Notification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FloodRelief.Controllers.Notification
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize(Roles = "User,Staff")]
    public class NotificationsController : ControllerBase
    {
        private readonly NotificationsService _service;

        public NotificationsController(NotificationsService service)
        {
            _service = service;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int take = 20)
        {
            return await _service.GetMyNotifications(take);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(string id)
        {
            return await _service.MarkAsRead(id);
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            return await _service.MarkAllAsRead();
        }
    }
}
