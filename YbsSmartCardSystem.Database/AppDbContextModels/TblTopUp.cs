using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblTopUp
{
    public int TopUpId { get; set; }

    public Guid TopUpNo { get; set; }

    public int CardId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TopUpDate { get; set; }

    public string? Remark { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblCard Card { get; set; } = null!;
}
