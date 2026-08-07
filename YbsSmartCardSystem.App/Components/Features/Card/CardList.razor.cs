using YbsSmartCardSystem.Domain.Common;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.Card;

namespace YbsSmartCardSystem.App.Components.Features.Card
{
    public partial class CardList
    {
        private CardListRequestModel request = new() { PageNo = 1, PageSize = 10 };
        private Result<CardListResponseModel> response = new();
        private CardPatchRequestModel editCard = new();
        private int? editingCardId;
        private string? message;
        private bool isSaving;
        private string searchText = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadCards();
        }

        private async Task LoadCards()
        {
            request.Search = searchText;
            response = await ApiService.GetCards(request);
        }

        private async Task Search()
        {
            request.PageNo = 1;
            await LoadCards();
        }

        private async Task ChangePage(int pageNo)
        {
            if (pageNo < 1 || (response.Data != null && pageNo > TotalPages)) return;
            request.PageNo = pageNo;
            await LoadCards();
        }

        private int TotalPages
        {
            get
            {
                if (response.Data == null || response.Data.TotalCount == 0) return 1;
                return (int)Math.Ceiling((double)response.Data.TotalCount / request.PageSize);
            }
        }

        private void EditCard(CardModel card)
        {
            editingCardId = card.CardId;
            editCard = new CardPatchRequestModel
            {
                CardNum  = card.CardNum,
                OwnerName = card.OwnerName,
                MobileNo  = card.MobileNo
                // Balance is removed from manual editing
            };
            message = null;
        }

        private void CancelEdit()
        {
            editingCardId = null;
            editCard = new CardPatchRequestModel();
            message = null;
        }

        private async Task UpdateCard()
        {
            if (editingCardId is null) return;

            isSaving = true;

            var result = await ApiService.CardPatch(editingCardId.Value, editCard);
            message = result.Message;

            if (result.IsSuccess)
            {
                CancelEdit();
                await LoadCards();
            }

            isSaving = false;
        }
    }
}
