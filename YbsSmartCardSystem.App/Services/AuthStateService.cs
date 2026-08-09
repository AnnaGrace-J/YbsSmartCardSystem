using YbsSmartCardSystem.Contracts.Features.Auth;

namespace YbsSmartCardSystem.App.Services;

public class AuthStateService
{
    public string? Token { get; private set; }
    public LoginResponseModel? CurrentUser { get; private set; }
    public List<string> Permissions { get; private set; } = [];
    public bool IsAuthenticated => !string.IsNullOrEmpty(Token);

    public int? UserId => CurrentUser?.UserId;
    public string? UserName => CurrentUser?.UserName;
    public List<string> Roles => CurrentUser?.Roles ?? [];

    public event Action? OnChange;

    public void SetLogin(LoginResponseModel loginResponse)
    {
        Token = loginResponse.Token;
        CurrentUser = loginResponse;
        OnChange?.Invoke();
    }

    public void SetPermissions(List<string> permissions)
    {
        Permissions = permissions ?? [];
        OnChange?.Invoke();
    }

    public bool HasPermission(string permissionCode)
    {
        return Permissions.Contains(permissionCode);
    }

    public bool IsInRole(string roleCode)
    {
        return Roles.Contains(roleCode);
    }

    public void Logout()
    {
        Token = null;
        CurrentUser = null;
        Permissions.Clear();
        OnChange?.Invoke();
    }
}
