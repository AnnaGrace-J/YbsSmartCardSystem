using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.Auth;

namespace YbsSmartCardSystem.App.Components.Features.Auth;

public partial class Login : ComponentBase
{
    private LoginRequestModel Model { get; set; } = new();
    private bool IsSubmitting { get; set; }
    private string? ErrorMessage { get; set; }
    private bool ShowPassword { get; set; }

    private async Task HandleSubmit()
    {
        IsSubmitting = true;
        ErrorMessage = null;

        var result = await Api.Login(Model);
        if (result.IsSuccess && result.Data != null)
        {
            AuthState.SetLogin(result.Data);

            // Fetch permissions after successful login
            var permsResult = await Api.GetPermissions();
            if (permsResult.IsSuccess && permsResult.Data != null)
            {
                AuthState.SetPermissions(permsResult.Data.Permissions);
            }

            NavManager.NavigateTo("/dashboard");
        }
        else
        {
            ErrorMessage = result.Message ?? "Login failed.";
        }

        IsSubmitting = false;
    }
}
