using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using YbsSmartCardSystem.Contracts.Features.RolePermission;

namespace YbsSmartCardSystem.App.Components.Features.RolePermission;

public partial class RoleList : ComponentBase
{
    private List<RoleModel> Roles { get; set; } = [];
    private RoleListRequestModel Request { get; set; } = new() { PageNo = 1, PageSize = 10 };
    private int TotalCount { get; set; }
    private string? ErrorMessage { get; set; }
    private string? IsActiveFilter { get; set; } = "";
    private bool IsLoading { get; set; } = true;

    private bool ShowDeleteConfirm { get; set; }
    private RoleModel? RoleToDelete { get; set; }

    private int MaxPage => (int)Math.Ceiling((double)TotalCount / Request.PageSize);

    protected override async Task OnInitializedAsync()
    {
        await LoadRoles();
    }

    private async Task LoadRoles()
    {
        IsLoading = true;
        ErrorMessage = null;
        if (bool.TryParse(IsActiveFilter, out var isActiveVal))
        {
            Request.IsActive = isActiveVal;
        }
        else
        {
            Request.IsActive = null;
        }

        try
        {
            var result = await Api.GetRoles(Request);
            if (result.IsSuccess && result.Data != null)
            {
                Roles = result.Data.Roles;
                TotalCount = result.Data.TotalCount;
            }
            else
            {
                Roles = [];
                TotalCount = 0;
                ErrorMessage = result.Message ?? "Failed to load roles.";
            }
        }
        catch (Exception ex)
        {
            Roles = [];
            TotalCount = 0;
            ErrorMessage = $"Failed to load roles: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task HandleSearch(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            Request.PageNo = 1;
            await LoadRoles();
        }
    }

    private async Task PrevPage()
    {
        if (Request.PageNo > 1)
        {
            Request.PageNo--;
            await LoadRoles();
        }
    }

    private async Task NextPage()
    {
        if (Request.PageNo < MaxPage)
        {
            Request.PageNo++;
            await LoadRoles();
        }
    }

    private void ConfirmDelete(RoleModel role)
    {
        RoleToDelete = role;
        ShowDeleteConfirm = true;
    }

    private void CancelDelete()
    {
        RoleToDelete = null;
        ShowDeleteConfirm = false;
    }

    private async Task DeleteRole()
    {
        if (RoleToDelete == null) return;

        var result = await Api.RoleDelete(RoleToDelete.RoleId);
        if (result.IsSuccess)
        {
            ShowDeleteConfirm = false;
            RoleToDelete = null;
            await LoadRoles();
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to delete role.";
            ShowDeleteConfirm = false;
            RoleToDelete = null;
        }
    }
}
