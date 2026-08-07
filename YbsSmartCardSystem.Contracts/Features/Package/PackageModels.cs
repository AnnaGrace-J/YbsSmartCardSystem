namespace YbsSmartCardSystem.Contracts.Features.Package;

public class PackageListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class PackageListResponseModel
{
    public int TotalCount { get; set; }
    public List<PackageModel> Packages { get; set; } = [];
}

public class PackageModel
{
    public int PackageId { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class PackageCreateRequestModel
{
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PackageCreateResponseModel : PackageModel
{
}

public class PackagePatchRequestModel
{
    public string? PackageCode { get; set; }
    public string? PackageName { get; set; }
    public decimal? Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
