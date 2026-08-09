namespace YbsSmartCardSystem.Contracts.Features.BusPayment;

public class TerminalListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsDeleted { get; set; } = false;
}

public class TerminalListResponseModel
{
    public List<TerminalModel> Terminals { get; set; } = new();
    public int TotalCount { get; set; }
}

public class TerminalCreateRequestModel
{
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TerminalCreateResponseModel
{
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class TerminalPatchRequestModel
{
    public string? TerminalSerialNo { get; set; }
    public int? BusId { get; set; }
    public bool? IsActive { get; set; }
}

public class TerminalModel
{
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? CreatedByName { get; set; }
    public string? CreatedByRole { get; set; }
    public bool DeleteFlag { get; set; }
}
