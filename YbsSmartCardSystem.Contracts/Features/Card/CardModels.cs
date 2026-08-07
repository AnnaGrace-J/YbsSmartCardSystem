namespace YbsSmartCardSystem.Contracts.Features.Card;

public class CardListRequestModel
{
    public int     PageNo   { get; set; } = 1;
    public int     PageSize { get; set; } = 10;
    public string? Search   { get; set; }
}

public class CardListResponseModel
{
    public List<CardModel> Cards      { get; set; } = new();
    public int             TotalCount { get; set; }
}
public class CardCreateRequestModel
{
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? MobileNo { get; set; } = string.Empty;
   // public decimal Balance { get; set; }
}
public class CardCreateResponseModel()
{
    public int CardId { get; set; }
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string? MobileNo { get; set; } = string.Empty;
    // public decimal Balance { get; set; }
}

//public class CardUpdateRequestModel
//{
//    public string CardNum { get; set; } = null!;

//    public string OwnerName { get; set; } = null!;

//    public string? MobileNo { get; set; }

//    public decimal Balance { get; set; }
//}
public class CardPatchRequestModel
{
    public string? CardNum   { get; set; }
    public string? OwnerName { get; set; }
    public string? MobileNo  { get; set; }
    // Balance is intentionally excluded — balance is only changed via TopUp or Transaction.
}
public class CardPatchResponseModel
{
    public string? CardNum   { get; set; }
    public string? OwnerName { get; set; }
    public string? MobileNo  { get; set; }
}
public class CardModel
{
    public int CardId { get; set; }

    public string CardNum { get; set; } = null!;

    public string OwnerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public decimal Balance { get; set; }
}
