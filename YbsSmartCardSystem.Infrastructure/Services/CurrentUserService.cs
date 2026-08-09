using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace YbsSmartCardSystem.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out var id))
                return id;
            return null;
        }
    }

    public string? UserName =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? PhoneNumber =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.MobilePhone)?.Value ??
        _httpContextAccessor.HttpContext?.User?.FindFirst("PhoneNumber")?.Value;

    public string? UserType =>
        _httpContextAccessor.HttpContext?.User?.FindFirst("UserType")?.Value;

    public bool IsStaff => string.Equals(UserType, "Staff", StringComparison.OrdinalIgnoreCase);

    public bool IsViewer => string.Equals(UserType, "Viewer", StringComparison.OrdinalIgnoreCase);

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? IpAddress =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
