using YbsSmartCardSystem.Domain.Common;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.Card;
using Microsoft.JSInterop;

namespace YbsSmartCardSystem.App.Components.Features.Card
{
    public partial class CardList
    {
        private CardListRequestModel request = new() { PageNo = 1, PageSize = 10 };
        private Result<CardListResponseModel> response = new();
        private CardPatchRequestModel editCard = new();
        private int? editingCardId;
        private string? editingCardNum;
        private string? editingMobileNo;
        private string? message;
        private bool isSaving;
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
            editingCardNum = card.CardNum;
            editingMobileNo = card.MobileNo;
            editCard = new CardPatchRequestModel
            {
                OwnerName = card.OwnerName
                // CardNum and MobileNo are no longer editable
            };
            message = null;
        }

        private void CancelEdit()
        {
            editingCardId = null;
            editingCardNum = null;
            editingMobileNo = null;
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

        [Microsoft.AspNetCore.Components.Inject]
        private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = null!;

        private async Task DeleteCard(int id)
        {
            var confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this card?");
            if (!confirmed) return;

            var result = await ApiService.CardDelete(id);
            message = result.Message;

            if (result.IsSuccess)
            {
                await LoadCards();
            }
        }
    }
}
