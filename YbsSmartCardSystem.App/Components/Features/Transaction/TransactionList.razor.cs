using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.Transaction;

namespace YbsSmartCardSystem.App.Components.Features.Transaction;

public partial class TransactionList
{
    [Inject] private ApiService ApiService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private TransactionListRequestModel request = new() { PageNo = 1, PageSize = 10 };
    private Result<TransactionListResponseModel> response = new();
    private bool isLoading;
    private string? copiedItem;

    protected override async Task OnInitializedAsync()
    {
        await LoadTransactions();
    }

    private async Task LoadTransactions()
    {
        isLoading = true;
        response = await ApiService.GetTransactions(request);
        isLoading = false;
    }

    private async Task ApplyFilter()
    {
        request.PageNo = 1;
        await LoadTransactions();
    }

    private async Task ClearFilter()
    {
        request.CardNum = null;
        request.TerminalSerialNo = null;
        request.PageNo = 1;
        await LoadTransactions();
    }

    private async Task ChangePage(int pageNo)
    {
        if (pageNo < 1 || pageNo > TotalPages)
        {
            return;
        }

        request.PageNo = pageNo;
        await LoadTransactions();
    }

    private int TotalPages
    {
        get
        {
            if (response.Data is null || response.Data.TotalCount == 0)
            {
                return 1;
            }

            return (int)Math.Ceiling((double)response.Data.TotalCount / request.PageSize);
        }
    }

    private async Task CopyText(string key, string text)
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("copyToClipboard", text);
        }
        catch
        {
            return;
        }

        copiedItem = key;
        StateHasChanged();
        await Task.Delay(1200);
        if (copiedItem == key)
        {
            copiedItem = null;
            StateHasChanged();
        }
    }
}
