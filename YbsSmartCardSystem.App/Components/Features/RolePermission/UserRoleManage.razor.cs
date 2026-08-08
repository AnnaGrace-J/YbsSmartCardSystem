using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.RolePermission;

namespace YbsSmartCardSystem.App.Components.Features.RolePermission;

public partial class UserRoleManage : ComponentBase
{
    [Parameter]
    public int UserId { get; set; }

    private string UserName { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; }
    private string? ErrorMessage { get; set; }

    private List<RoleModel> AvailableRoles { get; set; } = [];
    private HashSet<int> SelectedRoleIds { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        IsLoading = true;
        ErrorMessage = null;

        var allRolesResult = await Api.GetRoles(new RoleListRequestModel { PageNo = 1, PageSize = 100, IsActive = true });
        var userRolesResult = await Api.GetUserRoles(UserId);

        if (allRolesResult.IsSuccess && userRolesResult.IsSuccess && allRolesResult.Data != null && userRolesResult.Data != null)
        {
            UserName = userRolesResult.Data.UserName;
            SelectedRoleIds = new HashSet<int>(userRolesResult.Data.Roles.Select(x => x.RoleId));
            AvailableRoles = allRolesResult.Data.Roles;
        }
        else
        {
            ErrorMessage = allRolesResult.Message ?? userRolesResult.Message ?? "Failed to load user roles data.";
        }

        IsLoading = false;
    }

    private void ToggleRole(int roleId, object? checkedValue)
    {
        bool isChecked = checkedValue is bool b && b;
        if (isChecked)
        {
            SelectedRoleIds.Add(roleId);
        }
        else
        {
            SelectedRoleIds.Remove(roleId);
        }
    }

    private async Task HandleSave()
    {
        IsSaving = true;
        ErrorMessage = null;

        var request = new UserRoleUpdateRequestModel
        {
            UserId = UserId,
            RoleIds = SelectedRoleIds.ToList()
        };

        var result = await Api.UpdateUserRoles(request);
        if (result.IsSuccess)
        {
            // Redirect back, or show success message.
            NavManager.NavigateTo("/cards");
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to update user roles.";
        }

        IsSaving = false;
    }
}
