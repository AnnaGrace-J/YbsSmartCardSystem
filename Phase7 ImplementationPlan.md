# Phase 7 Implementation Plan: Add Authentication

## Goal

Implement user authentication with secure password verification and JWT token issuance.

This phase adds login/logout support and prepares the API/App for protected workflows. Dynamic RBAC permission enforcement is handled later in Phase 8/9.

## Scope

Add authentication across:

- Contracts
- Domain
- Infrastructure
- API
- Blazor App

Use existing maintenance architecture:

- Domain contains `AuthService` workflow
- Infrastructure contains JWT/password/current-user technical services
- Database contains scaffolded `TblUser`, `TblRole`, `TblUserRole`
- API controller delegates to Domain service
- Blazor calls API through `ApiService`

## Prerequisites

Phase 4 and Phase 5 must be complete.

Confirm these generated files exist:

```text
YbsSmartCardSystem.Database/AppDbContextModels/TblUser.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblRole.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblUserRole.cs
```

Confirm `AppDbContext` has DbSets for users and roles.

## Step 1: Add Auth Contracts

Create:

```text
YbsSmartCardSystem.Contracts/Features/Auth/AuthModels.cs
```

Suggested models:

```csharp
namespace YbsSmartCardSystem.Contracts.Features.Auth;

public class LoginRequestModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = [];
}

public class CurrentUserModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
```

## Step 2: Add Infrastructure Interfaces

Create interfaces in:

```text
YbsSmartCardSystem.Infrastructure/Authentication
```

Required interfaces:

```csharp
public interface IJwtTokenService
{
    JwtTokenResult GenerateToken(JwtTokenUser user);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}
```

Suggested support models:

```csharp
public class JwtTokenUser
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}

public class JwtTokenResult
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
```

## Step 3: Implement PasswordService

Create:

```text
YbsSmartCardSystem.Infrastructure/Authentication/PasswordService.cs
```

Use ASP.NET Core password hashing:

```csharp
Microsoft.AspNetCore.Identity.PasswordHasher<T>
```

Recommended approach:

```csharp
PasswordHasher<object>
```

Do not store plain text passwords.

Do not use reversible encryption.

If existing database has plain text test passwords, migrate them manually or create a secure seed process. Do not implement plain text fallback unless explicitly required for a temporary migration, and if used, remove it immediately after migration.

## Step 4: Implement JwtTokenService

Create:

```text
YbsSmartCardSystem.Infrastructure/Authentication/JwtTokenService.cs
```

Read settings from configuration:

```text
Jwt:Issuer
Jwt:Audience
Jwt:SigningKey
Jwt:ExpiryMinutes
```

Token claims should include:

```text
NameIdentifier = UserId
Name = UserName
FullName
Role claims
```

Use:

```text
System.IdentityModel.Tokens.Jwt
Microsoft.IdentityModel.Tokens
```

If packages are missing, add package references to the appropriate project.

## Step 5: Add Infrastructure DI Extension

Create:

```text
YbsSmartCardSystem.Infrastructure/Extensions/InfrastructureServiceCollectionExtensions.cs
```

Add:

```csharp
public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
```

Register:

```csharp
services.AddScoped<IJwtTokenService, JwtTokenService>();
services.AddScoped<IPasswordService, PasswordService>();
```

JWT authentication pipeline setup may also live here or in API extensions.

## Step 6: Add Auth Domain Service

Create:

```text
YbsSmartCardSystem.Domain/Features/Auth/AuthService.cs
```

Dependencies:

```csharp
AppDbContext
IPasswordService
IJwtTokenService
```

Required method:

```csharp
Result<LoginResponseModel> Login(LoginRequestModel request)
```

Login workflow:

1. Validate request.
2. Find active, non-deleted user by username.
3. Verify password hash.
4. Load active, non-deleted roles.
5. Generate JWT token.
6. Update `LastLoginDate`.
7. Return user info, token, expiry, roles.

Recommended failed login response:

```text
StatusCode = 401
Message = "Invalid username or password."
```

Do not reveal whether username or password was wrong.

## Step 7: Add Auth API Controller

Create:

```text
YbsSmartCardSystem.Api/Controllers/AuthController.cs
```

Endpoints:

```text
POST /api/Auth/Login
GET  /api/Auth/Profile
```

`Login`:

- Allows anonymous access
- Calls `AuthService.Login`
- Returns through `BaseController.Execute(result)`

`Profile`:

- Requires authentication
- Returns current user claims
- Can be simple for this phase

Do not add permission checks yet.

## Step 8: Configure API JWT Authentication

Update:

```text
YbsSmartCardSystem.Api/Program.cs
```

Add:

```csharp
builder.Services.AddAuthentication(...)
builder.Services.AddAuthorization()
app.UseAuthentication()
app.UseAuthorization()
```

Important order:

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

`UseAuthentication()` must come before `UseAuthorization()`.

Keep Swagger enabled in development.

Optional: configure Swagger Bearer token support in this phase.

## Step 9: Add JWT Settings

Update:

```text
YbsSmartCardSystem.Api/appsettings.Development.json
```

Add development-only settings:

```json
"Jwt": {
  "Issuer": "YbsSmartCardSystem",
  "Audience": "YbsSmartCardSystem",
  "SigningKey": "CHANGE_THIS_DEVELOPMENT_SECRET_TO_AT_LEAST_32_CHARS",
  "ExpiryMinutes": 60
}
```

Do not commit production secrets.

For production, use environment variables or user secrets.

## Step 10: Register AuthService

Update API DI registration:

```csharp
builder.Services.AddScoped<AuthService>();
```

Also register Infrastructure services:

```csharp
builder.Services.AddInfrastructureServices(builder.Configuration);
```

## Step 11: Seed Or Create Initial Admin User

Create a safe development-only admin setup.

Options:

1. Create a small one-time console/script to generate password hash.
2. Add a temporary development endpoint guarded by environment check.
3. Insert hash manually into SQL after generating it.

Recommended:

- Use a one-time local script or small utility to generate the hash.
- Insert admin user into `Tbl_User`.
- Assign Admin role in `Tbl_UserRole`.

Do not seed plain text passwords.

Do not expose admin creation endpoint in production.

## Step 12: Update Blazor ApiService

Update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Add:

```csharp
Task<Result<LoginResponseModel>> Login(LoginRequestModel request)
```

Add endpoint:

```csharp
public const string Login = "api/Auth/Login";
```

Do not attach JWT to every request yet unless token storage is implemented in this phase.

## Step 13: Add Blazor Auth Pages

Create:

```text
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor.cs
```

Minimum UI:

- Username input
- Password input
- Login button
- Error message
- Redirect after successful login

For token storage, use a simple service:

```text
YbsSmartCardSystem.App/Services/AuthStateService.cs
```

Store token in memory for this phase unless persistent login is explicitly required.

Persistent browser storage can be added later.

## Step 14: Attach JWT To API Calls

If `AuthStateService` exists, update `ApiService` to attach:

```text
Authorization: Bearer {token}
```

to protected calls.

For Phase 7, only login must work. Protecting feature endpoints can wait until RBAC phases.

## Step 15: Update Navigation

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Add:

```text
Login
Logout
```

Keep role-aware navigation for later RBAC phases.

## Do Not Do In Phase 7

- Do not implement Dynamic RBAC permission checks.
- Do not implement RolePermission management UI.
- Do not implement AuditLog writing except optional login audit placeholder.
- Do not protect every endpoint unless required.
- Do not store plain text passwords.
- Do not commit production JWT secrets.
- Do not redesign the whole Blazor UI.
- Do not introduce repositories.
- Do not change existing business routes.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run:

```powershell
rg "UseAuthentication"
rg "Jwt"
rg "AuthService"
rg "AuthController"
rg "PasswordService"
rg "JwtTokenService"
```

Manual API tests:

```http
POST /api/Auth/Login
```

Expected:

- Valid credentials return token and user info.
- Invalid credentials return 401.
- Deleted or inactive users cannot login.

Optional profile test:

```http
GET /api/Auth/Profile
Authorization: Bearer {token}
```

Expected:

- Valid token returns current user claims.
- Missing/invalid token returns 401.

## Expected Result

- Users can login with username/password.
- API can issue JWT tokens.
- API authentication middleware is configured.
- Blazor has a basic login flow.
- Project is ready for Phase 8: Role and Permission Management.

## Git Milestone

```text
feat: add jwt authentication
```
