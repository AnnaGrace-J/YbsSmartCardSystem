namespace YbsSmartCardSystem.Contracts.Features.BusPayment;

public class BusListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsDeleted { get; set; } = false;
}

public class BusListResponseModel
{
    public List<BusModel> Buses { get; set; } = new();
    public int TotalCount { get; set; }
}

public class BusCreateRequestModel
{
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
}

public class BusCreateResponseModel
{
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
}

public class BusPatchRequestModel
{
    public string? BusNo { get; set; }
    public string? BusLicense { get; set; }
}

public class BusModel
{
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public string? CreatedByName { get; set; }
    public string? CreatedByRole { get; set; }
    public bool DeleteFlag { get; set; }
}
