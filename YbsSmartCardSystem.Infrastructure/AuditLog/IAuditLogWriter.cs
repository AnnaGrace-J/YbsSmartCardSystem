namespace YbsSmartCardSystem.Infrastructure.AuditLog;

public interface IAuditLogWriter
{
    Task WriteAsync(AuditLogWriteModel model, CancellationToken cancellationToken = default);
}
