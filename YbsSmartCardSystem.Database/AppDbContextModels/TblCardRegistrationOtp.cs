using System;
using System.Collections.Generic;

namespace YbsSmartCardSystem.Database.AppDbContextModels;

public partial class TblCardRegistrationOtp
{
    public int OtpId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string OtpCodeHash { get; set; } = null!;

    public string Purpose { get; set; } = null!;

    public DateTime ExpiresAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public int AttemptCount { get; set; }

    public int MaxAttemptCount { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool DeleteFlag { get; set; }
}
