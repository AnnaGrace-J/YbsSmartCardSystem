# Implementation Plan: Terminal Feature

## Goal

Implement the Terminal feature after Bus and before Transaction.

The Terminal feature should support:

- List terminals
- Create terminal
- Get terminal by id
- Patch terminal
- Soft delete terminal

Important uniqueness rules:

- `BusNo` can be duplicated.
- `BusLicense` cannot be duplicated.
- `TerminalSerialNo` cannot be duplicated.

Use the existing `Tbl_Terminal` table. No database schema change is required if `Tbl_Terminal.TerminalSerialNo` already has a unique constraint, which the current EF model indicates it does.

## Current Database Shape

Existing EF model:

```csharp
public partial class TblTerminal
{
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = null!;
    public int BusId { get; set; }
    public bool IsActive { get; set; }
    public bool DeleteFlag { get; set; }
    public virtual TblBus Bus { get; set; } = null!;
    public virtual ICollection<TblTransaction> TblTransactions { get; set; } = new List<TblTransaction>();
}
```

Existing relationship:

- `Tbl_Terminal.BusId` references `Tbl_Bus.BusId`.
- A terminal belongs to one bus.
- A bus can have many terminals.

Existing DB constraints from `AppDbContext`:

- `TerminalId` primary key
- `TerminalSerialNo` unique
- `TerminalSerialNo` max length 100
- `IsActive` default value `true`
- Foreign key to `Tbl_Bus`

## Bus Validation Correction

Before continuing, make sure the Bus feature follows the correct uniqueness rule:

- Do not validate duplicate `BusNo`.
- Do validate duplicate `BusLicense`.

If the previous Bus implementation plan or code checks duplicate `BusNo`, replace that check with `BusLicense`.

Recommended duplicate check:

```csharp
var isDuplicateBusLicense = _db.TblBus
    .AsNoTracking()
    .Any(x => x.BusLicense == busLicense && x.DeleteFlag == false);
```

For patch:

```csharp
var isDuplicateBusLicense = _db.TblBus
    .AsNoTracking()
    .Any(x => x.BusLicense == busLicense
        && x.BusId != id
        && x.DeleteFlag == false);
```

Recommended duplicate message:

```text
Bus license already exists.
```

Recommended status:

```http
409 Conflict
```

## Relevant Files

Create or update these files:

- `YbsSmartCardSystem.Domain/Features/Terminal/Models/TerminalModels.cs`
- `YbsSmartCardSystem.Domain/Features/Terminal/TerminalService.cs`
- `YbsSmartCardSystem.Api/Controllers/TerminalController.cs`
- `YbsSmartCardSystem.Api/Program.cs`
- `YbsSmartCardSystem.App/Services/ApiService.cs`
- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor`
- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalCreate.razor`
- `YbsSmartCardSystem.App/Components/Layout/NavMenu.razor`

## Domain Models

Create:

`YbsSmartCardSystem.Domain/Features/Terminal/Models/TerminalModels.cs`

Suggested models:

```csharp
namespace YbsSmartCardSystem.Domain.Features.Terminal.Models;

public class TerminalListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TerminalListResponseModel
{
    public List<TerminalModel> Terminals { get; set; } = new();
}

public class TerminalCreateRequestModel
{
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class TerminalCreateResponseModel
{
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class TerminalPatchRequestModel
{
    public string? TerminalSerialNo { get; set; }
    public int? BusId { get; set; }
    public bool? IsActive { get; set; }
}

public class TerminalModel
{
    public int TerminalId { get; set; }
    public string TerminalSerialNo { get; set; } = string.Empty;
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

## Domain Service

Create:

`YbsSmartCardSystem.Domain/Features/Terminal/TerminalService.cs`

Add methods:

```csharp
public Result<TerminalListResponseModel> GetList(TerminalListRequestModel request)
public Result<TerminalModel> GetById(int id)
public Result<TerminalCreateResponseModel> Create(TerminalCreateRequestModel request)
public Result<TerminalModel> Patch(int id, TerminalPatchRequestModel request)
public Result<TerminalModel> Delete(int id)
```

### List Rules

`GetList` should:

- Return terminals where `DeleteFlag == false`.
- Include bus info using `.Include(x => x.Bus)`.
- Only show terminal rows where the terminal is not deleted.
- Recommended: also exclude deleted buses by requiring `x.Bus.DeleteFlag == false`.
- Order by `TerminalId` descending.
- Support pagination.
- Map to `TerminalModel`.

Suggested query:

```csharp
var terminals = _db.TblTerminals
    .AsNoTracking()
    .Include(x => x.Bus)
    .Where(x => x.DeleteFlag == false && x.Bus.DeleteFlag == false)
    .OrderByDescending(x => x.TerminalId)
    .Skip((request.PageNo - 1) * request.PageSize)
    .Take(request.PageSize)
    .ToList();
```

### Get By Id Rules

`GetById` should:

- Validate `id > 0`.
- Return only terminals where `DeleteFlag == false`.
- Include bus info.
- Return `404` if not found.

### Create Rules

`Create` should validate:

- Request data is required.
- `TerminalSerialNo` is required.
- `TerminalSerialNo` cannot exceed 100 characters.
- `TerminalSerialNo` must be unique among active terminals where `DeleteFlag == false`.
- `BusId` is required.
- Bus must exist where `DeleteFlag == false`.

Recommended duplicate check:

```csharp
var terminalSerialNo = request.TerminalSerialNo.Trim();

var isDuplicateTerminalSerialNo = _db.TblTerminals
    .AsNoTracking()
    .Any(x => x.TerminalSerialNo == terminalSerialNo && x.DeleteFlag == false);
```

Recommended duplicate response:

```csharp
return new Result<TerminalCreateResponseModel>
{
    IsSuccess = false,
    StatusCode = 409,
    Message = "Terminal serial number already exists."
};
```

Recommended save:

```csharp
var terminal = new TblTerminal
{
    TerminalSerialNo = terminalSerialNo,
    BusId = request.BusId,
    IsActive = request.IsActive,
    DeleteFlag = false
};
```

### Patch Rules

`Patch` should validate:

- `id > 0`.
- Request data is required.
- At least one field must be supplied.
- Target terminal must exist and `DeleteFlag == false`.
- If `TerminalSerialNo` is supplied:
  - cannot be empty,
  - cannot exceed 100 characters,
  - cannot duplicate another active terminal.
- If `BusId` is supplied:
  - must be greater than 0,
  - bus must exist and `DeleteFlag == false`.
- If `IsActive` is supplied, update it.

Duplicate patch check must exclude the current terminal:

```csharp
var isDuplicateTerminalSerialNo = _db.TblTerminals
    .AsNoTracking()
    .Any(x => x.TerminalSerialNo == terminalSerialNo
        && x.TerminalId != id
        && x.DeleteFlag == false);
```

### Delete Rules

`Delete` should:

- Validate `id > 0`.
- Find terminal where `DeleteFlag == false`.
- Return `404` when missing.
- Soft delete by setting `DeleteFlag = true`.
- Do not physically delete the record.

### DbUpdateException Handling

Even with manual duplicate validation, keep a `DbUpdateException` catch because two requests can race.

For duplicate terminal serial number, return:

```csharp
return new Result<TerminalModel>
{
    IsSuccess = false,
    StatusCode = 409,
    Message = "Terminal serial number already exists."
};
```

For create, return `Result<TerminalCreateResponseModel>` with the same status and message.

## API Controller

Create:

`YbsSmartCardSystem.Api/Controllers/TerminalController.cs`

Suggested endpoints:

```http
GET    /api/Terminal
GET    /api/Terminal/{id}
POST   /api/Terminal
PATCH  /api/Terminal/{id}
DELETE /api/Terminal/{id}
```

Suggested controller:

```csharp
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Terminal;
using YbsSmartCardSystem.Domain.Features.Terminal.Models;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TerminalController : BaseController
{
    private readonly TerminalService _terminalService;

    public TerminalController(TerminalService terminalService)
    {
        _terminalService = terminalService;
    }

    [HttpGet]
    public IActionResult TerminalList([FromQuery] TerminalListRequestModel request)
    {
        var result = _terminalService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    public IActionResult TerminalGetById(int id)
    {
        var result = _terminalService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    public IActionResult TerminalCreate([FromBody] TerminalCreateRequestModel request)
    {
        var result = _terminalService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    public IActionResult TerminalPatch(int id, [FromBody] TerminalPatchRequestModel request)
    {
        var result = _terminalService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    public IActionResult TerminalDelete(int id)
    {
        var result = _terminalService.Delete(id);
        return Execute(result);
    }
}
```

## API Registration

Update:

`YbsSmartCardSystem.Api/Program.cs`

Add:

```csharp
using YbsSmartCardSystem.Domain.Features.Terminal;
```

Register:

```csharp
builder.Services.AddScoped<TerminalService>();
```

## Blazor API Service

Update:

`YbsSmartCardSystem.App/Services/ApiService.cs`

Add methods:

```csharp
public async Task<Result<TerminalListResponseModel>> GetTerminals(TerminalListRequestModel request)
public async Task<Result<TerminalCreateResponseModel>> TerminalCreate(TerminalCreateRequestModel request)
public async Task<Result<TerminalModel>> TerminalPatch(int id, TerminalPatchRequestModel request)
public async Task<Result<TerminalModel>> TerminalDelete(int id)
```

Add endpoints:

```csharp
public const string TerminalList = "api/Terminal";
public const string CreateTerminal = "api/Terminal";
public static string TerminalDetail(int terminalId) => $"api/Terminal/{terminalId}";
```

Also add a way to get bus options for terminal create/edit. If Bus API already exists:

```csharp
public async Task<Result<BusListResponseModel>> GetBuses(BusListRequestModel request)
```

Use this to populate a bus dropdown.

Do not return `result!`. Return friendly failed results when the API returns null or unexpected content.

## Blazor Pages

Create folder:

`YbsSmartCardSystem.App/Components/Features/Terminal`

### Terminal List Page

Create:

`YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor`

Route:

```razor
@page "/terminals"
@rendermode InteractiveServer
@inject ApiService ApiService
@inject IJSRuntime JSRuntime
```

UI requirements:

- New Terminal button linking to `/terminal/new`.
- Table showing active terminals.
- Columns:
  - Actions
  - No
  - Terminal Serial Number
  - Bus No
  - Bus License
  - Active
- Each row should have:
  - Edit button
  - Delete button
- Edit should use PATCH.
- Delete should call DELETE after confirmation.
- Show success/error messages on the page.

Recommended component state:

```csharp
private TerminalListRequestModel request = new();
private Result<TerminalListResponseModel> response = new();
private TerminalPatchRequestModel editTerminal = new();
private Result<BusListResponseModel> busOptions = new();
private int? editingTerminalId;
private string? message;
private bool isSaving;
```

Recommended methods:

```csharp
private async Task LoadTerminals()
private async Task LoadBusOptions()
private void EditTerminal(TerminalModel terminal)
private void CancelEdit()
private bool ValidateEdit()
private async Task UpdateTerminal()
private async Task DeleteTerminal(int id)
```

Edit validation:

- Terminal serial number required.
- Terminal serial number max length 100.
- BusId required and greater than 0.

The edit form should use a dropdown for bus selection.

### Terminal Create Page

Create:

`YbsSmartCardSystem.App/Components/Features/Terminal/TerminalCreate.razor`

Route:

```razor
@page "/terminal/new"
@rendermode InteractiveServer
@inject ApiService ApiService
@inject NavigationManager NavigationManager
```

Fields:

- Terminal Serial Number
- Bus dropdown
- IsActive checkbox

Validation:

- Terminal serial number required.
- Terminal serial number max length 100.
- Bus is required.

After successful create:

- Navigate back to `/terminals`.

## Navigation

Update:

`YbsSmartCardSystem.App/Components/Layout/NavMenu.razor`

Add:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="terminals">
        <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Terminal List
    </NavLink>
</div>
```

## Recommended Status Codes

Use these response statuses:

- `200 OK`: success.
- `400 Bad Request`: invalid input.
- `404 Not Found`: terminal or bus not found.
- `409 Conflict`: duplicate `TerminalSerialNo`.
- `500 Internal Server Error`: unexpected failure.

Example duplicate response:

```json
{
  "isSuccess": false,
  "isError": true,
  "message": "Terminal serial number already exists.",
  "data": null,
  "statusCode": 409
}
```

## Base Controller Requirement

If not already updated, `BaseController.Execute` should respect `Result<T>.StatusCode`.

Recommended:

```csharp
[NonAction]
public IActionResult Execute<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        return Ok(result);
    }

    return StatusCode(result.StatusCode, result);
}
```

## Verification Checklist

Run:

```powershell
dotnet build YbsSmartCardSystem.slnx
```

Backend verification:

- `GET /api/Terminal` returns active terminals only.
- `GET /api/Terminal/{id}` returns one terminal.
- `GET /api/Terminal/{invalidId}` returns `404`.
- `POST /api/Terminal` creates a valid terminal.
- Empty `TerminalSerialNo` returns `400`.
- `TerminalSerialNo` over 100 characters returns `400`.
- Duplicate `TerminalSerialNo` returns `409`.
- Invalid `BusId` returns `404`.
- Deleted bus cannot be assigned to a terminal.
- `PATCH /api/Terminal/{id}` updates only supplied fields.
- Duplicate patched `TerminalSerialNo` returns `409`.
- `DELETE /api/Terminal/{id}` sets `DeleteFlag = true`.
- Deleted terminals no longer appear in list.

Frontend verification:

- `/terminals` loads terminal list.
- `/terminal/new` loads bus dropdown.
- Create form validates required fields.
- Create form validates max length.
- Duplicate serial number shows friendly error.
- Edit fills form with selected terminal data.
- Edit uses bus dropdown.
- Update uses PATCH and refreshes list.
- Cancel clears edit mode.
- Delete asks for confirmation.
- Delete removes terminal from visible list after success.

## Notes

Do not remove the unique constraint on `Tbl_Terminal.TerminalSerialNo`.

Do not make `BusNo` unique. Bus number can be duplicated.

Bus license is the field that must be unique for buses.

Use soft delete only. Do not physically delete terminal records.
