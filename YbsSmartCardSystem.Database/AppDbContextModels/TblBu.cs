using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblBu
{
    public int BusId { get; set; }

    public string BusNo { get; set; } = null!;

    public string BusLicense { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public bool DeleteFlag { get; set; }

    public int? CreatedBy { get; set; }

    public virtual TblStaffUser? CreatedUser { get; set; }

    public virtual ICollection<TblTerminal> TblTerminals { get; set; } = new List<TblTerminal>();
}
