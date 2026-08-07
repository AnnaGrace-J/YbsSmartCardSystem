using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Terminal;
using YbsSmartCardSystem.Contracts.Features.BusPayment;

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
    public IActionResult TerminalList([FromQuery] TerminalListRequestModel request)
    {
        var result = _terminalService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    public IActionResult TerminalGetById(int id)
    {
        var result = _terminalService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    public IActionResult TerminalCreate([FromBody] TerminalCreateRequestModel request)
    {
        var result = _terminalService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    public IActionResult TerminalPatch(int id, [FromBody] TerminalPatchRequestModel request)
    {
        var result = _terminalService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    public IActionResult TerminalDelete(int id)
    {
        var result = _terminalService.Delete(id);
        return Execute(result);
    }
}
