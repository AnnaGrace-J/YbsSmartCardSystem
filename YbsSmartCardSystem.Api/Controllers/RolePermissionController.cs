using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Contracts.Features.RolePermission;
using YbsSmartCardSystem.Domain.Features.RolePermission;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RolePermissionController : BaseController
{
    private readonly RolePermissionService _service;

    public RolePermissionController(RolePermissionService service)
    {
        _service = service;
    }

    [HttpGet("Roles")]
    [RequirePermission(PermissionCodes.RolePermissionView)]
    public IActionResult GetRoles([FromQuery] RoleListRequestModel request)
    {
        var result = _service.GetRoles(request);
        return Execute(result);
    }

    [HttpGet("Roles/{roleId}")]
    [RequirePermission(PermissionCodes.RolePermissionView)]
    public IActionResult GetRoleById(int roleId)
    {
        var result = _service.GetRoleById(roleId);
        return Execute(result);
    }

    [HttpPost("Roles")]
    [RequirePermission(PermissionCodes.RolePermissionManage)]
    public IActionResult CreateRole([FromBody] RoleCreateRequestModel request)
    {
        var result = _service.CreateRole(request);
        return Execute(result);
    }

    [HttpPatch("Roles/{roleId}")]
    [RequirePermission(PermissionCodes.RolePermissionManage)]
    public IActionResult PatchRole(int roleId, [FromBody] RolePatchRequestModel request)
    {
        var result = _service.PatchRole(roleId, request);
        return Execute(result);
    }

    [HttpDelete("Roles/{roleId}")]
    [RequirePermission(PermissionCodes.RolePermissionManage)]
    public IActionResult DeleteRole(int roleId)
    {
        var result = _service.DeleteRole(roleId);
        return Execute(result);
    }

    [HttpGet("Permissions")]
    [RequirePermission(PermissionCodes.RolePermissionView)]
    public IActionResult GetPermissions([FromQuery] PermissionListRequestModel request)
    {
        var result = _service.GetPermissions(request);
        return Execute(result);
    }

    [HttpGet("Users/{userId}/Roles")]
    [RequirePermission(PermissionCodes.RolePermissionView)]
    public IActionResult GetUserRoles(int userId)
    {
        var result = _service.GetUserRoles(userId);
        return Execute(result);
    }

    [HttpPut("Users/{userId}/Roles")]
    [RequirePermission(PermissionCodes.RolePermissionManage)]
    public IActionResult UpdateUserRoles(int userId, [FromBody] UserRoleUpdateRequestModel request)
    {
        if (userId != request.UserId)
            return BadRequest("User ID in route does not match User ID in body.");

        var result = _service.UpdateUserRoles(request);
        return Execute(result);
    }

    [HttpGet("Roles/{roleId}/Permissions")]
    [RequirePermission(PermissionCodes.RolePermissionView)]
    public IActionResult GetRolePermissions(int roleId)
    {
        var result = _service.GetRolePermissions(roleId);
        return Execute(result);
    }

    [HttpPut("Roles/{roleId}/Permissions")]
    [RequirePermission(PermissionCodes.RolePermissionManage)]
    public IActionResult UpdateRolePermissions(int roleId, [FromBody] RolePermissionUpdateRequestModel request)
    {
        if (roleId != request.RoleId)
            return BadRequest("Role ID in route does not match Role ID in body.");

        var result = _service.UpdateRolePermissions(request);
        return Execute(result);
    }
}
