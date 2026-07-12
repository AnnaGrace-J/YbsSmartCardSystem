namespace YbsSmartCardSystem.Domain.Features.TopUp.Models;

public class TopUpCreateRequestModel
{
    public int     CardId { get; set; }
    public decimal Amount { get; set; }
    public string? Remark { get; set; }
}

public class TopUpCreateResponseModel
{
    public int      TopUpId    { get; set; }
    public Guid     TopUpNo    { get; set; }
    public int      CardId     { get; set; }
    public string   CardNum    { get; set; } = null!;
    public string   OwnerName  { get; set; } = null!;
    public decimal  Amount     { get; set; }
    public decimal  NewBalance { get; set; }
    public DateTime TopUpDate  { get; set; }
    public string?  Remark     { get; set; }
}

public class TopUpListRequestModel
{
    public int  CardId   { get; set; }  // 0 = all cards
    public int  PageNo   { get; set; } = 1;
    public int  PageSize { get; set; } = 10;
}

public class TopUpListResponseModel
{
    public List<TopUpModel> TopUps     { get; set; } = new();
    public int              TotalCount { get; set; }
}

public class TopUpModel
{
    public int      TopUpId   { get; set; }
    public Guid     TopUpNo   { get; set; }
    public int      CardId    { get; set; }
    public string   CardNum   { get; set; } = null!;
    public string   OwnerName { get; set; } = null!;
    public decimal  Amount    { get; set; }
    public DateTime TopUpDate { get; set; }
    public string?  Remark    { get; set; }
}
