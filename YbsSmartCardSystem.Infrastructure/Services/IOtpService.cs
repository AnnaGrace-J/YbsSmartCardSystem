using System.Threading.Tasks;

namespace YbsSmartCardSystem.Infrastructure.Services;

public interface IOtpService
{
    Task<(string PhoneNumber, System.DateTime ExpiresAt)> SendOtpAsync(string phoneNumber, string purpose);
    Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode, string purpose);
}
