using System;

namespace YbsSmartCardSystem.Contracts.Features.Card;

public class CardRegistrationSendOtpRequestModel
{
    public string PhoneNumber { get; set; } = string.Empty;
}

public class CardRegistrationSendOtpResponseModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class CardRegistrationVerifyOtpRequestModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public class CardRegistrationVerifyOtpResponseModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}
