using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.Package;

namespace YbsSmartCardSystem.App.Components.Features.Package;

public partial class PackageCreate : ComponentBase
{
    private PackageCreateRequestModel Model { get; set; } = new();
    private bool IsSubmitting { get; set; }
    private string? ErrorMessage { get; set; }

    private async Task HandleSubmit()
    {
        IsSubmitting = true;
        ErrorMessage = null;

        var result = await Api.PackageCreate(Model);
        if (result.IsSuccess)
        {
            NavManager.NavigateTo("/packages");
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to create package.";
        }

        IsSubmitting = false;
    }
}
