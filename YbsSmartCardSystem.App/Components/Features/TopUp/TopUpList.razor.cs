using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;

namespace YbsSmartCardSystem.App.Components.Features.TopUp
{
    public partial class TopUpList
    {
        [Inject] private ApiService ApiService { get; set; } = null!;

        private TopUpListRequestModel request = new() { PageNo = 1, PageSize = 10 };
        private Result<TopUpListResponseModel> response = new();
        private List<CardModel> cards = new();
        private int selectedCardId = 0;

        protected override async Task OnInitializedAsync()
        {
            await LoadCards();
            await LoadTopUps();
        }

        private async Task LoadCards()
        {
            var result = await ApiService.GetCards(new CardListRequestModel { PageNo = 1, PageSize = 1000 });
            if (result.IsSuccess && result.Data != null)
            {
                cards = result.Data.Cards;
            }
        }

        private async Task LoadTopUps()
        {
            request.CardId = selectedCardId;
            response = await ApiService.GetTopUps(request);
        }

        private async Task OnCardFilterChanged(ChangeEventArgs e)
        {
            if (int.TryParse(e.Value?.ToString(), out var cardId))
            {
                selectedCardId = cardId;
                request.PageNo = 1;
                await LoadTopUps();
            }
        }

        private async Task ChangePage(int pageNo)
        {
            if (pageNo < 1 || (response.Data != null && pageNo > TotalPages)) return;
            request.PageNo = pageNo;
            await LoadTopUps();
        }

        private int TotalPages
        {
            get
            {
                if (response.Data == null || response.Data.TotalCount == 0) return 1;
                return (int)Math.Ceiling((double)response.Data.TotalCount / request.PageSize);
            }
        }
    }
}
