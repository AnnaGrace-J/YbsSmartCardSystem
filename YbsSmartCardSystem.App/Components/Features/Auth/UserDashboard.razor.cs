using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.Contracts.Features.Auth;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.App.Services;

namespace YbsSmartCardSystem.App.Components.Features.Auth
{
    public partial class UserDashboard : ComponentBase
    {
        [Inject]
        private ApiService ApiService { get; set; } = null!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        private UserDashboardResponseModel? DashboardData { get; set; }
        private bool IsLoading { get; set; } = true;
        private string? ErrorMessage { get; set; }

        private string? message;
        private bool isSuccess = true;

        // Modals / Dialog states
        private bool showRenameModal = false;
        private string newOwnerName = "";
        private bool isRenaming = false;

        private bool showDeactivateModal = false;

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboard();
        }

        private async Task LoadDashboard()
        {
            IsLoading = true;
            ErrorMessage = null;

            var result = await Api.GetUserDashboard();
            if (result.IsSuccess && result.Data != null)
            {
                DashboardData = result.Data;
            }
            else
            {
                ErrorMessage = result.Message ?? "Failed to load dashboard.";
            }

            IsLoading = false;
        }

        private void OpenRenameModal()
        {
            var card = DashboardData?.Cards.FirstOrDefault();
            if (card == null) return;
            newOwnerName = card.OwnerName;
            showRenameModal = true;
        }

        private async Task RenameCard()
        {
            var card = DashboardData?.Cards.FirstOrDefault();
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

        private void ReportLost()
        {
            var card = DashboardData?.Cards.FirstOrDefault();
            if (card == null) return;
            showDeactivateModal = true;
        }

        private async Task ConfirmReportLost()
        {
            var card = DashboardData?.Cards.FirstOrDefault();
            if (card == null) return;

            showDeactivateModal = false;
            IsLoading = true;
            message = null;
            try
            {
                var result = await ApiService.CardDelete(card.CardId);
                if (result.IsSuccess)
                {
                    DashboardData?.Cards.Clear();
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
                IsLoading = false;
            }
        }

        private string FormatCardNum(string? cardNum)
        {
            if (string.IsNullOrEmpty(cardNum)) return "**** **** **** ----";
            if (cardNum.Length <= 4) return $"**** **** **** {cardNum}";
            return $"**** **** **** {cardNum.Substring(cardNum.Length - 4)}";
        }

        private string FormatBalance(decimal bal)
        {
            return $"{bal:N0} MMK";
        }
    }
}
