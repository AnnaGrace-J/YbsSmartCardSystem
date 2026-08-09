using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;

namespace YbsSmartCardSystem.App.Components.Features.TopUp
{
    public partial class TopUpCreate
    {
        [Inject] private ApiService ApiService { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        [Parameter]
        [SupplyParameterFromQuery]
        public int? CardId { get; set; }

        private TopUpCreateRequestModel request = new();
        private List<CardModel> cards = new();
        private string? message;
        private bool isSaving;
        private bool isSuccess;
        private TopUpCreateResponseModel? responseData;

        private string? cardSearchText;
        private bool isCardDropdownOpen;

        private IEnumerable<CardModel> FilteredCards
        {
            get
            {
                if (string.IsNullOrWhiteSpace(cardSearchText))
                {
                    return cards.Take(50);
                }
                var lower = cardSearchText.ToLowerInvariant();
                return cards.Where(c => c.CardNum.ToLowerInvariant().Contains(lower) || c.OwnerName.ToLowerInvariant().Contains(lower)).Take(50);
            }
        }

        protected override async Task OnInitializedAsync()
        {
            await LoadCards();
            if (CardId.HasValue && CardId.Value > 0)
            {
                request.CardId = CardId.Value;
                var card = cards.FirstOrDefault(c => c.CardId == request.CardId);
                if (card != null)
                {
                    cardSearchText = $"{card.CardNum} - {card.OwnerName}";
                }
            }
        }

        private void ShowCardDropdown()
        {
            isCardDropdownOpen = true;
        }

        private async Task HideCardDropdown()
        {
            await Task.Delay(200);
            isCardDropdownOpen = false;

            if (request.CardId > 0)
            {
                var card = cards.FirstOrDefault(c => c.CardId == request.CardId);
                if (card != null)
                {
                    cardSearchText = $"{card.CardNum} - {card.OwnerName}";
                }
            }
            else
            {
                cardSearchText = null;
            }
        }

        private void SelectCard(CardModel card)
        {
            request.CardId = card.CardId;
            cardSearchText = $"{card.CardNum} - {card.OwnerName}";
            isCardDropdownOpen = false;
        }

        private async Task LoadCards()
        {
            // Fetch all cards for selector. Let's request page size 1000 to get a large set.
            var result = await ApiService.GetCards(new CardListRequestModel { PageNo = 1, PageSize = 1000 });
            if (result.IsSuccess && result.Data != null)
            {
                cards = result.Data.Cards;
            }
        }

        private bool Validate()
        {
            if (request.CardId <= 0)
            {
                message = "Please select a card.";
                return false;
            }

            if (request.Amount < 1000)
            {
                message = "Minimum top-up amount is 1,000 MMK.";
                return false;
            }

            if (request.Amount > 100000)
            {
                message = "Maximum top-up amount is 100,000 MMK.";
                return false;
            }

            if (!string.IsNullOrEmpty(request.Remark) && request.Remark.Length > 250)
            {
                message = "Remark cannot exceed 250 characters.";
                return false;
            }

            return true;
        }

        private async Task Save()
        {
            message = null;
            responseData = null;
            isSuccess = false;

            if (!Validate())
            {
                return;
            }

            isSaving = true;

            var response = await ApiService.TopUpCreate(request);
            message = response.Message;
            isSuccess = response.IsSuccess;

            if (response.IsSuccess && response.Data != null)
            {
                responseData = response.Data;
                request = new TopUpCreateRequestModel(); // Reset form
                cardSearchText = null;
            }

            isSaving = false;
        }
    }
}
