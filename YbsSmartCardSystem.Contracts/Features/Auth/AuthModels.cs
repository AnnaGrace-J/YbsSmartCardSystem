namespace YbsSmartCardSystem.Contracts.Features.Auth;

public class LoginRequestModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string UserType { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class CurrentUserModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class CurrentUserPermissionsResponseModel
{
    public List<string> Permissions { get; set; } = [];
}
