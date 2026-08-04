using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FloodRelief.Helpers;

/// <summary>
/// สร้างและตรวจสอบ Primary Key แบบตัวเลขที่เก็บเป็น string และเติมเลขศูนย์ด้านหน้า
/// เช่น 0000000001 หรือ 01
/// </summary>
public static class PrimaryKeyHelper
{
    public static async Task<string> GenerateNextIdAsync<TEntity>(
        IQueryable<TEntity> query,
        Expression<Func<TEntity, string>> idSelector,
        string entityName,
        int digits = 10,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        ValidateDigits(digits);

        var lastId = await query
            .OrderByDescending(idSelector)
            .Select(idSelector)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(lastId))
        {
            return FormatId(1, digits, entityName);
        }

        var currentNumber = ParseId(lastId, digits, entityName);
        return FormatId(currentNumber + 1, digits, entityName);
    }

    public static string IncrementId(
        string currentId,
        string entityName,
        int digits = 10)
    {
        ValidateDigits(digits);

        var currentNumber = ParseId(currentId, digits, entityName);
        return FormatId(currentNumber + 1, digits, entityName);
    }

    public static bool IsValidId(string? id, int digits = 10)
    {
        if (string.IsNullOrWhiteSpace(id) || digits <= 0)
        {
            return false;
        }

        return id.Length == digits &&
               id.All(char.IsDigit) &&
               long.TryParse(id, out var number) &&
               number > 0;
    }

    private static long ParseId(
        string id,
        int digits,
        string entityName)
    {
        if (string.IsNullOrWhiteSpace(id) ||
            id.Length != digits ||
            !id.All(char.IsDigit) ||
            !long.TryParse(id, out var number) ||
            number <= 0)
        {
            throw new InvalidOperationException(
                $"รหัส {entityName} ไม่ถูกต้อง: {id} (ต้องเป็นตัวเลข {digits} หลัก)");
        }

        return number;
    }

    private static string FormatId(
        long number,
        int digits,
        string entityName)
    {
        var maximum = GetMaximumValue(digits);

        if (number <= 0 || number > maximum)
        {
            throw new InvalidOperationException(
                $"รหัส {entityName} เต็มแล้ว หรืออยู่นอกช่วง {digits} หลัก");
        }

        return number.ToString($"D{digits}");
    }

    private static long GetMaximumValue(int digits)
    {
        ValidateDigits(digits);

        // long รองรับได้สูงสุด 18 หลักอย่างปลอดภัยสำหรับรูปแบบนี้
        return (long)Math.Pow(10, digits) - 1;
    }

    private static void ValidateDigits(int digits)
    {
        if (digits is < 1 or > 18)
        {
            throw new ArgumentOutOfRangeException(
                nameof(digits),
                "จำนวนหลักของรหัสต้องอยู่ระหว่าง 1 ถึง 18");
        }
    }
}
