using System.Security.Claims;

namespace FloodRelief.Services.Common;

/// <summary>
/// อ่านข้อมูลผู้ใช้งานปัจจุบันจาก JWT Claims โดยไม่ผูก Service เข้ากับ ControllerContext
/// </summary>
public sealed class CurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? CenterId =>
        Principal?.FindFirstValue("CenterId")
        ?? Principal?.FindFirstValue("centerId");

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) == true;
}
