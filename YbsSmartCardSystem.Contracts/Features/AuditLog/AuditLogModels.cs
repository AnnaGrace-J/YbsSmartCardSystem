namespace YbsSmartCardSystem.Contracts.Features.AuditLog;

public class AuditLogListRequestModel
{
    public int    PageNo      { get; set; } = 1;
    public int    PageSize    { get; set; } = 20;
    public int?   UserId      { get; set; }
    public string? Action     { get; set; }
    public string? FeatureName { get; set; }
    public string? EntityName  { get; set; }
    public DateTime? FromDate  { get; set; }
    public DateTime? ToDate    { get; set; }
}

public class AuditLogListResponseModel
{
    public int             TotalCount { get; set; }
    public List<AuditLogModel> Logs   { get; set; } = [];
}

public class AuditLogModel
{
    public long      AuditLogId       { get; set; }
    public int?      UserId           { get; set; }
    public string?   UserName         { get; set; }
    public string    Action           { get; set; } = string.Empty;
    public string    FeatureName      { get; set; } = string.Empty;
    public string?   EntityName       { get; set; }
    public string?   EntityId         { get; set; }
    public string?   OldValue         { get; set; }
    public string?   NewValue         { get; set; }
    public string?   IpAddress        { get; set; }
    public string?   UserAgent        { get; set; }
    public DateTime  CreatedDateTime  { get; set; }
}
