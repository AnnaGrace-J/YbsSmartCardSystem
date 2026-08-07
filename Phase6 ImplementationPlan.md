# Phase 6 Implementation Plan: Add Package Feature

## Goal

Implement Package management across Contracts, Domain, API, and Blazor.

This phase adds the first new business feature after the database schema and EF Core models are ready.

## Scope

Add Package support for:

- Create package
- List packages with pagination/search
- Get package by ID
- Patch/update package
- Soft delete package

Use the existing project style:

- Domain service contains business workflow logic
- API controller delegates to Domain service
- Blazor calls API through `ApiService`
- EF Core database-first models stay in `YbsSmartCardSystem.Database`

## Prerequisites

Phase 4 and Phase 5 must be complete.

Confirm these exist:

```text
YbsSmartCardSystem.Database/AppDbContextModels/TblPackage.cs
```

and `AppDbContext` has a DbSet similar to:

```csharp
public virtual DbSet<TblPackage> TblPackages { get; set; }
```

Use the exact generated DbSet name.

## Step 1: Add Package Contracts

Create:

```text
YbsSmartCardSystem.Contracts/Features/Package/PackageModels.cs
```

Suggested contracts:

```csharp
namespace YbsSmartCardSystem.Contracts.Features.Package;

public class PackageListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}

public class PackageListResponseModel
{
    public int TotalCount { get; set; }
    public List<PackageModel> Packages { get; set; } = [];
}

public class PackageModel
{
    public int PackageId { get; set; }
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

public class PackageCreateRequestModel
{
    public string PackageCode { get; set; } = string.Empty;
    public string PackageName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class PackageCreateResponseModel : PackageModel
{
}

public class PackagePatchRequestModel
{
    public string? PackageCode { get; set; }
    public string? PackageName { get; set; }
    public decimal? Price { get; set; }
    public int? RideLimit { get; set; }
    public int? ValidDays { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
```

Adjust property names only if the scaffolded database columns differ.

## Step 2: Add Package Domain Service

Create:

```text
YbsSmartCardSystem.Domain/Features/Package/PackageService.cs
```

The service should:

- Inject `AppDbContext`
- Return `Result<T>`
- Use contract models from `YbsSmartCardSystem.Contracts.Features.Package`
- Exclude soft-deleted records
- Use `AsNoTracking()` for read-only queries
- Use tracked entities for update/delete

Required methods:

```csharp
Result<PackageListResponseModel> GetList(PackageListRequestModel request)
Result<PackageModel> GetById(int id)
Result<PackageCreateResponseModel> Create(PackageCreateRequestModel request)
Result<PackageModel> Patch(int id, PackagePatchRequestModel request)
Result<PackageModel> Delete(int id)
```

## Step 3: Package Validation Rules

Apply these rules:

List:

- Default request if null
- `PageNo > 0`
- `PageSize > 0`
- `PageSize <= 100`
- Optional search by `PackageCode` or `PackageName`
- Optional filter by `IsActive`

Create:

- `PackageCode` required, max 50
- `PackageName` required, max 100
- `Price > 0`
- `RideLimit` must be null or greater than 0
- `ValidDays` must be null or greater than 0
- `Description` max 250
- Duplicate active `PackageCode` should return conflict
- Set `CreatedDate`
- Set `DeleteFlag = false`

Patch:

- ID required
- Request required
- At least one field must be supplied
- Same validation as create for supplied fields
- Duplicate active `PackageCode` should return conflict
- Set `UpdatedDate`

Delete:

- ID required
- Must exist and not be deleted
- Set `DeleteFlag = true`
- Set `UpdatedDate`

Recommended status codes:

```text
400 validation
404 not found
409 duplicate/conflict
500 unexpected
```

## Step 4: Add Package API Controller

Create:

```text
YbsSmartCardSystem.Api/Controllers/PackageController.cs
```

Use route:

```csharp
[Route("api/[controller]")]
```

Endpoints:

```text
GET    /api/Package
GET    /api/Package/{id}
POST   /api/Package
PATCH  /api/Package/{id}
DELETE /api/Package/{id}
```

Controller should:

- Inject `PackageService`
- Use contract request models
- Delegate to service
- Return through `BaseController.Execute(result)`

Do not add authorization yet. RBAC comes later.

## Step 5: Register PackageService

Update:

```text
YbsSmartCardSystem.Api/Program.cs
```

Add:

```csharp
builder.Services.AddScoped<PackageService>();
```

Keep current registration style. DI extension cleanup can happen later.

## Step 6: Update Blazor ApiService

Update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Add Package API methods:

```csharp
Task<Result<PackageListResponseModel>> GetPackages(PackageListRequestModel request)
Task<Result<PackageCreateResponseModel>> PackageCreate(PackageCreateRequestModel request)
Task<Result<PackageModel>> PackagePatch(int id, PackagePatchRequestModel request)
Task<Result<PackageModel>> PackageDelete(int id)
```

Add endpoints:

```csharp
public const string PackageList = "api/Package";
public const string CreatePackage = "api/Package";
public static string PackageDetail(int packageId) => $"api/Package/{packageId}";
```

Use the same error-handling style as existing API methods, but set `StatusCode` consistently when available.

## Step 7: Add Blazor Package Pages

Create folder:

```text
YbsSmartCardSystem.App/Components/Features/Package
```

Add pages:

```text
PackageList.razor
PackageList.razor.cs
PackageCreate.razor
PackageCreate.razor.cs
```

Minimum UI requirements:

- List packages
- Search by code/name
- Pagination
- Create package form
- Edit package inline or via simple form/modal/page
- Soft delete package
- Show success/error messages

Follow existing Blazor feature style for now.

Do not redesign the whole UI in this phase.

## Step 8: Update Navigation

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Add Package link:

```text
Package List
```

Suggested route:

```text
packages
```

## Step 9: Manual API Smoke Tests

After build succeeds and API runs, test:

```http
GET /api/Package
POST /api/Package
GET /api/Package/{id}
PATCH /api/Package/{id}
DELETE /api/Package/{id}
```

Check:

- Duplicate code returns 409
- Missing required fields return 400
- Deleted packages no longer appear in list
- Deleted package cannot be fetched by ID

## Do Not Do In Phase 6

- Do not add JWT/Auth.
- Do not add RBAC permission checks.
- Do not add AuditLog writing.
- Do not change database schema unless `Tbl_Package` is missing or incorrect.
- Do not re-scaffold unless the database model is missing.
- Do not introduce repositories.
- Do not redesign all Blazor pages.
- Do not rename existing routes.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run:

```powershell
rg "PackageService"
rg "PackageController"
rg "PackageListRequestModel"
rg "api/Package"
```

Expected:

- Package contracts exist.
- Package service exists.
- Package controller exists.
- Package API methods exist in Blazor `ApiService`.
- Package navigation exists.

## Expected Result

- Package management works end to end.
- Existing features remain unchanged.
- Package uses Contracts for request/response models.
- Package business workflow is implemented in Domain.
- Project is ready for Phase 7: Authentication.

## Git Milestone

```text
feat: implement package feature
```
