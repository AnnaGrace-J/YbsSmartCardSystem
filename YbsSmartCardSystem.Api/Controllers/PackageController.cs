using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.Package;
using YbsSmartCardSystem.Domain.Features.Package;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PackageController : BaseController
{
    private readonly PackageService _packageService;

    public PackageController(PackageService packageService)
    {
        _packageService = packageService;
    }

    [HttpGet]
    public IActionResult GetPackages([FromQuery] PackageListRequestModel request)
    {
        var result = _packageService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetPackage(int id)
    {
        var result = _packageService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    public IActionResult CreatePackage([FromBody] PackageCreateRequestModel request)
    {
        var result = _packageService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    public IActionResult PatchPackage(int id, [FromBody] PackagePatchRequestModel request)
    {
        var result = _packageService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    public IActionResult DeletePackage(int id)
    {
        var result = _packageService.Delete(id);
        return Execute(result);
    }
}
