using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.App.Services;
using YbsSmartCardSystem.Contracts.Features.BusPayment;

namespace YbsSmartCardSystem.App.Components.Features.Bus
{
    public partial class BusCreate
    {
        [Inject] private ApiService ApiService { get; set; } = null!;
        [Inject] private NavigationManager NavigationManager { get; set; } = null!;

        private BusCreateRequestModel request = new();
        private string? message;
        private bool isSaving;
        private bool isSuccess;

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(request.BusNo))
            {
                message = "Bus number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.BusLicense))
            {
                message = "Bus license is required.";
                return false;
            }

            if (request.BusNo.Trim().Length > 50)
            {
                message = "Bus number cannot exceed 50 characters.";
                return false;
            }

            if (request.BusLicense.Trim().Length > 50)
            {
                message = "Bus license cannot exceed 50 characters.";
                return false;
            }

            return true;
        }

        private async Task Save()
        {
            message = null;

            if (!Validate())
            {
                return;
            }

            isSaving = true;

            var response = await ApiService.BusCreate(request);
            message = response.Message;
            isSuccess = response.IsSuccess;

            if (response.IsSuccess)
            {
                request = new BusCreateRequestModel();
                NavigationManager.NavigateTo("/buses");
                return;
            }

            isSaving = false;
        }
    }
}
