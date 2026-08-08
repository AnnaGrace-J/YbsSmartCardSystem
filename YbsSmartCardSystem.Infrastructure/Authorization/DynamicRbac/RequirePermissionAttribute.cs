using Microsoft.AspNetCore.Authorization;

namespace YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = PolicyPrefix + permissionCode;
    }
}
