using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using YbsSmartCardSystem.Contracts.Features.RolePermission;

namespace YbsSmartCardSystem.App.Components.Features.RolePermission;

public partial class PermissionList : ComponentBase
{
    private List<PermissionModel>? Permissions { get; set; }
    private PermissionListRequestModel Request { get; set; } = new() { PageNo = 1, PageSize = 10 };
    private int TotalCount { get; set; }
    private string? ErrorMessage { get; set; }

    private int MaxPage => (int)Math.Ceiling((double)TotalCount / Request.PageSize);

    protected override async Task OnInitializedAsync()
    {
        await LoadPermissions();
    }

    private async Task LoadPermissions()
    {
        ErrorMessage = null;
        var result = await Api.GetPermissions(Request);
        if (result.IsSuccess && result.Data != null)
        {
            Permissions = result.Data.Permissions;
            TotalCount = result.Data.TotalCount;
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to load permissions.";
        }
    }

    private async Task HandleSearch(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            Request.PageNo = 1;
            await LoadPermissions();
        }
    }

    private async Task PrevPage()
    {
        if (Request.PageNo > 1)
        {
            Request.PageNo--;
            await LoadPermissions();
        }
    }

    private async Task NextPage()
    {
        if (Request.PageNo < MaxPage)
        {
            Request.PageNo++;
            await LoadPermissions();
        }
    }
}
