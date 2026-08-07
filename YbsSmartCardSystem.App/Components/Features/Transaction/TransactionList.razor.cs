using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.Transaction;

namespace YbsSmartCardSystem.App.Components.Features.Transaction;

public partial class TransactionList
{
    [Inject] private ApiService ApiService { get; set; } = default!;

    private TransactionListRequestModel request = new() { PageNo = 1, PageSize = 10 };
    private Result<TransactionListResponseModel> response = new();
    private bool isLoading;

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
}
