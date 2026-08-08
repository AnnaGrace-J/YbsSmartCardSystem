namespace YbsSmartCardSystem.Infrastructure.AuditLog;

public class AuditLogWriteModel
{
    public int?    UserId      { get; set; }
    public string  Action      { get; set; } = string.Empty;
    public string  FeatureName { get; set; } = string.Empty;
    public string? EntityName  { get; set; }
    public string? EntityId    { get; set; }
    public object? OldValue    { get; set; }
    public object? NewValue    { get; set; }
    public string? IpAddress   { get; set; }
    public string? UserAgent   { get; set; }
}
