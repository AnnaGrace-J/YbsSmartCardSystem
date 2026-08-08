using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Contracts.Features.AuditLog;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Common;

namespace YbsSmartCardSystem.Domain.Features.AuditLog;

public class AuditLogService
{
    private readonly AppDbContext _db;

    public AuditLogService(AppDbContext db)
    {
        _db = db;
    }

    public Result<AuditLogListResponseModel> GetList(AuditLogListRequestModel request)
    {
        try
        {
            request ??= new AuditLogListRequestModel();

            if (request.PageNo <= 0)
                return new Result<AuditLogListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageNo must be greater than 0." };
            if (request.PageSize <= 0)
                return new Result<AuditLogListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageSize must be greater than 0." };
            if (request.PageSize > 100)
                request.PageSize = 100;

            var query = _db.TblAuditLogs
                .AsNoTracking()
                .AsQueryable();

            if (request.UserId.HasValue)
                query = query.Where(x => x.UserId == request.UserId.Value);

            if (!string.IsNullOrWhiteSpace(request.Action))
                query = query.Where(x => x.Action.Contains(request.Action.Trim()));

            if (!string.IsNullOrWhiteSpace(request.FeatureName))
                query = query.Where(x => x.FeatureName.Contains(request.FeatureName.Trim()));

            if (!string.IsNullOrWhiteSpace(request.EntityName))
                query = query.Where(x => x.EntityName != null && x.EntityName.Contains(request.EntityName.Trim()));

            if (request.FromDate.HasValue)
                query = query.Where(x => x.CreatedDateTime >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.CreatedDateTime <= request.ToDate.Value.AddDays(1));

            var totalCount = query.Count();

            var logs = query
                .OrderByDescending(x => x.CreatedDateTime)
                .ThenByDescending(x => x.AuditLogId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new AuditLogModel
                {
                    AuditLogId      = x.AuditLogId,
                    UserId          = x.UserId,
                    UserName        = null, // Note: You'll need to join manually if you want the username, or look it up post-query
                    Action          = x.Action,
                    FeatureName     = x.FeatureName,
                    EntityName      = x.EntityName,
                    EntityId        = x.EntityId,
                    OldValue        = x.OldValue,
                    NewValue        = x.NewValue,
                    IpAddress       = x.IpAddress,
                    UserAgent       = x.UserAgent,
                    CreatedDateTime = x.CreatedDateTime
                })
                .ToList();

            return new Result<AuditLogListResponseModel>
            {
                IsSuccess = true,
                Message   = "Audit logs retrieved successfully.",
                Data      = new AuditLogListResponseModel
                {
                    TotalCount = totalCount,
                    Logs       = logs
                }
            };
        }
        catch (Exception)
        {
            return new Result<AuditLogListResponseModel>
            {
                IsSuccess  = false,
                StatusCode = 500,
                Message    = "An unexpected error occurred."
            };
        }
    }
}
