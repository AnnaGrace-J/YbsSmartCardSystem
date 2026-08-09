using YbsSmartCardSystem.Domain.Common;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Domain;
using YbsSmartCardSystem.Contracts.Features.BusPayment;

namespace YbsSmartCardSystem.App.Components.Features.Bus
{
    public partial class BusList
    {
        [Inject] private ApiService ApiService { get; set; } = null!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

        private BusListRequestModel request = new() { PageNo = 1, PageSize = 10 };
        private Result<BusListResponseModel> response = new();
        private BusPatchRequestModel editBus = new();
        private int? editingBusId;
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
            await LoadBuses();
        }

        private async Task LoadBuses()
        {
            request.Search = searchText;
            response = await ApiService.GetBuses(request);
        }

        private async Task Search()
        {
            request.PageNo = 1;
            await LoadBuses();
        }

        private void EditBus(BusModel bus)
        {
            editingBusId = bus.BusId;
            editBus = new BusPatchRequestModel
            {
                BusNo      = bus.BusNo,
                BusLicense = bus.BusLicense
            };
            message = null;
        }

        private void CancelEdit()
        {
            editingBusId = null;
            editBus = new BusPatchRequestModel();
            message = null;
        }

        private bool ValidateEdit()
        {
            if (string.IsNullOrWhiteSpace(editBus.BusNo))
            {
                message = "Bus number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(editBus.BusLicense))
            {
                message = "Bus license is required.";
                return false;
            }

            if (editBus.BusNo.Trim().Length > 50)
            {
                message = "Bus number cannot exceed 50 characters.";
                return false;
            }

            if (editBus.BusLicense.Trim().Length > 50)
            {
                message = "Bus license cannot exceed 50 characters.";
                return false;
            }

            return true;
        }

        private async Task UpdateBus()
        {
            if (editingBusId is null) return;

            if (!ValidateEdit())
            {
                return;
            }

            isSaving = true;

            var result = await ApiService.BusPatch(editingBusId.Value, editBus);
            message = result.Message;

            if (result.IsSuccess)
            {
                CancelEdit();
                await LoadBuses();
            }

            isSaving = false;
        }

        private async Task DeleteBus(int id)
        {
            bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this bus?");
            if (!confirmed) return;

            var result = await ApiService.BusDelete(id);
            if (result.IsSuccess)
            {
                message = "Bus deleted successfully.";
                if (editingBusId == id)
                {
                    CancelEdit();
                }
                await LoadBuses();
            }
            else
            {
                message = result.Message;
            }
        }

        private async Task ChangePage(int pageNo)
        {
            if (pageNo < 1 || (response.Data != null && pageNo > TotalPages)) return;
            request.PageNo = pageNo;
            await LoadBuses();
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
