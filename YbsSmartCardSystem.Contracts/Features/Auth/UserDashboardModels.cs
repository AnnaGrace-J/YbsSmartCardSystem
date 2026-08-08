namespace YbsSmartCardSystem.Contracts.Features.Auth;

public class UserDashboardResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<UserCardSummaryModel> Cards { get; set; } = [];
}

public class UserCardSummaryModel
{
    public int CardId { get; set; }
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
