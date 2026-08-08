# Phase 8 Implementation Plan: Add Role and Permission Management

## Goal

Implement Role and Permission management for Dynamic RBAC.

This phase adds the ability to view/manage:

- Roles
- Permissions
- User role assignments
- Role permission assignments

Permission enforcement on protected API endpoints is handled in Phase 9.

## Scope

Add RolePermission support across:

- Contracts
- Domain
- API
- Blazor App

Use the existing maintenance architecture:

- Domain contains business workflows
- Database contains scaffolded RBAC tables
- API controllers delegate to Domain services
- Blazor calls API through `ApiService`

## Prerequisites

Phase 7 must be complete.

Confirm these generated files exist:

```text
YbsSmartCardSystem.Database/AppDbContextModels/TblUser.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblRole.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblPermission.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblUserRole.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblRolePermission.cs
```

Confirm login works and JWT authentication is configured.

## Step 1: Add RolePermission Contracts

Create:

```text
YbsSmartCardSystem.Contracts/Features/RolePermission/RolePermissionModels.cs
```

Suggested models:

```csharp
namespace YbsSmartCardSystem.Contracts.Features.RolePermission;

public class RoleListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class RoleListResponseModel
{
    public int TotalCount { get; set; }
    public List<RoleModel> Roles { get; set; } = [];
}

public class RoleModel
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
}

public class RoleCreateRequestModel
{
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class RolePatchRequestModel
{
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? FeatureName { get; set; }
    public bool? IsActive { get; set; }
}

public class PermissionListResponseModel
{
    public int TotalCount { get; set; }
    public List<PermissionModel> Permissions { get; set; } = [];
}

public class PermissionModel
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string FeatureName { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class UserRoleUpdateRequestModel
{
    public int UserId { get; set; }
    public List<int> RoleIds { get; set; } = [];
}

public class RolePermissionUpdateRequestModel
{
    public int RoleId { get; set; }
    public List<int> PermissionIds { get; set; } = [];
}

public class UserRoleResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public List<RoleModel> Roles { get; set; } = [];
}

public class RolePermissionResponseModel
{
    public int RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public List<PermissionModel> Permissions { get; set; } = [];
}
```

Add create/patch permission models only if permissions must be user-editable. Recommended for this phase: seed permissions from database/scripts and manage assignments only.

## Step 2: Add RolePermission Domain Service

Create:

```text
YbsSmartCardSystem.Domain/Features/RolePermission/RolePermissionService.cs
```

Inject:

```csharp
AppDbContext
```

Use:

```csharp
YbsSmartCardSystem.Contracts.Features.RolePermission
```

Required methods:

```csharp
Result<RoleListResponseModel> GetRoles(RoleListRequestModel request)
Result<RoleModel> GetRoleById(int roleId)
Result<RoleModel> CreateRole(RoleCreateRequestModel request)
Result<RoleModel> PatchRole(int roleId, RolePatchRequestModel request)
Result<RoleModel> DeleteRole(int roleId)

Result<PermissionListResponseModel> GetPermissions(PermissionListRequestModel request)
Result<UserRoleResponseModel> GetUserRoles(int userId)
Result<UserRoleResponseModel> UpdateUserRoles(UserRoleUpdateRequestModel request)
Result<RolePermissionResponseModel> GetRolePermissions(int roleId)
Result<RolePermissionResponseModel> UpdateRolePermissions(RolePermissionUpdateRequestModel request)
```

## Step 3: Role Validation Rules

For role list:

- Default request if null
- `PageNo > 0`
- `PageSize > 0`
- `PageSize <= 100`
- Optional search by `RoleCode` or `RoleName`
- Optional `IsActive` filter

For role create:

- `RoleCode` required, max 50
- `RoleName` required, max 100
- `Description` max 250
- Duplicate active `RoleCode` returns 409
- Set `IsSystemRole = false`
- Set `CreatedDate`
- Set `DeleteFlag = false`

For role patch:

- Role must exist and not be deleted
- Do not allow editing `IsSystemRole` through public patch
- If `IsSystemRole = true`, block delete and optionally block code/name changes
- Duplicate active `RoleCode` returns 409
- Set `UpdatedDate`

For role delete:

- Role must exist and not be deleted
- If `IsSystemRole = true`, return 409
- Soft delete role
- Soft delete related active `Tbl_UserRole` and `Tbl_RolePermission` rows

## Step 4: Permission Rules

For permissions:

- List/search only is enough for this phase
- Exclude deleted records
- Optional `FeatureName` filter
- Optional `IsActive` filter
- Use `AsNoTracking()`

Do not let users delete core permissions from UI unless explicitly required.

Permissions should generally be created by seed scripts or admin maintenance scripts because endpoint enforcement depends on stable permission codes.

## Step 5: User Role Assignment Rules

For `UpdateUserRoles`:

- `UserId > 0`
- User must exist, active, and not deleted
- Role IDs must exist, active, and not deleted
- Replace current active role assignments with the submitted list
- Prefer soft delete old assignments instead of physical delete
- Avoid duplicate active assignments
- Use a database transaction

Recommended behavior:

1. Load current active user roles.
2. Soft delete assignments not in new list.
3. Add missing assignments.
4. Save changes.

## Step 6: Role Permission Assignment Rules

For `UpdateRolePermissions`:

- `RoleId > 0`
- Role must exist, active, and not deleted
- Permission IDs must exist, active, and not deleted
- Replace current active permission assignments with the submitted list
- Prefer soft delete old assignments instead of physical delete
- Avoid duplicate active assignments
- Use a database transaction

If role is a system role, allow permission changes only if this is acceptable for the project. Recommended:

- Allow Admin role permission updates only carefully
- Block deleting Admin role
- Do not block assigning permissions to Admin

## Step 7: Add RolePermission API Controller

Create:

```text
YbsSmartCardSystem.Api/Controllers/RolePermissionController.cs
```

Use:

```csharp
[Route("api/[controller]")]
```

Endpoints:

```text
GET    /api/RolePermission/Roles
GET    /api/RolePermission/Roles/{roleId}
POST   /api/RolePermission/Roles
PATCH  /api/RolePermission/Roles/{roleId}
DELETE /api/RolePermission/Roles/{roleId}

GET    /api/RolePermission/Permissions

GET    /api/RolePermission/Users/{userId}/Roles
PUT    /api/RolePermission/Users/{userId}/Roles

GET    /api/RolePermission/Roles/{roleId}/Permissions
PUT    /api/RolePermission/Roles/{roleId}/Permissions
```

For PUT assignment endpoints, validate route ID matches request ID. If not, return 400.

For this phase:

- Require authentication with `[Authorize]`
- Do not add Dynamic RBAC policy checks yet

## Step 8: Register RolePermissionService

Update:

```text
YbsSmartCardSystem.Api/Program.cs
```

Add:

```csharp
builder.Services.AddScoped<RolePermissionService>();
```

## Step 9: Update Blazor ApiService

Update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Add methods:

```csharp
Task<Result<RoleListResponseModel>> GetRoles(RoleListRequestModel request)
Task<Result<RoleModel>> GetRoleById(int roleId)
Task<Result<RoleModel>> RoleCreate(RoleCreateRequestModel request)
Task<Result<RoleModel>> RolePatch(int roleId, RolePatchRequestModel request)
Task<Result<RoleModel>> RoleDelete(int roleId)

Task<Result<PermissionListResponseModel>> GetPermissions(PermissionListRequestModel request)
Task<Result<UserRoleResponseModel>> GetUserRoles(int userId)
Task<Result<UserRoleResponseModel>> UpdateUserRoles(UserRoleUpdateRequestModel request)
Task<Result<RolePermissionResponseModel>> GetRolePermissions(int roleId)
Task<Result<RolePermissionResponseModel>> UpdateRolePermissions(RolePermissionUpdateRequestModel request)
```

Add endpoints under `ApiEndpoints`.

Attach JWT bearer token if Phase 7 added token storage.

## Step 10: Add Blazor RolePermission Pages

Create:

```text
YbsSmartCardSystem.App/Components/Features/RolePermission
```

Suggested pages:

```text
RoleList.razor
RoleList.razor.cs
RoleCreate.razor
RoleCreate.razor.cs
RolePermissionManage.razor
RolePermissionManage.razor.cs
UserRoleManage.razor
UserRoleManage.razor.cs
PermissionList.razor
PermissionList.razor.cs
```

Minimum UI:

- List roles
- Create/edit/delete non-system roles
- List permissions
- Assign permissions to a role
- Assign roles to a user
- Show API success/error messages

Keep UI consistent with existing Blazor style. Tailwind conversion can happen later.

## Step 11: Update Navigation

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Add:

```text
Roles
Permissions
```

Role-aware hiding comes after Dynamic RBAC enforcement is implemented.

## Do Not Do In Phase 8

- Do not implement Dynamic RBAC endpoint enforcement yet.
- Do not add permission attributes/policies to every existing controller.
- Do not implement AuditLog yet.
- Do not change JWT login behavior except where needed for authenticated calls.
- Do not change existing Card/Package/TopUp/Bus/Transaction business behavior.
- Do not physically delete role assignment records if soft delete is available.
- Do not allow deleting system roles.
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
rg "RolePermissionService"
rg "RolePermissionController"
rg "RoleListRequestModel"
rg "RolePermissionUpdateRequestModel"
```

Manual API tests:

```http
GET /api/RolePermission/Roles
GET /api/RolePermission/Permissions
POST /api/RolePermission/Roles
PUT /api/RolePermission/Roles/{roleId}/Permissions
PUT /api/RolePermission/Users/{userId}/Roles
```

Expected:

- Unauthenticated requests return 401.
- Authenticated requests can list roles and permissions.
- Duplicate role codes return 409.
- System roles cannot be deleted.
- Role permission assignments update correctly.
- User role assignments update correctly.

## Expected Result

- Roles can be managed.
- Permissions can be viewed.
- Users can be assigned roles.
- Roles can be assigned permissions.
- Authenticated admins can manage RBAC data.
- Project is ready for Phase 9: Dynamic RBAC enforcement.

## Git Milestone

```text
feat: add role permission management
```
