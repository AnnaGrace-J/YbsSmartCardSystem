using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Contracts.Features.RolePermission;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Common;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Services;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Domain.Features.RolePermission;

public class RolePermissionService
{
    private readonly AppDbContext _db;
    private readonly IAuditLogWriter _audit;
    private readonly ICurrentUserService _currentUser;

    public RolePermissionService(AppDbContext db, IAuditLogWriter audit, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _currentUser = currentUser;
    }

    public Result<RoleListResponseModel> GetRoles(RoleListRequestModel request)
    {
        try
        {
            request ??= new RoleListRequestModel();

            if (request.PageNo <= 0)
                return new Result<RoleListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageNo must be greater than 0." };
            if (request.PageSize <= 0 || request.PageSize > 100)
                return new Result<RoleListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageSize must be between 1 and 100." };

            var query = _db.TblRoles.AsNoTracking().Where(x => !x.DeleteFlag);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(x => x.RoleCode.Contains(request.Search) || x.RoleName.Contains(request.Search));
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = query.Count();
            var items = query
                .OrderByDescending(x => x.RoleId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new RoleModel
                {
                    RoleId = x.RoleId,
                    RoleCode = x.RoleCode,
                    RoleName = x.RoleName,
                    Description = x.Description,
                    IsSystemRole = x.IsSystemRole,
                    IsActive = x.IsActive
                })
                .ToList();

            var response = new RoleListResponseModel
            {
                TotalCount = totalCount,
                Roles = items
            };

            return new Result<RoleListResponseModel> { IsSuccess = true, Data = response, Message = "Roles retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RoleListResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RoleModel> GetRoleById(int roleId)
    {
        try
        {
            if (roleId <= 0)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid Role ID." };

            var role = _db.TblRoles.AsNoTracking().FirstOrDefault(x => x.RoleId == roleId && !x.DeleteFlag);
            if (role == null)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 404, Message = "Role not found." };

            var model = new RoleModel
            {
                RoleId = role.RoleId,
                RoleCode = role.RoleCode,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive
            };

            return new Result<RoleModel> { IsSuccess = true, Data = model, Message = "Role retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RoleModel> CreateRole(RoleCreateRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RoleCode) || request.RoleCode.Length > 50)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "RoleCode is required and max 50 characters." };
            if (string.IsNullOrWhiteSpace(request.RoleName) || request.RoleName.Length > 100)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "RoleName is required and max 100 characters." };
            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 250)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Description max 250 characters." };

            if (_db.TblRoles.Any(x => x.RoleCode == request.RoleCode && x.IsActive && !x.DeleteFlag))
            {
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 409, Message = "An active Role with this code already exists." };
            }

            var entity = new TblRole
            {
                RoleCode = request.RoleCode,
                RoleName = request.RoleName,
                Description = request.Description,
                IsSystemRole = false,
                IsActive = request.IsActive,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };

            _db.TblRoles.Add(entity);
            _db.SaveChanges();

            var model = new RoleModel
            {
                RoleId = entity.RoleId,
                RoleCode = entity.RoleCode,
                RoleName = entity.RoleName,
                Description = entity.Description,
                IsSystemRole = entity.IsSystemRole,
                IsActive = entity.IsActive
            };

            return new Result<RoleModel> { IsSuccess = true, Data = model, Message = "Role created successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RoleModel> PatchRole(int roleId, RolePatchRequestModel request)
    {
        try
        {
            if (request == null)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Request is required." };
            if (roleId <= 0)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid Role ID." };

            var role = _db.TblRoles.FirstOrDefault(x => x.RoleId == roleId && !x.DeleteFlag);
            if (role == null)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 404, Message = "Role not found." };

            bool hasChanges = false;

            if (request.RoleCode != null)
            {
                if (role.IsSystemRole)
                    return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Cannot change the RoleCode of a System Role." };
                if (string.IsNullOrWhiteSpace(request.RoleCode) || request.RoleCode.Length > 50)
                    return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "RoleCode is required and max 50 characters." };
                
                role.RoleCode = request.RoleCode;
                hasChanges = true;
            }

            if (request.RoleName != null)
            {
                if (role.IsSystemRole)
                    return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Cannot change the RoleName of a System Role." };
                if (string.IsNullOrWhiteSpace(request.RoleName) || request.RoleName.Length > 100)
                    return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "RoleName is required and max 100 characters." };
                
                role.RoleName = request.RoleName;
                hasChanges = true;
            }

            if (request.Description != null)
            {
                if (!string.IsNullOrWhiteSpace(request.Description) && request.Description.Length > 250)
                    return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Description max 250 characters." };
                role.Description = request.Description;
                hasChanges = true;
            }

            if (request.IsActive.HasValue)
            {
                role.IsActive = request.IsActive.Value;
                hasChanges = true;
            }

            if (!hasChanges)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "At least one field must be supplied for patch." };

            if (_db.TblRoles.Any(x => x.RoleCode == role.RoleCode && x.IsActive && !x.DeleteFlag && x.RoleId != roleId))
            {
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 409, Message = "Another active Role with this code already exists." };
            }

            role.UpdatedDate = DateTime.Now;
            _db.SaveChanges();

            var model = new RoleModel
            {
                RoleId = role.RoleId,
                RoleCode = role.RoleCode,
                RoleName = role.RoleName,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive
            };

            return new Result<RoleModel> { IsSuccess = true, Data = model, Message = "Role updated successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RoleModel> DeleteRole(int roleId)
    {
        try
        {
            if (roleId <= 0)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid Role ID." };

            var role = _db.TblRoles.FirstOrDefault(x => x.RoleId == roleId && !x.DeleteFlag);
            if (role == null)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 404, Message = "Role not found." };

            if (role.IsSystemRole)
                return new Result<RoleModel> { IsSuccess = false, StatusCode = 409, Message = "System Roles cannot be deleted." };

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                role.DeleteFlag = true;
                role.UpdatedDate = DateTime.Now;

                // Soft delete user role assignments
                var userRoles = _db.TblUserRoles.Where(x => x.RoleId == roleId && !x.DeleteFlag);
                foreach (var ur in userRoles)
                {
                    ur.DeleteFlag = true;
                }

                // Soft delete role permission assignments
                var rolePermissions = _db.TblRolePermissions.Where(x => x.RoleId == roleId && !x.DeleteFlag);
                foreach (var rp in rolePermissions)
                {
                    rp.DeleteFlag = true;
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return new Result<RoleModel> { IsSuccess = true, Data = new RoleModel(), Message = "Role deleted successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RoleModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<PermissionListResponseModel> GetPermissions(PermissionListRequestModel request)
    {
        try
        {
            request ??= new PermissionListRequestModel();

            if (request.PageNo <= 0)
                return new Result<PermissionListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageNo must be greater than 0." };
            if (request.PageSize <= 0 || request.PageSize > 100)
                return new Result<PermissionListResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PageSize must be between 1 and 100." };

            var query = _db.TblPermissions.AsNoTracking().Where(x => !x.DeleteFlag);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(x => x.PermissionCode.Contains(request.Search) || x.PermissionName.Contains(request.Search));
            }

            if (!string.IsNullOrWhiteSpace(request.FeatureName))
            {
                query = query.Where(x => x.FeatureName == request.FeatureName);
            }

            if (request.IsActive.HasValue)
            {
                query = query.Where(x => x.IsActive == request.IsActive.Value);
            }

            var totalCount = query.Count();
            var items = query
                .OrderBy(x => x.PermissionId)
                .Skip((request.PageNo - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new PermissionModel
                {
                    PermissionId = x.PermissionId,
                    PermissionCode = x.PermissionCode,
                    PermissionName = x.PermissionName,
                    FeatureName = x.FeatureName,
                    ActionName = x.ActionName,
                    Description = x.Description,
                    IsActive = x.IsActive
                })
                .ToList();

            var response = new PermissionListResponseModel
            {
                TotalCount = totalCount,
                Permissions = items
            };

            return new Result<PermissionListResponseModel> { IsSuccess = true, Data = response, Message = "Permissions retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<PermissionListResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<UserRoleResponseModel> GetUserRoles(int userId)
    {
        try
        {
            if (userId <= 0)
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid User ID." };

            var user = _db.TblStaffUsers.AsNoTracking().FirstOrDefault(x => x.StaffUserId == userId && !x.DeleteFlag);
            if (user == null)
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 404, Message = "User not found." };

            var userRoles = _db.TblUserRoles
                .AsNoTracking()
                .Include(x => x.Role)
                .Where(x => x.UserId == userId && !x.DeleteFlag && !x.Role.DeleteFlag)
                .Select(x => new RoleModel
                {
                    RoleId = x.Role.RoleId,
                    RoleCode = x.Role.RoleCode,
                    RoleName = x.Role.RoleName,
                    Description = x.Role.Description,
                    IsSystemRole = x.Role.IsSystemRole,
                    IsActive = x.Role.IsActive
                })
                .ToList();

            var response = new UserRoleResponseModel
            {
                UserId = user.StaffUserId,
                UserName = user.UserName,
                Roles = userRoles
            };

            return new Result<UserRoleResponseModel> { IsSuccess = true, Data = response, Message = "User roles retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<UserRoleResponseModel> UpdateUserRoles(UserRoleUpdateRequestModel request)
    {
        try
        {
            if (request == null)
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Request is required." };
            if (request.UserId <= 0)
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid User ID." };

            var user = _db.TblStaffUsers.FirstOrDefault(x => x.StaffUserId == request.UserId && !x.DeleteFlag);
            if (user == null)
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 404, Message = "User not found." };

            if (request.RoleIds.Any(roleId => !_db.TblRoles.Any(r => r.RoleId == roleId && r.IsActive && !r.DeleteFlag)))
            {
                return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 400, Message = "One or more role IDs are invalid or inactive." };
            }

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var existingUserRoles = _db.TblUserRoles.Where(x => x.UserId == request.UserId && !x.DeleteFlag).ToList();

                // Soft delete ones not in new list
                foreach (var eur in existingUserRoles)
                {
                    if (!request.RoleIds.Contains(eur.RoleId))
                    {
                        eur.DeleteFlag = true;
                    }
                }

                // Add missing ones
                foreach (var roleId in request.RoleIds)
                {
                    var eur = existingUserRoles.FirstOrDefault(x => x.RoleId == roleId);
                    if (eur == null)
                    {
                        _db.TblUserRoles.Add(new TblUserRole
                        {
                            UserId = request.UserId,
                            RoleId = roleId,
                            CreatedDate = DateTime.Now,
                            DeleteFlag = false
                        });
                    }
                    else if (eur.DeleteFlag)
                    {
                        // Reactivate if previously soft deleted
                        eur.DeleteFlag = false;
                        eur.CreatedDate = DateTime.Now;
                    }
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = _currentUser.UserId,
                Action      = AuditActions.RoleChanged,
                FeatureName = "RolePermission",
                EntityName  = "TblUserRole",
                EntityId    = request.UserId.ToString(),
                NewValue    = new { request.UserId, RoleIds = request.RoleIds },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            return GetUserRoles(request.UserId);
        }
        catch (Exception)
        {
            return new Result<UserRoleResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RolePermissionResponseModel> GetRolePermissions(int roleId)
    {
        try
        {
            if (roleId <= 0)
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid Role ID." };

            var role = _db.TblRoles.AsNoTracking().FirstOrDefault(x => x.RoleId == roleId && !x.DeleteFlag);
            if (role == null)
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 404, Message = "Role not found." };

            var rolePermissions = _db.TblRolePermissions
                .AsNoTracking()
                .Include(x => x.Permission)
                .Where(x => x.RoleId == roleId && !x.DeleteFlag && !x.Permission.DeleteFlag)
                .Select(x => new PermissionModel
                {
                    PermissionId = x.Permission.PermissionId,
                    PermissionCode = x.Permission.PermissionCode,
                    PermissionName = x.Permission.PermissionName,
                    FeatureName = x.Permission.FeatureName,
                    ActionName = x.Permission.ActionName,
                    Description = x.Permission.Description,
                    IsActive = x.Permission.IsActive
                })
                .ToList();

            var response = new RolePermissionResponseModel
            {
                RoleId = role.RoleId,
                RoleCode = role.RoleCode,
                Permissions = rolePermissions
            };

            return new Result<RolePermissionResponseModel> { IsSuccess = true, Data = response, Message = "Role permissions retrieved successfully.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<RolePermissionResponseModel> UpdateRolePermissions(RolePermissionUpdateRequestModel request)
    {
        try
        {
            if (request == null)
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Request is required." };
            if (request.RoleId <= 0)
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid Role ID." };

            var role = _db.TblRoles.FirstOrDefault(x => x.RoleId == request.RoleId && !x.DeleteFlag);
            if (role == null)
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 404, Message = "Role not found." };

            if (request.PermissionIds.Any(permissionId => !_db.TblPermissions.Any(p => p.PermissionId == permissionId && p.IsActive && !p.DeleteFlag)))
            {
                return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 400, Message = "One or more permission IDs are invalid or inactive." };
            }

            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var existingRolePermissions = _db.TblRolePermissions.Where(x => x.RoleId == request.RoleId && !x.DeleteFlag).ToList();

                // Soft delete ones not in new list
                foreach (var erp in existingRolePermissions)
                {
                    if (!request.PermissionIds.Contains(erp.PermissionId))
                    {
                        erp.DeleteFlag = true;
                    }
                }

                // Add missing ones
                foreach (var permissionId in request.PermissionIds)
                {
                    var erp = existingRolePermissions.FirstOrDefault(x => x.PermissionId == permissionId);
                    if (erp == null)
                    {
                        _db.TblRolePermissions.Add(new TblRolePermission
                        {
                            RoleId = request.RoleId,
                            PermissionId = permissionId,
                            CreatedDate = DateTime.Now,
                            DeleteFlag = false
                        });
                    }
                    else if (erp.DeleteFlag)
                    {
                        // Reactivate if previously soft deleted
                        erp.DeleteFlag = false;
                        erp.CreatedDate = DateTime.Now;
                    }
                }

                _db.SaveChanges();
                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = _currentUser.UserId,
                Action      = AuditActions.PermissionChanged,
                FeatureName = "RolePermission",
                EntityName  = "TblRolePermission",
                EntityId    = request.RoleId.ToString(),
                NewValue    = new { request.RoleId, PermissionIds = request.PermissionIds },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            return GetRolePermissions(request.RoleId);
        }
        catch (Exception)
        {
            return new Result<RolePermissionResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }
}
