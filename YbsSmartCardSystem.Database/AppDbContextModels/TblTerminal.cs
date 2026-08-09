using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblTerminal
{
    public int TerminalId { get; set; }

    public string TerminalSerialNo { get; set; } = null!;

    public int BusId { get; set; }

    public bool IsActive { get; set; }

    public bool DeleteFlag { get; set; }

    public int? CreatedBy { get; set; }

    public virtual TblStaffUser? CreatedUser { get; set; }

    public virtual TblBu Bus { get; set; } = null!;

    public virtual ICollection<TblTransaction> TblTransactions { get; set; } = new List<TblTransaction>();
}
