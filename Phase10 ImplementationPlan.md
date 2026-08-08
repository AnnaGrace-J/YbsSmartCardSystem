# Phase 10 Implementation Plan: Add AuditLog

## Goal

Implement AuditLog writing and viewing for important system actions.

This phase records who did what, when, from where, and what changed where practical.

## Scope

Add AuditLog support across:

- Contracts
- Infrastructure
- Domain
- API
- Blazor App

Use the current maintenance architecture:

- Infrastructure writes audit logs
- Domain calls the audit writer after successful business actions
- API exposes audit log viewing
- Blazor shows audit logs to authorized users

## Prerequisites

Phase 7, Phase 8, and Phase 9 must be complete.

Confirm:

```text
JWT authentication works
Dynamic RBAC works
Tbl_AuditLog exists
Tbl_User exists
AppDbContext has AuditLog DbSet
AuditLog.View permission exists
```

## Step 1: Add AuditLog Contracts

Create:

```text
YbsSmartCardSystem.Contracts/Features/AuditLog/AuditLogModels.cs
```

Add these models:

```text
AuditLogListRequestModel
AuditLogListResponseModel
AuditLogModel
```

`AuditLogListRequestModel` should support:

```text
PageNo
PageSize
UserId
Action
FeatureName
EntityName
FromDate
ToDate
```

`AuditLogModel` should include:

```text
AuditLogId
UserId
UserName
Action
FeatureName
EntityName
EntityId
OldValue
NewValue
IpAddress
UserAgent
CreatedDateTime
```

## Step 2: Add Audit Constants

Create:

```text
YbsSmartCardSystem.Shared/Constants/AuditActions.cs
```

Recommended constants:

```text
Login
CreateCard
UpdateCard
DeleteCard
CreatePackage
UpdatePackage
DeletePackage
TopUp
BusTap
PermissionChanged
RoleChanged
```

Use constants instead of raw strings inside services.

## Step 3: Add Audit Writer Infrastructure

Create:

```text
YbsSmartCardSystem.Infrastructure/AuditLog/IAuditLogWriter.cs
YbsSmartCardSystem.Infrastructure/AuditLog/AuditLogWriteModel.cs
YbsSmartCardSystem.Infrastructure/AuditLog/AuditLogWriter.cs
```

`IAuditLogWriter` should expose:

```csharp
Task WriteAsync(AuditLogWriteModel model, CancellationToken cancellationToken = default);
```

`AuditLogWriteModel` should contain:

```text
UserId
Action
FeatureName
EntityName
EntityId
OldValue
NewValue
IpAddress
UserAgent
```

`AuditLogWriter` should:

- Validate `Action` and `FeatureName`
- Serialize `OldValue` and `NewValue` with `System.Text.Json`
- Insert a new `TblAuditLog`
- Save changes
- Avoid breaking the main business workflow if audit writing fails

## Step 4: Add Request Context Support

Extend `ICurrentUserService` if needed:

```csharp
string? IpAddress { get; }
string? UserAgent { get; }
```

Read values from:

```text
HttpContext.Connection.RemoteIpAddress
HttpContext.Request.Headers.UserAgent
```

If the API is behind a proxy, configure forwarded headers during deployment.

## Step 5: Register Audit Services

Update:

```text
YbsSmartCardSystem.Infrastructure/Extensions/InfrastructureServiceCollectionExtensions.cs
```

Register:

```csharp
services.AddScoped<IAuditLogWriter, AuditLogWriter>();
services.AddHttpContextAccessor();
```

## Step 6: Add AuditLog Domain Service

Create:

```text
YbsSmartCardSystem.Domain/Features/AuditLog/AuditLogService.cs
```

Required method:

```csharp
Result<AuditLogListResponseModel> GetList(AuditLogListRequestModel request)
```

Rules:

- Default request if null
- Validate `PageNo > 0`
- Validate `PageSize > 0`
- Cap `PageSize` at 100
- Filter by user, action, feature, entity, and date range
- Sort newest first
- Use `AsNoTracking()`

## Step 7: Add AuditLog API Controller

Create:

```text
YbsSmartCardSystem.Api/Controllers/AuditLogController.cs
```

Endpoint:

```text
GET /api/AuditLog
```

The controller should:

- Require authentication
- Require `AuditLog.View`
- Inject `AuditLogService`
- Return through `BaseController.Execute(result)`

## Step 8: Register AuditLogService

Update:

```text
YbsSmartCardSystem.Api/Program.cs
```

Add:

```csharp
builder.Services.AddScoped<AuditLogService>();
```

## Step 9: Write Audit Logs For Important Actions

Inject `IAuditLogWriter` into services and write logs after successful actions:

```text
AuthService -> Login
CardService -> CreateCard, UpdateCard, DeleteCard
PackageService -> CreatePackage, UpdatePackage, DeletePackage
TopUpService -> TopUp
TransactionService -> BusTap
RolePermissionService -> RoleChanged, PermissionChanged
```

Include old/new values where practical.

Never log:

```text
Passwords
PasswordHash
PasswordSalt
JWT tokens
Signing keys
Connection strings
```

## Step 10: Add Blazor AuditLog Support

Update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Add:

```csharp
Task<Result<AuditLogListResponseModel>> GetAuditLogs(AuditLogListRequestModel request)
```

Add endpoint:

```csharp
public const string AuditLogList = "api/AuditLog";
```

Create:

```text
YbsSmartCardSystem.App/Components/Features/AuditLog/AuditLogList.razor
YbsSmartCardSystem.App/Components/Features/AuditLog/AuditLogList.razor.cs
```

Minimum UI:

- Paginated table
- Filters for action, feature, user, and date range
- Show action, feature, entity, user, IP, user agent, created date
- Expand or view old and new values

## Step 11: Update Navigation

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Add:

```text
Audit Log
```

Show only for users with:

```text
AuditLog.View
```

If permission-aware navigation is incomplete, add the link and rely on API enforcement.

## Do Not Do In Phase 10

- Do not log passwords, password hashes, JWT tokens, or secrets.
- Do not allow unauthorized users to view audit logs.
- Do not make audit write failures break normal workflows by default.
- Do not replace Serilog/application logs with AuditLog.
- Do not change database schema unless `Tbl_AuditLog` is missing or incorrect.
- Do not redesign the whole UI.
- Do not introduce repositories.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run:

```powershell
rg "IAuditLogWriter"
rg "AuditLogWriter"
rg "AuditLogService"
rg "AuditLogController"
rg "AuditActions"
rg "AuditLog.View"
```

Manual API test:

```http
GET /api/AuditLog
```

Expected:

- No token returns 401.
- Token without `AuditLog.View` returns 403.
- Token with `AuditLog.View` returns paginated logs.

Business action tests:

```http
POST /api/Auth/Login
POST /api/Card
PATCH /api/Card/{id}
POST /api/TopUp
POST /api/Transaction
PUT /api/RolePermission/Roles/{roleId}/Permissions
```

Expected:

- Successful actions create audit log rows.
- Failed validation usually does not create business audit rows.
- Sensitive data is not stored in audit values.

SQL check:

```sql
SELECT TOP 50 *
FROM dbo.Tbl_AuditLog
ORDER BY CreatedDateTime DESC;
```

## Expected Result

- Important system actions are recorded in `Tbl_AuditLog`.
- Authorized users can view audit logs through API and Blazor.
- Audit rows include user, action, feature, entity, old/new values where practical, IP, user agent, and timestamp.
- Sensitive data is not logged.
- Project is ready for Phase 11: frontend maintenance and Tailwind cleanup.

## Git Milestone

```text
feat: add audit log tracking and viewing
```
