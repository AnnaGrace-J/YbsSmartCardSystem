# Phase 9 Implementation Plan: Dynamic RBAC Enforcement

## Goal

Enforce API access using permissions stored in the database.

Phase 8 created role and permission management. This phase makes those permissions active by protecting API endpoints with Dynamic RBAC.

## Scope

Add permission enforcement across:

- Infrastructure
- API
- Domain where needed for helper lookups only
- Blazor navigation visibility where practical

Do not implement AuditLog yet. AuditLog is Phase 10.

## Prerequisites

Phase 7 and Phase 8 must be complete.

Confirm:

```text
JWT login works
Tbl_User exists
Tbl_Role exists
Tbl_Permission exists
Tbl_UserRole exists
Tbl_RolePermission exists
RolePermissionService works
Authenticated API requests work
```

Confirm seeded permissions include at least:

```text
Card.View
Card.Create
Card.Update
Card.Delete
Package.View
Package.Create
Package.Update
Package.Delete
TopUp.View
TopUp.Create
BusPayment.Create
Transaction.View
RolePermission.View
RolePermission.Manage
AuditLog.View
```

## Step 1: Add Permission Constants

Create:

```text
YbsSmartCardSystem.Shared/Constants/PermissionCodes.cs
```

Suggested content:

```csharp
namespace YbsSmartCardSystem.Shared.Constants;

public static class PermissionCodes
{
    public const string CardView = "Card.View";
    public const string CardCreate = "Card.Create";
    public const string CardUpdate = "Card.Update";
    public const string CardDelete = "Card.Delete";

    public const string PackageView = "Package.View";
    public const string PackageCreate = "Package.Create";
    public const string PackageUpdate = "Package.Update";
    public const string PackageDelete = "Package.Delete";

    public const string TopUpView = "TopUp.View";
    public const string TopUpCreate = "TopUp.Create";

    public const string BusPaymentCreate = "BusPayment.Create";
    public const string TransactionView = "Transaction.View";

    public const string RolePermissionView = "RolePermission.View";
    public const string RolePermissionManage = "RolePermission.Manage";

    public const string AuditLogView = "AuditLog.View";
}
```

Use constants in attributes to avoid typo-based security bugs.

## Step 2: Add Current User Service

Create:

```text
YbsSmartCardSystem.Infrastructure/Services/CurrentUserService.cs
```

Create interface:

```text
YbsSmartCardSystem.Infrastructure/Services/ICurrentUserService.cs
```

Suggested interface:

```csharp
public interface ICurrentUserService
{
    int? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
}
```

Implementation should read claims from `IHttpContextAccessor`.

Register:

```csharp
services.AddHttpContextAccessor();
services.AddScoped<ICurrentUserService, CurrentUserService>();
```

## Step 3: Add Permission Checker

Create:

```text
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/IPermissionChecker.cs
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/PermissionChecker.cs
```

Suggested interface:

```csharp
public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(int userId, string permissionCode, CancellationToken cancellationToken = default);
}
```

Checker behavior:

1. User must exist, be active, and not deleted.
2. User must have at least one active, non-deleted role.
3. Role must be active and not deleted.
4. Role must have active, non-deleted permission assignment.
5. Permission must match `PermissionCode`, be active, and not deleted.

Use `AsNoTracking()`.

Return `false` for missing users, inactive users, deleted roles, inactive permissions, or unknown permission code.

## Step 4: Add Permission Requirement

Create:

```text
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/PermissionRequirement.cs
```

Suggested:

```csharp
public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }

    public string PermissionCode { get; }
}
```

## Step 5: Add Permission Authorization Handler

Create:

```text
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/PermissionAuthorizationHandler.cs
```

Behavior:

1. Ensure user is authenticated.
2. Read user ID from `ClaimTypes.NameIdentifier`.
3. Use `IPermissionChecker`.
4. Succeed only when permission exists through active user role assignment.

Invalid or missing user ID should fail authorization.

## Step 6: Add Permission Attribute

Create:

```text
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/RequirePermissionAttribute.cs
```

Recommended implementation:

```csharp
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "Permission:";

    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = PolicyPrefix + permissionCode;
    }
}
```

## Step 7: Add Dynamic Policy Provider

Create:

```text
YbsSmartCardSystem.Infrastructure/Authorization/DynamicRbac/PermissionPolicyProvider.cs
```

Behavior:

- Detect policies beginning with `Permission:`
- Extract permission code
- Build policy with:

```csharp
policy.RequireAuthenticatedUser();
policy.AddRequirements(new PermissionRequirement(permissionCode));
```

Fallback to default policy provider for normal policies.

## Step 8: Register Dynamic RBAC Services

Update:

```text
YbsSmartCardSystem.Infrastructure/Extensions/InfrastructureServiceCollectionExtensions.cs
```

Register:

```csharp
services.AddScoped<IPermissionChecker, PermissionChecker>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
services.AddHttpContextAccessor();
```

Confirm API already calls:

```csharp
builder.Services.AddInfrastructureServices(builder.Configuration);
```

Confirm API has:

```csharp
builder.Services.AddAuthorization();
app.UseAuthentication();
app.UseAuthorization();
```

Order matters: `UseAuthentication()` before `UseAuthorization()`.

## Step 9: Protect API Endpoints

Apply `[Authorize]` and `[RequirePermission(...)]` to controllers/actions.

Recommended mapping:

### CardController

```text
GET list/get by id -> Card.View
POST -> Card.Create
PATCH/PUT -> Card.Update
DELETE -> Card.Delete
```

### PackageController

```text
GET list/get by id -> Package.View
POST -> Package.Create
PATCH/PUT -> Package.Update
DELETE -> Package.Delete
```

### TopUpController

```text
GET list -> TopUp.View
POST -> TopUp.Create
```

### TransactionController

If it represents bus tap/payment:

```text
POST -> BusPayment.Create
GET list -> Transaction.View
```

### BusController and TerminalController

If no dedicated permissions exist yet, either:

- Add permissions for `Bus.View`, `Bus.Create`, `Bus.Update`, `Bus.Delete`, `Terminal.View`, etc., or
- Temporarily protect them with `RolePermission.Manage` for admin-only maintenance.

Recommended for clarity: add separate Bus and Terminal permissions to database seed if these remain public management features.

### RolePermissionController

```text
GET roles/permissions/assignments -> RolePermission.View
POST/PATCH/DELETE/PUT assignments -> RolePermission.Manage
```

### AuthController

```text
POST /api/Auth/Login -> [AllowAnonymous]
GET /api/Auth/Profile -> [Authorize]
```

Do not require permission for login.

## Step 10: Update Permission Seed If Needed

If Bus and Terminal controllers need dedicated permissions, add these to seed data:

```text
Bus.View
Bus.Create
Bus.Update
Bus.Delete
Terminal.View
Terminal.Create
Terminal.Update
Terminal.Delete
```

Also add constants in `PermissionCodes`.

Assign them to Admin.

## Step 11: Add Permission API For Blazor Navigation

Add an endpoint that returns current user's permission codes.

Recommended endpoint:

```text
GET /api/Auth/Permissions
```

Response model:

```csharp
public class CurrentUserPermissionsResponseModel
{
    public List<string> Permissions { get; set; } = [];
}
```

This can be implemented in `AuthService` or `RolePermissionService`.

Use it later to hide/show navigation items.

## Step 12: Update Blazor Auth State

Update Blazor auth state/token service to also store permission codes after login or after calling `/api/Auth/Permissions`.

Minimum behavior:

- After login, fetch permissions.
- Store them in memory.
- Expose helper:

```csharp
bool HasPermission(string permissionCode)
```

## Step 13: Update Blazor Navigation Visibility

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Show/hide links based on permissions:

```text
Cards -> Card.View
Packages -> Package.View
TopUp -> TopUp.View or TopUp.Create
Transactions -> Transaction.View
Roles/Permissions -> RolePermission.View
AuditLog -> AuditLog.View later
```

This is UX only. API enforcement is the real security boundary.

## Step 14: Standardize 401 vs 403 Behavior

Expected:

```text
No token or invalid token -> 401 Unauthorized
Valid token without permission -> 403 Forbidden
```

Do not convert forbidden responses into generic 400 responses.

## Do Not Do In Phase 9

- Do not implement AuditLog writing.
- Do not redesign RolePermission management.
- Do not store permissions only in JWT as the source of truth.
- Do not trust Blazor navigation hiding as security.
- Do not allow unknown permission codes to pass.
- Do not remove JWT authentication.
- Do not introduce repositories.
- Do not change existing business logic except adding authorization attributes.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run:

```powershell
rg "RequirePermission"
rg "PermissionChecker"
rg "PermissionPolicyProvider"
rg "IAuthorizationPolicyProvider"
rg "UseAuthentication"
```

Manual API tests:

```http
GET /api/Card
```

Expected:

- No token returns 401.
- Token without `Card.View` returns 403.
- Token with `Card.View` returns 200.

Test write permission:

```http
POST /api/Card
```

Expected:

- Token with only `Card.View` returns 403.
- Token with `Card.Create` succeeds if request is valid.

Test RolePermission:

```http
PUT /api/RolePermission/Roles/{roleId}/Permissions
```

Expected:

- Token without `RolePermission.Manage` returns 403.
- Token with `RolePermission.Manage` succeeds.

## Expected Result

- API endpoints are protected by database-backed permissions.
- Authenticated users can access only permitted operations.
- Role/permission changes affect authorization without code changes.
- Blazor can hide navigation items based on current permissions.
- Project is ready for Phase 10: AuditLog.

## Git Milestone

```text
feat: enforce dynamic rbac permissions
```
