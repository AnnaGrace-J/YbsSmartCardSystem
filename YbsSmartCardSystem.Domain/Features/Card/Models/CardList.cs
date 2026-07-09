namespace YbsSmartCardSystem.Domain.Features.Card.Models;

public class CardListRequestModel
{
    public int PageNo { get; set; }
    public int PageSize { get; set; }
}

public class CardListResponseModel
{
    public List<CardModel> Cards { get; set; } = new List<CardModel>();
}
public class CardCreateRequestModel
{
    public string CardNum { get; set; } = null!;
    public string OwnerName { get; set; } = null!;
    public string? MobileNo { get; set; }
    public decimal Balance { get; set; }
}

public class CardUpdateRequestModel
{
    public string CardNum { get; set; } = null!;

    public string OwnerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public decimal Balance { get; set; }
}
public class CardPatchRequestModel
{
    public string? CardNum { get; set; }

    public string? OwnerName { get; set; }

    public string? MobileNo { get; set; }

    public decimal? Balance { get; set; }
}
public class CardModel
{
    public int CardId { get; set; }

    public string CardNum { get; set; } = null!;

    public string OwnerName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public decimal Balance { get; set; }
}
