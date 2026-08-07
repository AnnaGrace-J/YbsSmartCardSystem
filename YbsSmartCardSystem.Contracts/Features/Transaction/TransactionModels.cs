namespace YbsSmartCardSystem.Contracts.Features.Transaction;

public class TransactionCreateRequestModel
{
    public string CardNum { get; set; } = string.Empty;
    public string TerminalSerialNo { get; set; } = string.Empty;
}

public class TransactionCreateResponseModel
{
    public int TransactionId { get; set; }
    public Guid TransactionNo { get; set; }
    public int CardId { get; set; }
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RemainingBalance { get; set; }
    public DateTime TransactionDate { get; set; }
}

public class TransactionListRequestModel
{
    public string? CardNum { get; set; }
    public string? TerminalSerialNo { get; set; }
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TransactionListResponseModel
{
    public List<TransactionModel> Transactions { get; set; } = new();
    public int TotalCount { get; set; }
}

public class TransactionModel
{
    public int TransactionId { get; set; }
    public Guid TransactionNo { get; set; }
    public int CardId { get; set; }
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime TransactionDate { get; set; }
}
