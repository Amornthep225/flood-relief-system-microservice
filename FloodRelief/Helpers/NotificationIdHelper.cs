using FloodRelief.Data;
using Microsoft.EntityFrameworkCore;

namespace FloodRelief.Helpers;

/// <summary>
/// Generates a 10-digit Notification ID and prevents duplicate IDs
/// between concurrent requests inside the same backend process.
/// </summary>
public static class NotificationIdHelper
{
    private const int Digits = 10;
    private const long MaximumId = 9_999_999_999;

    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static long? _lastIssuedId;

    public static Task<string> GenerateNextIdAsync(
        AppDbContext context,
        CancellationToken cancellationToken = default)
    {
        return GenerateNextIdAsync(
            context.Notifications,
            cancellationToken
        );
    }

    // Compatibility overload for code that passes _context.Notifications directly.
    public static async Task<string> GenerateNextIdAsync(
        DbSet<FloodRelief.Models.Notification> notifications,
        CancellationToken cancellationToken = default)
    {
        await Gate.WaitAsync(cancellationToken);

        try
        {
            if (!_lastIssuedId.HasValue)
            {
                var lastId = await notifications
                    .AsNoTracking()
                    .OrderByDescending(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(lastId))
                {
                    _lastIssuedId = 0;
                }
                else if (
                    lastId.Length == Digits &&
                    lastId.All(char.IsDigit) &&
                    long.TryParse(lastId, out var parsedId) &&
                    parsedId >= 0)
                {
                    _lastIssuedId = parsedId;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Notification ID ล่าสุดไม่ถูกต้อง: {lastId} (ต้องเป็นตัวเลข {Digits} หลัก)"
                    );
                }
            }

            if (_lastIssuedId.Value >= MaximumId)
            {
                throw new InvalidOperationException(
                    $"Notification ID เต็มแล้ว หรืออยู่นอกช่วง {Digits} หลัก"
                );
            }

            _lastIssuedId++;
            return _lastIssuedId.Value.ToString($"D{Digits}");
        }
        finally
        {
            Gate.Release();
        }
    }
}
