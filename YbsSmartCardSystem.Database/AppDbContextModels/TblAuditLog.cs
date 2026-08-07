using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblAuditLog
{
    public long AuditLogId { get; set; }

    public int? UserId { get; set; }

    public string Action { get; set; } = null!;

    public string FeatureName { get; set; } = null!;

    public string? EntityName { get; set; }

    public string? EntityId { get; set; }

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime CreatedDateTime { get; set; }

    public virtual TblUser? User { get; set; }
}
