using Microsoft.EntityFrameworkCore;
using YbsSmartCardSystem.Contracts.Features.Auth;
using YbsSmartCardSystem.Database.AppDbContextModels;
using YbsSmartCardSystem.Domain.Common;
using YbsSmartCardSystem.Infrastructure.Authentication;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Services;
using YbsSmartCardSystem.Shared.Constants;

namespace YbsSmartCardSystem.Domain.Features.Auth;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IPasswordService _passwordService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IAuditLogWriter _audit;
    private readonly ICurrentUserService _currentUser;
    private readonly IOtpService _otpService;

    public AuthService(AppDbContext db, IPasswordService passwordService, IJwtTokenService jwtTokenService, IAuditLogWriter audit, ICurrentUserService currentUser, IOtpService otpService)
    {
        _db = db;
        _passwordService = passwordService;
        _jwtTokenService = jwtTokenService;
        _audit = audit;
        _currentUser = currentUser;
        _otpService = otpService;
    }

    public Result<LoginResponseModel> Login(LoginRequestModel request)
    {
        try
        {
            var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(request.Password))
            {
                return new Result<LoginResponseModel> { IsSuccess = false, StatusCode = 400, Message = "PhoneNumber and Password are required." };
            }

            var staffUser = _db.TblStaffUsers
                .Include(u => u.TblUserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefault(u => (u.PhoneNo == phoneNumber || u.UserName == phoneNumber) && u.IsActive && !u.DeleteFlag);

            var viewerUser = staffUser == null 
                ? _db.TblViewerUsers.FirstOrDefault(u => (u.PhoneNo == phoneNumber || u.UserName == phoneNumber) && u.IsActive && !u.DeleteFlag)
                : null;

            if (staffUser == null && viewerUser == null)
            {
                return new Result<LoginResponseModel> { IsSuccess = false, StatusCode = 401, Message = "Invalid phone number/username or password." };
            }

            var passwordHash = staffUser != null ? staffUser.PasswordHash : viewerUser!.PasswordHash;

            if (!_passwordService.VerifyPassword(request.Password, passwordHash))
            {
                return new Result<LoginResponseModel> { IsSuccess = false, StatusCode = 401, Message = "Invalid phone number/username or password." };
            }

            var roles = staffUser?.TblUserRoles
                .Where(ur => ur.Role != null && ur.Role.IsActive && !ur.Role.DeleteFlag)
                .Select(ur => ur.Role.RoleName)
                .ToList() ?? new List<string>();

            var userId = staffUser != null ? staffUser.StaffUserId : viewerUser!.ViewerUserId;
            var userName = staffUser != null ? staffUser.UserName : viewerUser!.UserName;
            var actualPhoneNumber = staffUser != null ? staffUser.PhoneNo ?? string.Empty : viewerUser!.PhoneNo;
            var fullName = staffUser != null ? staffUser.FullName : viewerUser!.FullName;
            var userType = staffUser != null ? "Staff" : "Viewer";

            var jwtUser = new JwtTokenUser
            {
                UserId = userId,
                UserName = userName,
                PhoneNumber = actualPhoneNumber,
                FullName = fullName,
                UserType = userType,
                Roles = roles
            };

            var tokenResult = _jwtTokenService.GenerateToken(jwtUser);

            if (staffUser != null)
            {
                staffUser.LastLoginDate = DateTime.Now;
            }
            else
            {
                viewerUser!.LastLoginDate = DateTime.Now;
            }
            _db.SaveChanges();

            _ = _audit.WriteAsync(new AuditLogWriteModel
            {
                UserId      = userId,
                Action      = AuditActions.Login,
                FeatureName = "Auth",
                EntityName  = staffUser != null ? "TblStaffUser" : "TblViewerUser",
                EntityId    = userId.ToString(),
                NewValue    = new { UserName = userName, LoginTime = DateTime.Now },
                IpAddress   = _currentUser.IpAddress,
                UserAgent   = _currentUser.UserAgent
            });

            var response = new LoginResponseModel
            {
                UserId = userId,
                UserName = userName,
                PhoneNumber = actualPhoneNumber,
                FullName = fullName,
                Token = tokenResult.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                UserType = userType,
                Roles = roles
            };

            return new Result<LoginResponseModel> { IsSuccess = true, Data = response, Message = "Login successful.", StatusCode = 200 };
        }
        catch (Exception)
        {
            return new Result<LoginResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public Result<CurrentUserPermissionsResponseModel> GetPermissions(int userId)
    {
        try
        {
            if (_currentUser.IsViewer)
            {
                return new Result<CurrentUserPermissionsResponseModel>
                {
                    IsSuccess = true,
                    Data = new CurrentUserPermissionsResponseModel
                    {
                        Permissions = new List<string> { "Bus.View", "Terminal.View" }
                    },
                    Message = "Viewer permissions retrieved successfully.",
                    StatusCode = 200
                };
            }

            var permissions = _db.TblUserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == userId && !ur.DeleteFlag)
                .Join(_db.TblRoles.Where(r => r.IsActive && !r.DeleteFlag),
                    ur => ur.RoleId,
                    r  => r.RoleId,
                    (ur, r) => r)
                .Join(_db.TblRolePermissions.Where(rp => !rp.DeleteFlag),
                    r  => r.RoleId,
                    rp => rp.RoleId,
                    (r, rp) => rp)
                .Join(_db.TblPermissions.Where(p => p.IsActive && !p.DeleteFlag),
                    rp => rp.PermissionId,
                    p  => p.PermissionId,
                    (rp, p) => p.PermissionCode)
                .Distinct()
                .ToList();

            var response = new CurrentUserPermissionsResponseModel
            {
                Permissions = permissions
            };

            return new Result<CurrentUserPermissionsResponseModel>
            {
                IsSuccess = true,
                Data = response,
                Message = "Permissions retrieved successfully.",
                StatusCode = 200
            };
        }
        catch (Exception)
        {
            return new Result<CurrentUserPermissionsResponseModel>
            {
                IsSuccess = false,
                StatusCode = 500,
                Message = "An unexpected error occurred."
            };
        }
    }

    public async Task<Result<UserRegistrationSendOtpResponseModel>> SendUserRegistrationOtpAsync(UserRegistrationSendOtpRequestModel request)
    {
        try
        {
            var userName = request.UserName?.Trim();
            var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            if (string.IsNullOrWhiteSpace(userName) || userName.Length > 100)
                return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Valid Username is required." };
            
            if (string.IsNullOrWhiteSpace(phoneNumber) || phoneNumber.Length > 20)
                return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Valid Phone number is required." };
            
            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Password must be at least 6 characters." };

            if (await _db.TblStaffUsers.AnyAsync(u => u.UserName == userName && !u.DeleteFlag) || 
                await _db.TblViewerUsers.AnyAsync(u => u.UserName == userName && !u.DeleteFlag))
                return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Username already exists." };

            if (await _db.TblStaffUsers.AnyAsync(u => u.PhoneNo == phoneNumber && !u.DeleteFlag) ||
                await _db.TblViewerUsers.AnyAsync(u => u.PhoneNo == phoneNumber && !u.DeleteFlag))
                return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Phone number already exists." };

            var otpResult = await _otpService.SendOtpAsync(phoneNumber, "UserRegistration");

            var response = new UserRegistrationSendOtpResponseModel
            {
                PhoneNumber = otpResult.PhoneNumber,
                ExpiresAt = otpResult.ExpiresAt
            };

            return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = true, Data = response, Message = "OTP sent successfully." };
        }
        catch (Exception)
        {
            return new Result<UserRegistrationSendOtpResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public async Task<Result<UserRegisterResponseModel>> RegisterAsync(UserRegisterRequestModel request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var userName = request.UserName?.Trim();
            var phoneNumber = NormalizePhoneNumber(request.PhoneNumber);

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(phoneNumber) || 
                string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.OtpCode))
            {
                return new Result<UserRegisterResponseModel> { IsSuccess = false, StatusCode = 400, Message = "All fields are required." };
            }

            if (await _db.TblStaffUsers.AnyAsync(u => u.UserName == userName && !u.DeleteFlag) || 
                await _db.TblViewerUsers.AnyAsync(u => u.UserName == userName && !u.DeleteFlag))
                return new Result<UserRegisterResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Username already exists." };

            if (await _db.TblStaffUsers.AnyAsync(u => u.PhoneNo == phoneNumber && !u.DeleteFlag) ||
                await _db.TblViewerUsers.AnyAsync(u => u.PhoneNo == phoneNumber && !u.DeleteFlag))
                return new Result<UserRegisterResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Phone number already exists." };

            var isOtpValid = await _otpService.VerifyOtpAsync(phoneNumber, request.OtpCode.Trim(), "UserRegistration");
            if (!isOtpValid)
            {
                return new Result<UserRegisterResponseModel> { IsSuccess = false, StatusCode = 400, Message = "Invalid or expired OTP." };
            }

            var passwordHash = _passwordService.HashPassword(request.Password);

            var newUser = new TblViewerUser
            {
                UserName = userName,
                FullName = userName, // Using UserName as default FullName
                PhoneNo = phoneNumber,
                PasswordHash = passwordHash,
                IsActive = true,
                CreatedDate = DateTime.Now,
                DeleteFlag = false
            };

            _db.TblViewerUsers.Add(newUser);
            await _db.SaveChangesAsync();

            // OTP is already verified and marked as verified in VerifyOtpAsync
            
            await transaction.CommitAsync();

            var response = new UserRegisterResponseModel
            {
                UserId = newUser.ViewerUserId,
                UserName = newUser.UserName,
                PhoneNumber = newUser.PhoneNo
            };

            return new Result<UserRegisterResponseModel> { IsSuccess = true, Data = response, Message = "User registered successfully." };
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return new Result<UserRegisterResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    public async Task<Result<UserDashboardResponseModel>> GetUserDashboardAsync(string phoneNumber)
    {
        try
        {
            phoneNumber = NormalizePhoneNumber(phoneNumber);

            var user = await _db.TblViewerUsers.FirstOrDefaultAsync(u => u.PhoneNo == phoneNumber && !u.DeleteFlag && u.IsActive);
            if (user == null)
            {
                return new Result<UserDashboardResponseModel> { IsSuccess = false, StatusCode = 404, Message = "Viewer user not found." };
            }

            var cards = await _db.TblCards
                .Where(c => c.MobileNo == phoneNumber && !c.DeleteFlag)
                .Select(c => new UserCardSummaryModel
                {
                    CardId = c.CardId,
                    CardNum = c.CardNum,
                    OwnerName = c.OwnerName,
                    MobileNo = c.MobileNo ?? string.Empty,
                    Balance = c.Balance
                })
                .ToListAsync();

            var response = new UserDashboardResponseModel
            {
                UserId = user.ViewerUserId,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNo,
                Cards = cards
            };

            return new Result<UserDashboardResponseModel> { IsSuccess = true, Data = response, StatusCode = 200, Message = "Dashboard retrieved successfully." };
        }
        catch (Exception)
        {
            return new Result<UserDashboardResponseModel> { IsSuccess = false, StatusCode = 500, Message = "An unexpected error occurred." };
        }
    }

    private static string NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return string.Empty;
        }

        var chars = phoneNumber.Trim()
            .Where(c => !char.IsWhiteSpace(c) && c != '-' && c != '(' && c != ')')
            .ToArray();

        return new string(chars);
    }
}
