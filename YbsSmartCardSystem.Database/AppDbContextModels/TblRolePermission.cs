using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblRolePermission
{
    public int RolePermissionId { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblPermission Permission { get; set; } = null!;

    public virtual TblRole Role { get; set; } = null!;
}
