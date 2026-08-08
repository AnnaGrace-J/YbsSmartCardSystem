using System.Text.Json;
using YbsSmartCardSystem.Database.AppDbContextModels;

namespace YbsSmartCardSystem.Infrastructure.AuditLog;

public class AuditLogWriter : IAuditLogWriter
{
    private readonly AppDbContext _db;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditLogWriter(AppDbContext db)
    {
        _db = db;
    }

    public async Task WriteAsync(AuditLogWriteModel model, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.Action))
                return;

            if (string.IsNullOrWhiteSpace(model.FeatureName))
                return;

            var entry = new TblAuditLog
            {
                UserId          = model.UserId,
                Action          = model.Action.Trim(),
                FeatureName     = model.FeatureName.Trim(),
                EntityName      = model.EntityName?.Trim(),
                EntityId        = model.EntityId?.Trim(),
                OldValue        = Serialize(model.OldValue),
                NewValue        = Serialize(model.NewValue),
                IpAddress       = model.IpAddress,
                UserAgent       = model.UserAgent,
                CreatedDateTime = DateTime.Now
            };

            _db.TblAuditLogs.Add(entry);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Audit write failures must not break the main workflow
        }
    }

    private static string? Serialize(object? value)
    {
        if (value is null) return null;
        if (value is string s) return s;
        try
        {
            return JsonSerializer.Serialize(value, _jsonOptions);
        }
        catch
        {
            return value.ToString();
        }
    }
}
