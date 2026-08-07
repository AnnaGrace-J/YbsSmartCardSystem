# Phase 3 Implementation Plan: Clean Current Domain Services

## Goal

Standardize and harden the existing Domain feature services without changing business behavior.

This phase keeps the current maintenance architecture:

- Domain services stay in `YbsSmartCardSystem.Domain`
- Domain services may continue using `AppDbContext`
- EF Core models stay in `YbsSmartCardSystem.Database`
- Request/response models should already be in `YbsSmartCardSystem.Contracts` from Phase 2

## Scope

Clean the existing implemented features:

```text
Card
TopUp
Bus
Terminal
Transaction
```

Do not add Package, Auth, RBAC, or AuditLog yet.

## Main Tasks

### 1. Move Result To Domain/Common

Move:

```text
YbsSmartCardSystem.Domain/Result.cs
```

to:

```text
YbsSmartCardSystem.Domain/Common/Result.cs
```

Update namespace to:

```csharp
namespace YbsSmartCardSystem.Domain.Common;
```

Update all usages in API, App, and Domain.

Do not move `Result<T>` to Contracts yet unless Phase 2 already made that decision.

### 2. Add Pagination Model

Create:

```text
YbsSmartCardSystem.Domain/Common/PaginationModel.cs
```

Suggested model:

```csharp
namespace YbsSmartCardSystem.Domain.Common;

public class PaginationModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
```

Use this only where it fits naturally. Do not force a large refactor if existing contract request models already define pagination properties.

### 3. Standardize Result Status Codes

Review all Domain services and make failure status codes consistent:

```text
400 = validation error
404 = entity not found
409 = conflict or business rule violation
500 = unexpected exception
```

For successful operations, use:

```text
200 = successful read/update/delete
201 = successful create, optional if existing API behavior should remain 200
```

Keep response shape unchanged.

### 4. Stop Returning Raw Exceptions

Replace user-facing `ex.ToString()` responses with safe messages.

Bad:

```csharp
Message = ex.ToString()
```

Better:

```csharp
StatusCode = 500,
Message = "An unexpected error occurred."
```

Do not swallow exceptions silently. If logging is not available yet, leave a TODO comment only where useful:

```csharp
// TODO: Log exception after Infrastructure logging is added.
```

Use comments sparingly.

### 5. Normalize DateTime Usage

Review all `DateTime.Now` usage.

For now, keep the current behavior unless the project already expects UTC.

If changing to UTC, do it consistently across all services:

```csharp
DateTime.UtcNow
```

Do not mix `DateTime.Now` and `DateTime.UtcNow` in the same workflow.

Recommended for this maintenance phase: keep `DateTime.Now` to avoid unexpected behavior changes, but document this as a future deployment decision.

### 6. Standardize Validation

For each service method:

- Check `request is null`
- Validate required IDs are greater than 0
- Validate required strings are not empty
- Trim strings before saving or comparing
- Validate max lengths based on database column sizes
- Validate `PageNo > 0`
- Validate `PageSize > 0`
- Cap `PageSize` at a reasonable maximum, such as 100

Apply this to:

```text
CardService
TopUpService
BusService
TerminalService
TransactionService
```

### 7. Standardize Soft Delete Behavior

Ensure all list/get/update/delete queries exclude deleted records:

```csharp
x.DeleteFlag == false
```

For delete operations, use soft delete:

```csharp
item.DeleteFlag = true;
```

Do not physically remove rows.

### 8. Review AsNoTracking Usage

Use `AsNoTracking()` for read-only queries.

Do not use `AsNoTracking()` when the entity will be modified unless the service intentionally re-attaches it.

Preferred update/delete pattern:

```csharp
var item = _db.TblCards.FirstOrDefault(...);
if (item is null) ...

item.OwnerName = ...;
item.UpdatedDate = DateTime.Now;
_db.SaveChanges();
```

Avoid unnecessary:

```csharp
_db.Entry(item).State = EntityState.Modified;
```

when the entity is already tracked.

### 9. Standardize Transactions

Keep explicit database transactions for workflows that update multiple tables:

```text
TopUp creates TblTopUp and updates card balance
Transaction creates TblTransaction and deducts card balance
```

Ensure rollback happens on failure.

Do not add transactions to simple single-table CRUD unless needed.

### 10. Clean Naming Consistency

Use consistent feature naming in code:

```text
TopUp or Topup
BusPayment or Bus/Terminal/Transaction
```

For Phase 3, avoid route-breaking renames.

Recommended:

- Keep existing public routes and class names.
- Internally document that `Bus`, `Terminal`, and `Transaction` currently represent the BusPayment workflow.
- Rename later only if there is a dedicated feature alignment phase.

### 11. Remove Unused Usings

Remove unused `using` statements from services and controllers after cleanup.

### 12. Check API BaseController

Ensure `BaseController.Execute<T>()` handles missing or invalid status codes safely.

Recommended behavior:

```text
If result.IsSuccess is true, return Ok(result).
If result.StatusCode is less than 400, return BadRequest(result).
Otherwise return StatusCode(result.StatusCode, result).
```

This prevents accidental failed responses returning HTTP 200.

## Files To Review

```text
YbsSmartCardSystem.Domain/Result.cs
YbsSmartCardSystem.Domain/Features/Card/CardService.cs
YbsSmartCardSystem.Domain/Features/TopUp/TopUpService.cs
YbsSmartCardSystem.Domain/Features/Bus/BusService.cs
YbsSmartCardSystem.Domain/Features/Terminal/TerminalService.cs
YbsSmartCardSystem.Domain/Features/Transaction/TransactionService.cs
YbsSmartCardSystem.Api/Controllers/BaseController.cs
YbsSmartCardSystem.Api/Controllers/CardController.cs
YbsSmartCardSystem.Api/Controllers/TopUpController.cs
YbsSmartCardSystem.Api/Controllers/BusController.cs
YbsSmartCardSystem.Api/Controllers/TerminalController.cs
YbsSmartCardSystem.Api/Controllers/TransactionController.cs
```

## Do Not Do In Phase 3

- Do not add new database tables.
- Do not scaffold EF Core models.
- Do not add Package.
- Do not add Auth.
- Do not add JWT.
- Do not add Dynamic RBAC.
- Do not add AuditLog.
- Do not redesign architecture.
- Do not introduce repositories.
- Do not remove `Domain -> Database`.
- Do not change API routes.
- Do not change Blazor UI behavior unless required by namespace changes.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run static checks:

```powershell
rg "ex\.ToString\(\)"
rg "StatusCode = 200" YbsSmartCardSystem.Domain
rg "AsNoTracking\(\).*EntityState.Modified" YbsSmartCardSystem.Domain
```

Expected results:

- No user-facing `ex.ToString()` remains.
- Failed results use appropriate status codes.
- Read queries use `AsNoTracking()`.
- Update/delete queries use tracked entities where practical.
- Existing routes and response shapes remain stable.

## Expected Result

- Current services are cleaner and more consistent.
- Error responses are safer.
- Validation behavior is more predictable.
- Soft delete behavior is consistent.
- Existing business workflows remain unchanged.
- The project is ready for Phase 4: database schema update.

## Git Milestone

```text
chore: clean current domain services
```
