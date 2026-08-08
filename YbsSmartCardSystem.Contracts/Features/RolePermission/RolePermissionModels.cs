namespace YbsSmartCardSystem.Contracts.Features.RolePermission;

public class RoleListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class RoleListResponseModel
{
    public int TotalCount { get; set; }
    public List<RoleModel> Roles { get; set; } = [];
}

public class RoleModel
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
}

public class RoleCreateRequestModel
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RolePatchRequestModel
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? FeatureName { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionListResponseModel
{
    public int TotalCount { get; set; }
    public List<PermissionModel> Permissions { get; set; } = [];
}

public class PermissionModel
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UserRoleUpdateRequestModel
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = [];
}

public class RolePermissionUpdateRequestModel
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = [];
}

public class UserRoleResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<RoleModel> Roles { get; set; } = [];
}

public class RolePermissionResponseModel
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public List<PermissionModel> Permissions { get; set; } = [];
}
