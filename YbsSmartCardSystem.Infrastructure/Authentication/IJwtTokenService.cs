namespace YbsSmartCardSystem.Infrastructure.Authentication;

public class JwtTokenUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class JwtTokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(JwtTokenUser user);
}
