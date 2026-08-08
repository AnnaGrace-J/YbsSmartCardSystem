using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.Auth;

namespace YbsSmartCardSystem.App.Components.Features.Auth;

public partial class UserDashboard : ComponentBase
{
    private UserDashboardResponseModel? DashboardData { get; set; }
    private bool IsLoading { get; set; } = true;
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboard();
    }

    private async Task LoadDashboard()
    {
        IsLoading = true;
        ErrorMessage = null;

        var result = await Api.GetUserDashboard();
        if (result.IsSuccess && result.Data != null)
        {
            DashboardData = result.Data;
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to load dashboard.";
        }

        IsLoading = false;
    }
}
