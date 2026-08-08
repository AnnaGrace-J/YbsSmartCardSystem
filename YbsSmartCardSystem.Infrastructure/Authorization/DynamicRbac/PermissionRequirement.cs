using Microsoft.AspNetCore.Authorization;

namespace YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public string PermissionCode { get; }
}
