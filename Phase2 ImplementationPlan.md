# Phase 2 Implementation Plan: Extract Contracts

## Goal

Move API request and response models out of `YbsSmartCardSystem.Domain` and into `YbsSmartCardSystem.Contracts`.

This phase reduces coupling between the Blazor app and Domain services while keeping business logic unchanged.

## Scope

This phase affects model classes only.

Move endpoint-facing models from:

```text
YbsSmartCardSystem.Domain/Features/*/Models
```

to:

```text
YbsSmartCardSystem.Contracts/Features/*
```

Keep Domain services in `YbsSmartCardSystem.Domain`.

## Current Model Locations

Existing model files include:

```text
YbsSmartCardSystem.Domain/Features/Card/Models/CardList.cs
YbsSmartCardSystem.Domain/Features/TopUp/Models/TopUp.cs
YbsSmartCardSystem.Domain/Features/Bus/Models/BusModels.cs
YbsSmartCardSystem.Domain/Features/Terminal/Models/TerminalModels.cs
YbsSmartCardSystem.Domain/Features/Transaction/Models/TransactionModels.cs
```

## Target Model Locations

Move them to:

```text
YbsSmartCardSystem.Contracts/Features/Card/CardModels.cs
YbsSmartCardSystem.Contracts/Features/TopUp/TopUpModels.cs
YbsSmartCardSystem.Contracts/Features/BusPayment/BusModels.cs
YbsSmartCardSystem.Contracts/Features/BusPayment/TerminalModels.cs
YbsSmartCardSystem.Contracts/Features/Transaction/TransactionModels.cs
```

Note: the current code has separate `Bus` and `Terminal` features. The maintenance plan calls the business workflow `BusPayment`. For Phase 2, only move models. Do not rename controllers/services yet unless required by compile errors.

## Steps

### 1. Move Model Files

Move each request/response/model file from Domain to Contracts.

Recommended mapping:

```text
Domain/Features/Card/Models/CardList.cs
  -> Contracts/Features/Card/CardModels.cs

Domain/Features/TopUp/Models/TopUp.cs
  -> Contracts/Features/TopUp/TopUpModels.cs

Domain/Features/Bus/Models/BusModels.cs
  -> Contracts/Features/BusPayment/BusModels.cs

Domain/Features/Terminal/Models/TerminalModels.cs
  -> Contracts/Features/BusPayment/TerminalModels.cs

Domain/Features/Transaction/Models/TransactionModels.cs
  -> Contracts/Features/Transaction/TransactionModels.cs
```

### 2. Update Namespaces

Update namespaces to match the new Contracts project.

Examples:

```csharp
namespace YbsSmartCardSystem.Contracts.Features.Card;
```

```csharp
namespace YbsSmartCardSystem.Contracts.Features.TopUp;
```

```csharp
namespace YbsSmartCardSystem.Contracts.Features.BusPayment;
```

```csharp
namespace YbsSmartCardSystem.Contracts.Features.Transaction;
```

Use one namespace per feature folder.

### 3. Update Domain Service Usings

Update Domain services to import models from Contracts.

Examples:

```csharp
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Contracts.Features.Transaction;
```

Do not move the services themselves.

### 4. Update API Controller Usings

Update controllers to use contract namespaces instead of Domain model namespaces.

Examples:

```csharp
using YbsSmartCardSystem.Contracts.Features.Card;
using YbsSmartCardSystem.Contracts.Features.TopUp;
using YbsSmartCardSystem.Contracts.Features.BusPayment;
using YbsSmartCardSystem.Contracts.Features.Transaction;
```

Controllers should still call Domain services.

### 5. Update Blazor App Usings

Update Blazor services and pages to use Contracts models.

Main file to update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Also update any `.razor` or `.razor.cs` files that import:

```csharp
YbsSmartCardSystem.Domain.Features.*.Models
```

to the matching Contracts namespace.

### 6. Keep Result.cs For Now

Do not move `Result.cs` in this phase.

The app currently consumes:

```csharp
YbsSmartCardSystem.Domain.Result<T>
```

Keep this temporarily to avoid a larger behavioral change.

Move or duplicate API-facing `Result<T>` into `Contracts/Common` in a later cleanup phase.

### 7. Remove Empty Domain Model Folders

After model files are moved and references compile, remove empty `Models` folders from Domain, or keep them with `.gitkeep` only if desired.

Do not remove feature service folders.

## Do Not Do In Phase 2

- Do not move Domain services.
- Do not change controller routes.
- Do not change API response shapes.
- Do not modify database scaffolded files.
- Do not add Package, Auth, RBAC, or AuditLog yet.
- Do not remove `App -> Domain` reference yet because `Result<T>` is still in Domain.
- Do not introduce repositories.
- Do not rename public endpoints.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If restore fails because NuGet is unavailable, record the exact error.

Also run:

```powershell
rg "YbsSmartCardSystem.Domain.Features.*.Models"
```

Expected result: no remaining imports from Domain model namespaces.

Run:

```powershell
rg "YbsSmartCardSystem.Contracts.Features"
```

Expected result: Domain services, API controllers, and Blazor pages/services now reference Contracts feature models.

## Expected Result

- Request and response models live in `YbsSmartCardSystem.Contracts`.
- Domain services still contain business workflows.
- API controllers still call Domain services.
- Blazor app uses shared contract models for API communication.
- Existing runtime behavior remains unchanged.

## Git Milestone

```text
feat: add feature contracts
```
