using FloodRelief.Data;
using FloodRelief.DTOs.Notification;
using FloodRelief.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services.Notification
{
    public class NotificationsService : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CurrentUserService _currentUser;

        public NotificationsService(
            AppDbContext context,
            CurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<IActionResult> GetMyNotifications(int take = 20)
        {
            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้งาน"
                });
            }

            take = Math.Clamp(take, 1, 50);

            var query = _context.Notifications
                .AsNoTracking()
                .Where(x => x.UserId == userId);

            var unreadCount = await query.CountAsync(x => !x.IsRead);

            var notifications = await query
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    Type = x.Type,
                    Title = x.Title,
                    Message = x.Message,
                    ReferenceType = x.ReferenceType,
                    ReferenceId = x.ReferenceId,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt,
                    ReadAt = x.ReadAt
                })
                .ToListAsync();

            return Ok(new
            {
                unreadCount,
                notifications
            });
        }

        public async Task<IActionResult> MarkAsRead(string id)
        {
            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้งาน"
                });
            }

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.UserId == userId
                );

            if (notification == null)
            {
                return NotFound(new
                {
                    message = "ไม่พบการแจ้งเตือน"
                });
            }

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "อ่านการแจ้งเตือนแล้ว",
                notificationId = notification.Id
            });
        }

        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _currentUser.UserId;

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new
                {
                    message = "ไม่พบข้อมูลผู้ใช้งาน"
                });
            }

            var unreadNotifications = await _context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead
                )
                .ToListAsync();

            if (unreadNotifications.Count == 0)
            {
                return Ok(new
                {
                    message = "ไม่มีการแจ้งเตือนที่ยังไม่ได้อ่าน",
                    updatedCount = 0
                });
            }

            var now = DateTime.Now;

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = now;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "อ่านการแจ้งเตือนทั้งหมดแล้ว",
                updatedCount = unreadNotifications.Count
            });
        }
    }
}
