using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.BusPayment;

namespace YbsSmartCardSystem.App.Components.Features.Terminal;

public partial class TerminalList
{
    [Inject] private ApiService ApiService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private TerminalListRequestModel request = new();
    private Result<TerminalListResponseModel> response = new();
    private TerminalPatchRequestModel editTerminal = new();
    private Result<BusListResponseModel> busOptions = new();
    private int? editingTerminalId;
    private bool editTerminalIsActive;
    private string? message;
    private bool isSuccess;
    private bool isSaving;
    private bool isLoading = true;

    private int TotalPages =>
        response.Data is null || request.PageSize == 0
            ? 1
            : (int)Math.Ceiling((double)response.Data.TotalCount / request.PageSize);

    private string searchText = string.Empty;

    private string StatusFilter
    {
        get => request.IsDeleted switch
        {
            false => "active",
            true => "inactive",
            _ => "all"
        };
        set
        {
            request.IsDeleted = value switch
            {
                "active" => false,
                "inactive" => true,
                _ => null
            };
        }
    }

    private async Task Search()
    {
        request.PageNo = 1;
        await LoadTerminals();
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadTerminals();
    }

    private async Task LoadTerminals()
    {
        isLoading = true;
        request.Search = searchText;

        try
        {
            response = await ApiService.GetTerminals(request);
        }
        catch (Exception ex)
        {
            response = new Result<TerminalListResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = $"Failed to load terminals: {ex.Message}"
            };
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadBusOptions()
    {
        var result = await ApiService.GetBuses(new BusListRequestModel { PageNo = 1, PageSize = 1000 });
        if (result.IsSuccess)
        {
            busOptions = result;
        }
        else
        {
            message = result.Message ?? "Failed to load bus options.";
            isSuccess = false;
        }
    }

    private async Task EditTerminal(TerminalModel terminal)
    {
        if (busOptions.Data is null)
        {
            await LoadBusOptions();
        }

        editingTerminalId    = terminal.TerminalId;
        editTerminalIsActive = terminal.IsActive;
        editTerminal = new TerminalPatchRequestModel
        {
            TerminalSerialNo = terminal.TerminalSerialNo,
            BusId            = terminal.BusId,
            IsActive         = terminal.IsActive
        };
        message = null;
    }

    private void CancelEdit()
    {
        editingTerminalId = null;
        editTerminal      = new();
        message           = null;
    }

    private bool ValidateEdit()
    {
        if (string.IsNullOrWhiteSpace(editTerminal.TerminalSerialNo))
        {
            message   = "Terminal serial number is required.";
            isSuccess = false;
            return false;
        }

        if (editTerminal.TerminalSerialNo.Trim().Length > 100)
        {
            message   = "Terminal serial number cannot exceed 100 characters.";
            isSuccess = false;
            return false;
        }

        if (editTerminal.BusId is null || editTerminal.BusId <= 0)
        {
            message   = "Bus is required.";
            isSuccess = false;
            return false;
        }

        return true;
    }

    private async Task UpdateTerminal()
    {
        if (editingTerminalId is null) return;

        if (!ValidateEdit()) return;

        editTerminal.IsActive = editTerminalIsActive;

        isSaving = true;

        var result = await ApiService.TerminalPatch(editingTerminalId.Value, editTerminal);
        message   = result.Message;
        isSuccess = result.IsSuccess;

        if (result.IsSuccess)
        {
            CancelEdit();
            await LoadTerminals();
        }

        isSaving = false;
    }

    private async Task DeleteTerminal(int id)
    {
        var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this terminal?");
        if (!confirmed) return;

        var result = await ApiService.TerminalDelete(id);
        message   = result.Message;
        isSuccess = result.IsSuccess;

        if (result.IsSuccess)
        {
            await LoadTerminals();
        }
    }

    private async Task PrevPage()
    {
        if (request.PageNo > 1)
        {
            request.PageNo--;
            await LoadTerminals();
        }
    }

    private async Task NextPage()
    {
        if (request.PageNo < TotalPages)
        {
            request.PageNo++;
            await LoadTerminals();
        }
    }
}
