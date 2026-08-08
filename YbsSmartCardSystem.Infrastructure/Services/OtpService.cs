using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YbsSmartCardSystem.Database.AppDbContextModels;

namespace YbsSmartCardSystem.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<OtpService> _logger;

    public OtpService(AppDbContext db, ICurrentUserService currentUser, ILogger<OtpService> logger)
    {
        _db = db;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<(string PhoneNumber, DateTime ExpiresAt)> SendOtpAsync(string phoneNumber, string purpose)
    {
        // 1. Invalidate old unverified OTPs for this phone number and purpose
        var oldOtps = await _db.TblCardRegistrationOtps
            .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose && !x.DeleteFlag && x.VerifiedAt == null)
            .ToListAsync();
            
        foreach (var old in oldOtps)
        {
            old.DeleteFlag = true;
        }

        // 2. Generate new OTP
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();
        var expiresAt = DateTime.Now.AddMinutes(5);

        // 3. Hash OTP
        var otpHash = HashOtp(otpCode);

        // 4. Save to database
        var otpEntity = new TblCardRegistrationOtp
        {
            PhoneNumber = phoneNumber,
            OtpCodeHash = otpHash,
            Purpose = purpose,
            ExpiresAt = expiresAt,
            CreatedByUserId = _currentUser.UserId ?? 0,
            CreatedDate = DateTime.Now,
            MaxAttemptCount = 5,
            AttemptCount = 0,
            DeleteFlag = false
        };

        _db.TblCardRegistrationOtps.Add(otpEntity);
        await _db.SaveChangesAsync();

        // 5. Send OTP (Simulated for now, logged instead)
        _logger.LogInformation($"[OTP Simulation] Sent OTP {otpCode} to {phoneNumber} for {purpose}");

        return (phoneNumber, expiresAt);
    }

    public async Task<bool> VerifyOtpAsync(string phoneNumber, string otpCode, string purpose)
    {
        var otpEntity = await _db.TblCardRegistrationOtps
            .Where(x => x.PhoneNumber == phoneNumber && x.Purpose == purpose && !x.DeleteFlag && x.VerifiedAt == null)
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefaultAsync();

        if (otpEntity == null) return false;
        
        if (otpEntity.ExpiresAt < DateTime.Now)
        {
            otpEntity.DeleteFlag = true;
            await _db.SaveChangesAsync();
            return false;
        }

        if (otpEntity.AttemptCount >= otpEntity.MaxAttemptCount)
        {
            otpEntity.DeleteFlag = true;
            await _db.SaveChangesAsync();
            return false;
        }

        otpEntity.AttemptCount++;

        var hashedInput = HashOtp(otpCode);
        if (otpEntity.OtpCodeHash == hashedInput)
        {
            otpEntity.VerifiedAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }

        await _db.SaveChangesAsync();
        return false;
    }

    private string HashOtp(string otp)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(otp);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
