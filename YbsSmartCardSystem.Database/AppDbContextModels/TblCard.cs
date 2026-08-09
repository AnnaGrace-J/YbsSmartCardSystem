using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblCard
{
    public int CardId { get; set; }

    public string CardNum { get; set; } = null!;

    public string OwnerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public decimal Balance { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public int? CreatedBy { get; set; }

    public virtual TblStaffUser? CreatedUser { get; set; }

    public virtual ICollection<TblTopUp> TblTopUps { get; set; } = new List<TblTopUp>();

    public virtual ICollection<TblTransaction> TblTransactions { get; set; } = new List<TblTransaction>();
}
