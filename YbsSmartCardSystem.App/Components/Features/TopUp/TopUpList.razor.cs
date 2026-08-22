using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;

namespace YbsSmartCardSystem.App.Components.Features.TopUp
{
    public partial class TopUpList
    {
        [Inject] private ApiService ApiService { get; set; } = null!;
        [Inject] private Microsoft.JSInterop.IJSRuntime JSRuntime { get; set; } = null!;

        private TopUpListRequestModel request = new() { PageNo = 1, PageSize = 10 };
        private Result<TopUpListResponseModel> response = new();
        private string? copiedItem;

        protected override async Task OnInitializedAsync()
        {
            await LoadTopUps();
        }

        private async Task LoadTopUps()
        {
            request.Search = searchText;
            response = await ApiService.GetTopUps(request);
        }

        private string? searchText;

private async Task Search()
        {
            request.PageNo = 1;
            await LoadTopUps();
        }

        private async Task Clear()
        {
            searchText = null;
            request.FilterDate = null;
            request.PageNo = 1;
            await LoadTopUps();
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
}
