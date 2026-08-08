using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Terminal;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TerminalController : BaseController
{
    private readonly TerminalService _terminalService;

    public TerminalController(TerminalService terminalService)
    {
        _terminalService = terminalService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.TerminalView)]
    public IActionResult TerminalList([FromQuery] TerminalListRequestModel request)
    {
        var result = _terminalService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.TerminalView)]
    public IActionResult TerminalGetById(int id)
    {
        var result = _terminalService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.TerminalCreate)]
    public IActionResult TerminalCreate([FromBody] TerminalCreateRequestModel request)
    {
        var result = _terminalService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    [RequirePermission(PermissionCodes.TerminalUpdate)]
    public IActionResult TerminalPatch(int id, [FromBody] TerminalPatchRequestModel request)
    {
        var result = _terminalService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(PermissionCodes.TerminalDelete)]
    public IActionResult TerminalDelete(int id)
    {
        var result = _terminalService.Delete(id);
        return Execute(result);
    }
}
