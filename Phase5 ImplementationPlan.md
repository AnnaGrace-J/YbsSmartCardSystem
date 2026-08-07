# Phase 5 Implementation Plan: Scaffold EF Core Database First Models

## Goal

Regenerate EF Core models from the updated SQL Server database schema.

This phase syncs the C# database model layer with the SQL Server schema created in Phase 4.

## Scope

Regenerate files under:

```text
YbsSmartCardSystem.Database/AppDbContextModels
```

Expected generated models should include existing and new tables:

```text
TblCard
TblBus
TblTerminal
TblTopUp
TblTransaction
TblPackage
TblUser
TblRole
TblPermission
TblUserRole
TblRolePermission
TblAuditLog
AppDbContext
```

## Important Rules

- SQL Server remains the source of truth.
- Do not manually edit generated files.
- Do not change business services in this phase unless required only to fix compile errors from regenerated names.
- Do not add Package/Auth/RBAC/AuditLog logic yet.
- Preserve custom code by keeping it outside generated files.

## Pre-Work

### 1. Confirm Phase 4 Database Script Was Applied

Verify these tables exist in the development database:

```sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN
(
    'Tbl_Package',
    'Tbl_User',
    'Tbl_Role',
    'Tbl_Permission',
    'Tbl_UserRole',
    'Tbl_RolePermission',
    'Tbl_AuditLog'
);
```

### 2. Back Up Current Scaffolded Files

Before regenerating, copy the current scaffolded folder to a temporary backup location outside the generated folder.

Example:

```text
YbsSmartCardSystem.Database/AppDbContextModels_Backup_PrePhase5
```

Use the backup only for comparison. Do not keep it as part of the final committed code unless intentionally documented.

### 3. Check Connection String

Use a development database connection string.

Do not commit real production credentials.

Recommended local source:

```text
YbsSmartCardSystem.Api/appsettings.Development.json
```

or user secrets/environment variables.

## Install/Verify EF Tools

Check EF Core CLI availability:

```powershell
dotnet ef --version
```

If unavailable, install or update the local/global EF tool as appropriate:

```powershell
dotnet tool install --global dotnet-ef
```

or:

```powershell
dotnet tool update --global dotnet-ef
```

Use a version compatible with the project EF Core package version.

## Scaffold Command

Run from the solution root.

Recommended command:

```powershell
dotnet ef dbcontext scaffold "Server=.;Database=YbsSmartCard;User Id=sa;Password=sasa@123;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer --project YbsSmartCardSystem.Database --startup-project YbsSmartCardSystem.Api --context AppDbContext --context-dir AppDbContextModels --output-dir AppDbContextModels --force --no-onconfiguring
```

Important:

- Replace the connection string with the correct development connection string.
- Prefer not to commit the raw connection string in scripts if it contains credentials.
- Keep generated files in `YbsSmartCardSystem.Database/AppDbContextModels`.
- Use `--force` only after confirming Phase 4 schema is correct.
- Use `--no-onconfiguring` so credentials are not written into `AppDbContext`.

## Optional Safer Command Pattern

If using an environment variable:

```powershell
$env:YBS_DB_CONNECTION="Server=.;Database=YbsSmartCard;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
dotnet ef dbcontext scaffold $env:YBS_DB_CONNECTION Microsoft.EntityFrameworkCore.SqlServer --project YbsSmartCardSystem.Database --startup-project YbsSmartCardSystem.Api --context AppDbContext --context-dir AppDbContextModels --output-dir AppDbContextModels --force --no-onconfiguring
```

Do not commit the environment variable value.

## Post-Scaffold Review

### 1. Confirm New Files

Verify these files exist:

```text
YbsSmartCardSystem.Database/AppDbContextModels/TblPackage.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblUser.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblRole.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblPermission.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblUserRole.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblRolePermission.cs
YbsSmartCardSystem.Database/AppDbContextModels/TblAuditLog.cs
```

### 2. Check AppDbContext

Verify `AppDbContext` contains DbSets for all expected tables:

```csharp
public virtual DbSet<TblPackage> TblPackages { get; set; }
public virtual DbSet<TblUser> TblUsers { get; set; }
public virtual DbSet<TblRole> TblRoles { get; set; }
public virtual DbSet<TblPermission> TblPermissions { get; set; }
public virtual DbSet<TblUserRole> TblUserRoles { get; set; }
public virtual DbSet<TblRolePermission> TblRolePermissions { get; set; }
public virtual DbSet<TblAuditLog> TblAuditLogs { get; set; }
```

Exact pluralization may vary. Use the generated names consistently.

### 3. Resolve Duplicate Bus Model Issue

The current project contains both:

```text
TblBu.cs
TblBus.cs
```

After scaffolding, verify whether both still exist.

Expected outcome should be one usable model for `Tbl_Bus`.

If EF generates `TblBu` again because of pluralization, either:

- Accept the generated name for now and update code only if needed, or
- Re-run scaffold with naming options if supported and agreed by the team.

Do not manually rename scaffolded generated classes unless the team intentionally stops treating them as generated.

### 4. Check For Connection String Leakage

Search generated files:

```powershell
rg "Password=|User Id=|Server=|ConnectionString|OnConfiguring" YbsSmartCardSystem.Database
```

Expected:

- No database password is generated into C# files.
- No `OnConfiguring` method contains a real connection string.

### 5. Compare Generated Changes

Use Git diff:

```powershell
git diff -- YbsSmartCardSystem.Database/AppDbContextModels
```

Review:

- New table classes
- DbSet names
- Relationship mappings
- Column types
- Nullability
- DeleteFlag fields
- Default values

## Compile Fixes Allowed In This Phase

Only fix compile errors caused directly by scaffolded naming changes.

Examples:

- `TblBus` generated as `TblBu`
- DbSet name changed
- Nullable property type changed

Do not refactor business logic.

## Do Not Do In Phase 5

- Do not modify SQL schema unless scaffolding reveals a schema mistake.
- Do not add Package service.
- Do not add Auth service.
- Do not add JWT service.
- Do not add Dynamic RBAC service.
- Do not add AuditLog writer.
- Do not update Blazor UI.
- Do not manually edit generated entity logic.
- Do not commit database credentials.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Run:

```powershell
rg "Password=|User Id=|Server=|OnConfiguring" YbsSmartCardSystem.Database
```

Expected result:

- No committed credentials in generated files.

Run:

```powershell
rg "TblPackage|TblUser|TblRole|TblPermission|TblAuditLog" YbsSmartCardSystem.Database/AppDbContextModels
```

Expected result:

- New scaffolded models and DbSets are present.

## Expected Result

- EF Core generated models match the updated SQL Server database.
- New Package, Auth, RBAC, and AuditLog tables are represented in C#.
- Existing services still compile or have only minimal naming fixes.
- The project is ready for Phase 6: Package feature implementation.

## Git Milestone

```text
feat: scaffold database first ef core models
```
