using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblTransaction
{
    public int TransactionId { get; set; }

    public Guid TransactionNo { get; set; }

    public int CardId { get; set; }

    public int TerminalId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public bool DeleteFlag { get; set; }

    public virtual TblCard Card { get; set; } = null!;

    public virtual TblTerminal Terminal { get; set; } = null!;
}
