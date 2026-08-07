using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblUser
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? Email { get; set; }

    public string PasswordHash { get; set; } = null!;

    public string? PasswordSalt { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual ICollection<TblAuditLog> TblAuditLogs { get; set; } = new List<TblAuditLog>();

    public virtual ICollection<TblUserRole> TblUserRoles { get; set; } = new List<TblUserRole>();
}
