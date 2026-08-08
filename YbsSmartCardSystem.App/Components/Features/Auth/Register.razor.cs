using Microsoft.AspNetCore.Components;
using YbsSmartCardSystem.Contracts.Features.Auth;

namespace YbsSmartCardSystem.App.Components.Features.Auth;

public partial class Register : ComponentBase
{
    private UserRegisterRequestModel Model { get; set; } = new();
    private string ConfirmPassword { get; set; } = string.Empty;
    private int CurrentStep { get; set; } = 1;
    private bool IsSubmitting { get; set; }
    private string? ErrorMessage { get; set; }

    private async Task HandleSendOtp()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Model.UserName) || string.IsNullOrWhiteSpace(Model.PhoneNumber) || string.IsNullOrWhiteSpace(Model.Password))
        {
            ErrorMessage = "Please fill in all fields.";
            return;
        }

        if (Model.Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        if (Model.Password.Length < 6)
        {
            ErrorMessage = "Password must be at least 6 characters.";
            return;
        }

        IsSubmitting = true;

        var request = new UserRegistrationSendOtpRequestModel
        {
            UserName = Model.UserName,
            PhoneNumber = Model.PhoneNumber,
            Password = Model.Password
        };

        var result = await Api.SendUserRegistrationOtp(request);
        if (result.IsSuccess)
        {
            if (result.Data is not null)
            {
                Model.PhoneNumber = result.Data.PhoneNumber;
            }

            CurrentStep = 2;
        }
        else
        {
            ErrorMessage = result.Message ?? "Failed to send OTP.";
        }

        IsSubmitting = false;
    }

    private async Task HandleRegister()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(Model.OtpCode))
        {
            ErrorMessage = "Please enter the OTP code.";
            return;
        }

        IsSubmitting = true;

        var result = await Api.Register(Model);
        if (result.IsSuccess)
        {
            NavManager.NavigateTo("/login");
        }
        else
        {
            ErrorMessage = result.Message ?? "Registration failed.";
        }

        IsSubmitting = false;
    }
}
