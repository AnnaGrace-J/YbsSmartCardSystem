using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblPermission
{
    public int PermissionId { get; set; }

    public string PermissionCode { get; set; } = null!;

    public string PermissionName { get; set; } = null!;

    public string FeatureName { get; set; } = null!;

    public string ActionName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual ICollection<TblRolePermission> TblRolePermissions { get; set; } = new List<TblRolePermission>();
}
