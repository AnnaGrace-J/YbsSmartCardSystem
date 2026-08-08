using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Database.AppDbContextModels;

using YbsSmartCardSystem.Infrastructure.Services;

namespace YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;

public class PermissionChecker : IPermissionChecker
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public PermissionChecker(AppDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default)
    {
        if (_currentUserService.IsViewer)
        {
            return permissionCode == "Bus.View" || 
                   permissionCode == "Terminal.View" || 
                   permissionCode == "ViewerDashboard.View";
        }
        // User must exist, be active, and not deleted
        var userExists = await _db.TblStaffUsers
            .AsNoTracking()
            .AnyAsync(u => u.StaffUserId == userId && u.IsActive && !u.DeleteFlag, cancellationToken);

        if (!userExists)
            return false;

        // Check permission through active user → role → permission chain
        var hasPermission = await _db.TblUserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId && !ur.DeleteFlag)
            .Join(_db.TblRoles.Where(r => r.IsActive && !r.DeleteFlag),
                ur => ur.RoleId,
                r  => r.RoleId,
                (ur, r) => r)
            .Join(_db.TblRolePermissions.Where(rp => !rp.DeleteFlag),
                r  => r.RoleId,
                rp => rp.RoleId,
                (r, rp) => rp)
            .Join(_db.TblPermissions.Where(p => p.IsActive && !p.DeleteFlag && p.PermissionCode == permissionCode),
                rp => rp.PermissionId,
                p  => p.PermissionId,
                (rp, p) => p)
            .AnyAsync(cancellationToken);

        return hasPermission;
    }
}
