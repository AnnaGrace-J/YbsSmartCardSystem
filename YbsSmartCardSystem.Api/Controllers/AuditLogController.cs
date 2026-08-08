using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.AuditLog;
using YbsSmartCardSystem.Domain.Features.AuditLog;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuditLogController : BaseController
{
    private readonly AuditLogService _auditLogService;

    public AuditLogController(AuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.AuditLogView)]
    public IActionResult GetAuditLogs([FromQuery] AuditLogListRequestModel request)
    {
        var result = _auditLogService.GetList(request);
        return Execute(result);
    }
}
