using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblPackage
{
    public int PackageId { get; set; }

    public string PackageCode { get; set; } = null!;

    public string PackageName { get; set; } = null!;

    public decimal Price { get; set; }

    public int? RideLimit { get; set; }

    public int? ValidDays { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool DeleteFlag { get; set; }
}
