using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblUserRole
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblRole Role { get; set; } = null!;

    public virtual TblUser User { get; set; } = null!;
}
