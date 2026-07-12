using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Bus;
using YbsSmartCardSystem.Domain.Features.Bus.Models;

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
    public IActionResult BusList([FromQuery] BusListRequestModel request)
    {
        var result = _busService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    public IActionResult BusGetById(int id)
    {
        var result = _busService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    public IActionResult BusCreate([FromBody] BusCreateRequestModel request)
    {
        var result = _busService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    public IActionResult BusPatch(int id, [FromBody] BusPatchRequestModel request)
    {
        var result = _busService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    public IActionResult BusDelete(int id)
    {
        var result = _busService.Delete(id);
        return Execute(result);
    }
}
