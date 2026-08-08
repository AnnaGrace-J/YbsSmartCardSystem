namespace YbsSmartCardSystem.Infrastructure.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    string? PhoneNumber { get; }
    string? UserType { get; }
    bool IsStaff { get; }
    bool IsViewer { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    string? UserAgent { get; }
}
