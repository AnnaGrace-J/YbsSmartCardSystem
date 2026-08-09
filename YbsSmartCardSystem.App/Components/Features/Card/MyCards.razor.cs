using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Domain.Common;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.TopUp;

namespace YbsSmartCardSystem.App.Components.Features.Card
{
    public partial class MyCards
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private NavigationManager NavigationManager { get; set; } = null!;

        private CardModel? card;
        private bool isLoading = true;
        private string? message;
        private bool isSuccess = true;

        // Modals / Dialog states
        private bool showRenameModal = false;
        private string newOwnerName = "";
        private bool isRenaming = false;

        private bool showTopUpModal = false;
        private decimal topUpAmount = 10000;
        private bool isToppingUp = false;

        private bool showAutoReloadModal = false;
        private bool autoReloadEnabled = true;
        private decimal autoReloadTriggerAmount = 10000;
        private decimal autoReloadTopUpAmount = 20000;

        protected override async Task OnInitializedAsync()
        {
            await LoadMyCard();
        }

        private async Task LoadMyCard()
        {
            isLoading = true;
            message = null;
            try
            {
                var result = await ApiService.GetMyCard();
                if (result.IsSuccess)
                {
                    card = result.Data;
                    if (card != null)
                    {
                        newOwnerName = card.OwnerName;
                    }
                }
                else
                {
                    message = result.Message;
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                message = $"An error occurred: {ex.Message}";
                isSuccess = false;
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OpenRenameModal()
        {
            if (card == null) return;
            newOwnerName = card.OwnerName;
            showRenameModal = true;
        }

        private async Task RenameCard()
        {
            if (card == null || string.IsNullOrWhiteSpace(newOwnerName)) return;

            isRenaming = true;
            message = null;
            try
            {
                var request = new CardPatchRequestModel
                {
                    OwnerName = newOwnerName
                };
                var result = await ApiService.CardPatch(card.CardId, request);
                if (result.IsSuccess)
                {
                    card.OwnerName = newOwnerName;
                    showRenameModal = false;
                }
                else
                {
                    message = result.Message;
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                message = $"Failed to rename card: {ex.Message}";
                isSuccess = false;
            }
            finally
            {
                isRenaming = false;
            }
        }

        private async Task ReportLost()
        {
            if (card == null) return;

            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "WARNING: Reporting this card as lost will permanently deactivate it. Are you sure you want to proceed?");
            if (!confirmed) return;

            isLoading = true;
            message = null;
            try
            {
                var result = await ApiService.CardDelete(card.CardId);
                if (result.IsSuccess)
                {
                    card = null;
                    message = "Card has been reported lost and deactivated.";
                    isSuccess = true;
                }
                else
                {
                    message = result.Message;
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                message = $"Failed to report card lost: {ex.Message}";
                isSuccess = false;
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task SubmitTopUp()
        {
            if (card == null || topUpAmount <= 0) return;

            isToppingUp = true;
            message = null;
            try
            {
                var request = new TopUpCreateRequestModel
                {
                    CardId = card.CardId,
                    Amount = topUpAmount
                };
                var result = await ApiService.TopUpCreate(request);
                if (result.IsSuccess)
                {
                    card.Balance += topUpAmount;
                    showTopUpModal = false;
                    message = $"Successfully topped up {topUpAmount:N0} MMK!";
                    isSuccess = true;
                }
                else
                {
                    message = result.Message;
                    isSuccess = false;
                }
            }
            catch (Exception ex)
            {
                message = $"Failed to top up: {ex.Message}";
                isSuccess = false;
            }
            finally
            {
                isToppingUp = false;
            }
        }

        private void SaveAutoReloadSettings()
        {
            showAutoReloadModal = false;
            message = autoReloadEnabled 
                ? $"Auto-reload set to top up {autoReloadTopUpAmount:N0} MMK when balance drops below {autoReloadTriggerAmount:N0} MMK."
                : "Auto-reload disabled.";
            isSuccess = true;
        }

        private string FormatCardNum(string? cardNum)
        {
            if (string.IsNullOrEmpty(cardNum)) return "**** **** **** ----";
            if (cardNum.Length <= 4) return $"**** **** **** {cardNum}";
            // mask digits except the last 4 digits
            return $"**** **** **** {cardNum.Substring(cardNum.Length - 4)}";
        }

        private string FormatBalance(decimal bal)
        {
            if (bal >= 500)
            {
                return $"${(bal / 1000m):N2}";
            }
            return $"${bal:N2}";
        }
    }
}
