using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.Transaction;

namespace YbsSmartCardSystem.App.Components.Features.Transaction;

public partial class TransactionCreate
{
    [Inject] private ApiService ApiService { get; set; } = default!;

    private TransactionCreateRequestModel request = new();
    private TransactionCreateResponseModel? transactionResult;
    private string? message;
    private bool isSuccess;
    private bool isSaving;
    private bool isCardBound;

    protected override async Task OnInitializedAsync()
    {
        var result = await ApiService.GetUserDashboard();
        if (result.IsSuccess && result.Data?.Cards != null && result.Data.Cards.Any())
        {
            request.CardNum = result.Data.Cards.First().CardNum;
            isCardBound = true;
        }
    }

    private bool Validate()
    {
        transactionResult = null;

        if (string.IsNullOrWhiteSpace(request.CardNum))
        {
            message = "Card number is required.";
            isSuccess = false;
            return false;
        }

        if (request.CardNum.Trim().Length > 50)
        {
            message = "Card number cannot exceed 50 characters.";
            isSuccess = false;
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.TerminalSerialNo))
        {
            message = "Terminal serial number is required.";
            isSuccess = false;
            return false;
        }

        if (request.TerminalSerialNo.Trim().Length > 100)
        {
            message = "Terminal serial number cannot exceed 100 characters.";
            isSuccess = false;
            return false;
        }

        return true;
    }

    private async Task Save()
    {
        if (!Validate())
        {
            return;
        }

        isSaving = true;

        var result = await ApiService.TransactionCreate(request);
        message = result.Message;
        isSuccess = result.IsSuccess;
        transactionResult = result.IsSuccess ? result.Data : null;

        isSaving = false;
    }
}
