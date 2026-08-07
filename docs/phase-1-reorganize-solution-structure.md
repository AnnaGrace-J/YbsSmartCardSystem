# Phase 1 Implementation Plan: Reorganize Solution Structure

## Goal

Add the missing support projects and wire project references only.

Do not move business code yet. Do not change runtime behavior.

## Scope

This phase prepares the solution for later maintenance work while keeping the existing project style:

- `YbsSmartCardSystem.Api`
- `YbsSmartCardSystem.App`
- `YbsSmartCardSystem.Domain`
- `YbsSmartCardSystem.Database`
- `YbsSmartCardSystem.Contracts`
- `YbsSmartCardSystem.Infrastructure`
- `YbsSmartCardSystem.Shared`

## Steps

### 1. Create New Projects

Run from the solution root:

```powershell
dotnet new classlib -n YbsSmartCardSystem.Contracts
dotnet new classlib -n YbsSmartCardSystem.Infrastructure
dotnet new classlib -n YbsSmartCardSystem.Shared
```

Remove the default `Class1.cs` file from each new project.

### 2. Add Projects To Solution

Add the new projects to `YbsSmartCardSystem.slnx`.

The solution should contain:

```text
YbsSmartCardSystem.Api
YbsSmartCardSystem.App
YbsSmartCardSystem.Domain
YbsSmartCardSystem.Database
YbsSmartCardSystem.Contracts
YbsSmartCardSystem.Infrastructure
YbsSmartCardSystem.Shared
```

### 3. Confirm Project Settings

Each new `.csproj` should use:

```xml
<PropertyGroup>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

### 4. Add Project References

`YbsSmartCardSystem.Api` should reference:

```text
YbsSmartCardSystem.Domain
YbsSmartCardSystem.Database
YbsSmartCardSystem.Infrastructure
YbsSmartCardSystem.Contracts
YbsSmartCardSystem.Shared
```

`YbsSmartCardSystem.App` should reference:

```text
YbsSmartCardSystem.Domain
YbsSmartCardSystem.Contracts
YbsSmartCardSystem.Shared
```

Keep the current `App -> Domain` reference for now because existing Blazor code still uses Domain models. Remove it later in Phase 2 after contracts are extracted.

`YbsSmartCardSystem.Domain` should reference:

```text
YbsSmartCardSystem.Database
YbsSmartCardSystem.Contracts
YbsSmartCardSystem.Shared
```

`YbsSmartCardSystem.Database` should reference:

```text
YbsSmartCardSystem.Shared
```

`YbsSmartCardSystem.Infrastructure` should reference:

```text
YbsSmartCardSystem.Database
YbsSmartCardSystem.Contracts
YbsSmartCardSystem.Shared
```

`YbsSmartCardSystem.Contracts` should reference:

```text
YbsSmartCardSystem.Shared
```

`YbsSmartCardSystem.Shared` should have no project references.

### 5. Create Folder Structure

Create these folders in `YbsSmartCardSystem.Contracts`:

```text
Features/Card
Features/Package
Features/TopUp
Features/BusPayment
Features/Transaction
Features/Auth
Features/RolePermission
Features/AuditLog
Common
```

Create these folders in `YbsSmartCardSystem.Infrastructure`:

```text
Authentication
Authorization/DynamicRbac
Logging
AuditLog
HttpClients
Services
Extensions
```

Create these folders in `YbsSmartCardSystem.Shared`:

```text
Constants
Enums
Extensions
Helpers
```

Create these folders in `YbsSmartCardSystem.Api`:

```text
Middlewares
Filters
Extensions
```

Add `.gitkeep` files to empty folders if needed.

## Do Not Do In Phase 1

- Do not move `Result.cs`.
- Do not move request or response models.
- Do not modify Domain services.
- Do not modify API controllers.
- Do not modify Blazor pages.
- Do not modify scaffolded database files.
- Do not add Auth, RBAC, or AuditLog implementation yet.
- Do not introduce a repository layer.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact restore error and still verify that the solution and project references are structurally correct.

## Expected Result

- The solution contains all 7 projects.
- Existing business behavior is unchanged.
- Existing code remains in place.
- The project is ready for Phase 2: extracting contracts.

## Git Milestone

```text
chore: reorganize solution structure
```
