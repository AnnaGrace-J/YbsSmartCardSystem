using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Domain.Features.Card.Models;

namespace YbsSmartCardSystem.App.Components.Features.Card
{
    public partial class CardList
    {
        private CardListRequestModel request = new();
        private Result<CardListResponseModel> response = new();
        private int rowNo = 0;
        protected override async Task OnInitializedAsync()
        {
            request = new CardListRequestModel
            {
                PageNo = 1,
                PageSize = 10
            };
            await ApiService.GetCards(request);
        }
    }
}
