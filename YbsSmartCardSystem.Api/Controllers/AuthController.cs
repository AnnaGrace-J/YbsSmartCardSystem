using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using YbsSmartCardSystem.Contracts.Features.Auth;
using YbsSmartCardSystem.Domain.Features.Auth;
using YbsSmartCardSystem.Domain.Common;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : BaseController
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("Login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequestModel request)
    {
        var result = _authService.Login(request);
        return Execute(result);
    }

    [HttpGet("Profile")]
    [Authorize]
    public IActionResult Profile()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var model = new CurrentUserModel
        {
            UserId = userId,
            UserName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty,
            PhoneNumber = User.FindFirstValue("PhoneNumber") ?? string.Empty,
            Roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList()
        };

        return Ok(new Result<CurrentUserModel> { IsSuccess = true, Data = model, StatusCode = 200, Message = "Profile retrieved successfully." });
    }

    [HttpPost("Register/SendOtp")]
    [AllowAnonymous]
    public async Task<IActionResult> SendRegistrationOtp([FromBody] UserRegistrationSendOtpRequestModel request)
    {
        var result = await _authService.SendUserRegistrationOtpAsync(request);
        return Execute(result);
    }

    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserRegisterRequestModel request)
    {
        var result = await _authService.RegisterAsync(request);
        return Execute(result);
    }

    [HttpGet("Dashboard")]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        var phoneNumber = User.FindFirstValue(ClaimTypes.MobilePhone);
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return Unauthorized();
        }

        var result = await _authService.GetUserDashboardAsync(phoneNumber);
        return Execute(result);
    }

    [HttpGet("Permissions")]
    [Authorize]
    public IActionResult GetPermissions()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var result = _authService.GetPermissions(userId);
        return Execute(result);
    }
}
