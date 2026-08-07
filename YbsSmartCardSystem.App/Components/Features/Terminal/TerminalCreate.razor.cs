using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Contracts.Features.BusPayment;

namespace YbsSmartCardSystem.App.Components.Features.Terminal;

public partial class TerminalCreate
{
    [Inject] private ApiService ApiService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private TerminalCreateRequestModel request = new();
    private Result<BusListResponseModel> busOptions = new();
    private string? message;
    private bool isSuccess;
    private bool isSaving;

    protected override async Task OnInitializedAsync()
    {
        busOptions = await ApiService.GetBuses(new BusListRequestModel { PageNo = 1, PageSize = 1000 });
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(request.TerminalSerialNo))
        {
            message   = "Terminal serial number is required.";
            isSuccess = false;
            return false;
        }

        if (request.TerminalSerialNo.Trim().Length > 100)
        {
            message   = "Terminal serial number cannot exceed 100 characters.";
            isSuccess = false;
            return false;
        }

        if (request.BusId <= 0)
        {
            message   = "Bus is required.";
            isSuccess = false;
            return false;
        }

        return true;
    }

    private async Task Save()
    {
        if (!Validate()) return;

        isSaving = true;

        var result = await ApiService.TerminalCreate(request);
        message   = result.Message;
        isSuccess = result.IsSuccess;

        if (result.IsSuccess)
        {
            NavigationManager.NavigateTo("/terminals");
        }

        isSaving = false;
    }
}
