using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.RolePermission;

namespace YbsSmartCardSystem.App.Components.Features.RolePermission;

public partial class RolePermissionManage : ComponentBase
{
    [Parameter]
    public int RoleId { get; set; }

    private string RoleCode { get; set; } = string.Empty;
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; }
    private string? ErrorMessage { get; set; }

    private Dictionary<string, List<PermissionModel>> GroupedPermissions { get; set; } = [];
    private HashSet<int> SelectedPermissionIds { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
    }

    private async Task LoadData()
    {
        IsLoading = true;
        ErrorMessage = null;

        var allPermissionsResult = await Api.GetPermissions(new PermissionListRequestModel { PageNo = 1, PageSize = 100 });
        var rolePermissionsResult = await Api.GetRolePermissions(RoleId);

        if (allPermissionsResult.IsSuccess && rolePermissionsResult.IsSuccess && allPermissionsResult.Data != null && rolePermissionsResult.Data != null)
        {
            RoleCode = rolePermissionsResult.Data.RoleCode;
            SelectedPermissionIds = new HashSet<int>(rolePermissionsResult.Data.Permissions.Select(x => x.PermissionId));
            
            GroupedPermissions = allPermissionsResult.Data.Permissions
                .GroupBy(x => x.FeatureName)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
        else
        {
            ErrorMessage = allPermissionsResult.Message ?? rolePermissionsResult.Message ?? "Failed to load permissions.";
        }

        IsLoading = false;
    }

    private void TogglePermission(int permissionId, object? checkedValue)
    {
        bool isChecked = checkedValue is bool b && b;
        if (isChecked)
        {
            SelectedPermissionIds.Add(permissionId);
        }
        else
        {
            SelectedPermissionIds.Remove(permissionId);
        }
    }

    private async Task HandleSave()
    {
        IsSaving = true;
        ErrorMessage = null;

        var request = new RolePermissionUpdateRequestModel
        {
            RoleId = RoleId,
            PermissionIds = SelectedPermissionIds.ToList()
        };

        var result = await Api.UpdateRolePermissions(request);
        if (result.IsSuccess)
        {
            NavManager.NavigateTo("/roles");
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to update permissions.";
        }

        IsSaving = false;
    }
}
