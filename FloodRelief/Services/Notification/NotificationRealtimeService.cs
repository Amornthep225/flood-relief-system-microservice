using FloodRelief.Data;
using FloodRelief.Hubs;
using FloodRelief.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Services.Notification;

/// <summary>
/// บันทึกข้อมูลพร้อมแจ้ง SignalR เฉพาะผู้รับ Notification ที่เกี่ยวข้อง
/// ถ้าอยู่ใน DB transaction จะพัก event ไว้จน transaction commit สำเร็จ
/// เพื่อไม่ให้ frontend refresh ก่อนข้อมูลถูก commit จริง
/// </summary>
public sealed class NotificationRealtimeService
{
    private readonly AppDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationRealtimeService> _logger;
    private readonly HashSet<string> _pendingGroups = new(StringComparer.Ordinal);

    public NotificationRealtimeService(
        AppDbContext context,
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationRealtimeService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = GetPendingNotificationGroups();
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);

        if (groups.Count == 0)
        {
            return affectedRows;
        }

        if (_context.Database.CurrentTransaction != null)
        {
            foreach (var group in groups)
            {
                _pendingGroups.Add(group);
            }
        }
        else
        {
            await PublishGroupsAsync(groups, cancellationToken);
        }

        return affectedRows;
    }

    /// <summary>
    /// เรียกทันทีหลัง transaction.CommitAsync() สำเร็จ
    /// </summary>
    public async Task FlushAsync(
        CancellationToken cancellationToken = default)
    {
        if (_pendingGroups.Count == 0)
        {
            return;
        }

        var groups = _pendingGroups.ToArray();
        _pendingGroups.Clear();

        await PublishGroupsAsync(groups, cancellationToken);
    }

    public void DiscardPending()
    {
        _pendingGroups.Clear();
    }

    public Task NotifyUserAsync(
        string? userId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.CompletedTask;
        }

        return PublishGroupsAsync(
            new[] { UserGroup(userId) },
            cancellationToken
        );
    }

    public Task NotifyStaffAsync(
        string? staffId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(staffId))
        {
            return Task.CompletedTask;
        }

        return PublishGroupsAsync(
            new[] { StaffGroup(staffId) },
            cancellationToken
        );
    }

    private HashSet<string> GetPendingNotificationGroups()
    {
        var groups = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in _context.ChangeTracker.Entries<FloodRelief.Models.Notification>())
        {
            if (entry.State != EntityState.Added)
            {
                continue;
            }

            var notification = entry.Entity;

            if (!string.IsNullOrWhiteSpace(notification.UserId))
            {
                groups.Add(UserGroup(notification.UserId));
            }

            if (!string.IsNullOrWhiteSpace(notification.StaffId))
            {
                groups.Add(StaffGroup(notification.StaffId));
            }
        }

        return groups;
    }

    private async Task PublishGroupsAsync(
        IEnumerable<string> groups,
        CancellationToken cancellationToken)
    {
        var uniqueGroups = groups
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (uniqueGroups.Length == 0)
        {
            return;
        }

        try
        {
            var publishTasks = uniqueGroups.Select(group =>
                _hubContext.Clients
                    .Group(group)
                    .SendCoreAsync(
                        "NotificationChanged",
                        Array.Empty<object>(),
                        cancellationToken
                    )
            );

            await Task.WhenAll(publishTasks);
        }
        catch (Exception exception)
        {
            // Notification ถูกบันทึกใน DB แล้ว จึงไม่ควรทำให้ API หลักล้ม
            // เพียงเพราะ realtime push มีปัญหาชั่วคราว
            _logger.LogWarning(
                exception,
                "ส่ง SignalR notification ไม่สำเร็จสำหรับ {GroupCount} กลุ่ม",
                uniqueGroups.Length
            );
        }
    }

    private static string UserGroup(string userId) => $"user:{userId}";

    private static string StaffGroup(string staffId) => $"staff:{staffId}";
}
