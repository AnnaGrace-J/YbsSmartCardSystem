namespace YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);
}
