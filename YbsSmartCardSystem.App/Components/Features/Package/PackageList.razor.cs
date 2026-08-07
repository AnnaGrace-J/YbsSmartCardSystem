using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.Package;

namespace YbsSmartCardSystem.App.Components.Features.Package;

public partial class PackageList : ComponentBase
{
    private List<PackageModel> Packages { get; set; } = new();
    private PackageListRequestModel Request { get; set; } = new();
    private int TotalCount { get; set; }
    private bool IsLoading { get; set; }
    private string? ErrorMessage { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await LoadPackages();
    }

    private async Task LoadPackages()
    {
        IsLoading = true;
        ErrorMessage = null;
        
        var result = await Api.GetPackages(Request);
        if (result.IsSuccess && result.Data != null)
        {
            Packages = result.Data.Packages;
            TotalCount = result.Data.TotalCount;
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to load packages.";
        }
        
        IsLoading = false;
    }

    private async Task SearchPackages()
    {
        Request.PageNo = 1;
        await LoadPackages();
    }

    private async Task ChangePage(int newPage)
    {
        Request.PageNo = newPage;
        await LoadPackages();
    }

    private async Task DeletePackage(int id)
    {
        var result = await Api.PackageDelete(id);
        if (result.IsSuccess)
        {
            await LoadPackages();
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to delete package.";
        }
    }
}
