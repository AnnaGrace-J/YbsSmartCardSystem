using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Domain.Features.Bus;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusController : BaseController
{
    private readonly BusService _busService;

    public BusController(BusService busService)
    {
        _busService = busService;
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.BusView)]
    public IActionResult BusList([FromQuery] BusListRequestModel request)
    {
        var result = _busService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    [RequirePermission(PermissionCodes.BusView)]
    public IActionResult BusGetById(int id)
    {
        var result = _busService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.BusCreate)]
    public IActionResult BusCreate([FromBody] BusCreateRequestModel request)
    {
        var result = _busService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    [RequirePermission(PermissionCodes.BusUpdate)]
    public IActionResult BusPatch(int id, [FromBody] BusPatchRequestModel request)
    {
        var result = _busService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    [RequirePermission(PermissionCodes.BusDelete)]
    public IActionResult BusDelete(int id)
    {
        var result = _busService.Delete(id);
        return Execute(result);
    }
}
