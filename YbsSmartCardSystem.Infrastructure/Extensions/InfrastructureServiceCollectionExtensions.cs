using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YbsSmartCardSystem.Infrastructure.AuditLog;
using YbsSmartCardSystem.Infrastructure.Authentication;
using YbsSmartCardSystem.Infrastructure.Authorization.DynamicRbac;
using YbsSmartCardSystem.Infrastructure.Services;

namespace YbsSmartCardSystem.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordService, PasswordService>();

        // Dynamic RBAC
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        // Audit Log
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();

        // OTP Service
        services.AddScoped<IOtpService, OtpService>();

        return services;
    }
}
