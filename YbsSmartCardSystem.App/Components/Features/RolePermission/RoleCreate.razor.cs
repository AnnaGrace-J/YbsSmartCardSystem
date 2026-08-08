using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.RolePermission;

namespace YbsSmartCardSystem.App.Components.Features.RolePermission;

public partial class RoleCreate : ComponentBase
{
    [Parameter]
    public int? RoleId { get; set; }

    private RoleModel Model { get; set; } = new() { IsActive = true };
    private bool IsSubmitting { get; set; }
    private string? ErrorMessage { get; set; }

    private bool IsEditMode => RoleId.HasValue;

    protected override async Task OnInitializedAsync()
    {
        if (IsEditMode)
        {
            var result = await Api.GetRoleById(RoleId!.Value);
            if (result.IsSuccess && result.Data != null)
            {
                Model = result.Data;
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to load role details.";
            }
        }
    }

    private async Task HandleSubmit()
    {
        IsSubmitting = true;
        ErrorMessage = null;

        if (IsEditMode)
        {
            var patchRequest = new RolePatchRequestModel
            {
                RoleCode = Model.RoleCode,
                RoleName = Model.RoleName,
                Description = Model.Description,
                IsActive = Model.IsActive
            };

            var result = await Api.RolePatch(RoleId!.Value, patchRequest);
            if (result.IsSuccess)
            {
                NavManager.NavigateTo("/roles");
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to update role.";
            }
        }
        else
        {
            var createRequest = new RoleCreateRequestModel
            {
                RoleCode = Model.RoleCode,
                RoleName = Model.RoleName,
                Description = Model.Description,
                IsActive = Model.IsActive
            };

            var result = await Api.RoleCreate(createRequest);
            if (result.IsSuccess)
            {
                NavManager.NavigateTo("/roles");
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to create role.";
            }
        }

        IsSubmitting = false;
    }
}
