# Implementation Plan: Bus Feature

## Goal

Implement the Bus feature before building the Transaction feature.

The Bus feature should support basic CRUD operations:

- List buses
- Create bus
- Get bus by id
- Patch bus
- Soft delete bus

Use the existing `Tbl_Bus` table. No database schema change is required for basic Bus CRUD.

## Current Database Shape

Existing EF model:

```csharp
public partial class TblBus
{
    public int BusId { get; set; }
    public string BusNo { get; set; } = null!;
    public string BusLicense { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public bool DeleteFlag { get; set; }
    public virtual ICollection<TblTerminal> TblTerminals { get; set; } = new List<TblTerminal>();
}
```

Existing DB constraints from `AppDbContext`:

- `BusId` primary key
- `BusNo` unique
- `BusNo` max length 50
- `BusLicense` max length 50
- `CreatedDate` default value

The service should still validate duplicate `BusNo` manually so the API returns a friendly error instead of a SQL Server unique constraint exception.

## Relevant Files

Create or update these files:

- `YbsSmartCardSystem.Domain/Features/Bus/Models/BusModels.cs`
- `YbsSmartCardSystem.Domain/Features/Bus/BusService.cs`
- `YbsSmartCardSystem.Api/Controllers/BusController.cs`
- `YbsSmartCardSystem.Api/Program.cs`
- `YbsSmartCardSystem.App/Services/ApiService.cs`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusList.razor`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusCreate.razor`
- `YbsSmartCardSystem.App/Components/Layout/NavMenu.razor`

## Domain Models

Create:

`YbsSmartCardSystem.Domain/Features/Bus/Models/BusModels.cs`

Suggested models:

```csharp
namespace YbsSmartCardSystem.Domain.Features.Bus.Models;

public class BusListRequestModel
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class BusListResponseModel
{
    public List<BusModel> Buses { get; set; } = new();
}

public class BusCreateRequestModel
{
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
}

public class BusCreateResponseModel
{
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
}

public class BusPatchRequestModel
{
    public string? BusNo { get; set; }
    public string? BusLicense { get; set; }
}

public class BusModel
{
    public int BusId { get; set; }
    public string BusNo { get; set; } = string.Empty;
    public string BusLicense { get; set; } = string.Empty;
}
```

## Domain Service

Create:

`YbsSmartCardSystem.Domain/Features/Bus/BusService.cs`

Add methods:

```csharp
public Result<BusListResponseModel> GetList(BusListRequestModel request)
public Result<BusModel> GetById(int id)
public Result<BusCreateResponseModel> Create(BusCreateRequestModel request)
public Result<BusModel> Patch(int id, BusPatchRequestModel request)
public Result<BusModel> Delete(int id)
```

### List Rules

`GetList` should:

- Return buses where `DeleteFlag == false`.
- Order by `BusId` descending.
- Support pagination with `PageNo` and `PageSize`.
- Map `TblBus` to `BusModel`.

### Get By Id Rules

`GetById` should:

- Validate `id > 0`.
- Return only active buses where `DeleteFlag == false`.
- Return `"Bus not found."` when missing.

### Create Rules

`Create` should validate:

- Request data is required.
- `BusNo` is required.
- `BusLicense` is required.
- `BusNo` cannot exceed 50 characters.
- `BusLicense` cannot exceed 50 characters.
- `BusNo` must be unique among active buses where `DeleteFlag == false`.

When saving:

```csharp
var bus = new TblBus
{
    BusNo = request.BusNo.Trim(),
    BusLicense = request.BusLicense.Trim(),
    CreatedDate = DateTime.Now,
    DeleteFlag = false
};
```

### Patch Rules

`Patch` should validate:

- `id > 0`.
- Request data is required.
- At least one field must be supplied.
- If `BusNo` is supplied, it cannot be empty.
- If `BusLicense` is supplied, it cannot be empty.
- If supplied, `BusNo` cannot exceed 50 characters.
- If supplied, `BusLicense` cannot exceed 50 characters.
- If `BusNo` changes, it must remain unique among active buses.
- Target bus must exist and `DeleteFlag == false`.

Patch only the supplied fields.

### Delete Rules

`Delete` should:

- Validate `id > 0`.
- Find the active bus.
- Return `"Bus not found."` when missing.
- Soft delete by setting `DeleteFlag = true`.
- Do not physically delete the record.

## API Controller

Create:

`YbsSmartCardSystem.Api/Controllers/BusController.cs`

Pattern should match `CardController`.

Suggested endpoints:

```http
GET    /api/Bus
GET    /api/Bus/{id}
POST   /api/Bus
PATCH  /api/Bus/{id}
DELETE /api/Bus/{id}
```

Suggested controller:

```csharp
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain.Features.Bus;
using YbsSmartCardSystem.Domain.Features.Bus.Models;

namespace YbsSmartCardSystem.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BusController : BaseController
{
    private readonly BusService _busService;

    public BusController(BusService busService)
    {
        _busService = busService;
    }

    [HttpGet]
    public IActionResult BusList([FromQuery] BusListRequestModel request)
    {
        var result = _busService.GetList(request);
        return Execute(result);
    }

    [HttpGet("{id}")]
    public IActionResult BusGetById(int id)
    {
        var result = _busService.GetById(id);
        return Execute(result);
    }

    [HttpPost]
    public IActionResult BusCreate([FromBody] BusCreateRequestModel request)
    {
        var result = _busService.Create(request);
        return Execute(result);
    }

    [HttpPatch("{id}")]
    public IActionResult BusPatch(int id, [FromBody] BusPatchRequestModel request)
    {
        var result = _busService.Patch(id, request);
        return Execute(result);
    }

    [HttpDelete("{id}")]
    public IActionResult BusDelete(int id)
    {
        var result = _busService.Delete(id);
        return Execute(result);
    }
}
```

## API Registration

Update:

`YbsSmartCardSystem.Api/Program.cs`

Add:

```csharp
using YbsSmartCardSystem.Domain.Features.Bus;
```

Register:

```csharp
builder.Services.AddScoped<BusService>();
```

## Blazor API Service

Update:

`YbsSmartCardSystem.App/Services/ApiService.cs`

Add methods:

```csharp
public async Task<Result<BusListResponseModel>> GetBuses(BusListRequestModel request)
public async Task<Result<BusCreateResponseModel>> BusCreate(BusCreateRequestModel request)
public async Task<Result<BusModel>> BusPatch(int id, BusPatchRequestModel request)
public async Task<Result<BusModel>> BusDelete(int id)
```

Add endpoints:

```csharp
public const string BusList = "api/Bus";
public const string CreateBus = "api/Bus";
public static string BusDetail(int busId) => $"api/Bus/{busId}";
```

Use the same API calling pattern as Card, but avoid returning `result!`. Return a friendly failed `Result<T>` when the API returns null or unexpected content.

Example:

```csharp
return result ?? new Result<BusListResponseModel>
{
    IsSuccess = false,
    Message = "Invalid response from API."
};
```

For PATCH, use either `PatchAsJsonAsync` or `HttpRequestMessage`:

```csharp
var response = await httpClient.PatchAsJsonAsync(ApiEndpoints.BusDetail(id), request);
```

For DELETE:

```csharp
var response = await httpClient.DeleteAsync(ApiEndpoints.BusDetail(id));
```

## Blazor Pages

Create folder:

`YbsSmartCardSystem.App/Components/Features/Bus`

### Bus List Page

Create:

`YbsSmartCardSystem.App/Components/Features/Bus/BusList.razor`

Route:

```razor
@page "/buses"
@rendermode InteractiveServer
@inject ApiService ApiService
@inject IJSRuntime JSRuntime
```

UI requirements:

- New Bus button linking to `/bus/new`.
- Table showing active buses.
- Columns:
  - Actions
  - No
  - Bus No
  - Bus License
- Each row should have:
  - Edit button
  - Delete button
- Edit should use PATCH.
- Delete should call DELETE after confirmation.
- Show success/error messages on the page.

Recommended component state:

```csharp
private BusListRequestModel request = new();
private Result<BusListResponseModel> response = new();
private BusPatchRequestModel editBus = new();
private int? editingBusId;
private string? message;
private bool isSaving;
```

Recommended methods:

```csharp
private async Task LoadBuses()
private void EditBus(BusModel bus)
private void CancelEdit()
private async Task UpdateBus()
private async Task DeleteBus(int id)
```

After update or delete, reload the list.

Avoid using a mutable `rowNo` field that increments inside markup across renders. Prefer:

```razor
@for (var i = 0; i < response.Data.Buses.Count; i++)
{
    var item = response.Data.Buses[i];
    <td>@(i + 1)</td>
}
```

### Bus Create Page

Create:

`YbsSmartCardSystem.App/Components/Features/Bus/BusCreate.razor`

Route:

```razor
@page "/bus/new"
@rendermode InteractiveServer
@inject ApiService ApiService
@inject NavigationManager NavigationManager
```

Fields:

- Bus No
- Bus License

Validation:

- Bus No required.
- Bus License required.
- Bus No max length 50.
- Bus License max length 50.

After successful create:

- Show success message or navigate back to `/buses`.
- Recommended: navigate back to `/buses`.

## Navigation

Update:

`YbsSmartCardSystem.App/Components/Layout/NavMenu.razor`

Add:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="buses">
        <span class="bi bi-list-nested-nav-menu" aria-hidden="true"></span> Bus List
    </NavLink>
</div>
```

## Recommended Base Controller Cleanup

If not already done from the Card feature, update:

`YbsSmartCardSystem.Api/Controllers/BaseController.cs`

Replace the JSON serialize/deserialize logic with:

```csharp
[NonAction]
public IActionResult Execute<T>(Result<T> result)
{
    if (result.IsSuccess)
    {
        return Ok(result);
    }

    return BadRequest(result);
}
```

This makes response handling simpler and more predictable.

## Verification Checklist

Run:

```powershell
dotnet build YbsSmartCardSystem.slnx
```

Backend verification:

- `GET /api/Bus` returns active buses only.
- `GET /api/Bus/{id}` returns one active bus.
- `GET /api/Bus/{invalidId}` returns a friendly error.
- `POST /api/Bus` creates a valid bus.
- Duplicate `BusNo` returns `"Bus number already exists."`.
- Empty `BusNo` returns validation error.
- Empty `BusLicense` returns validation error.
- `PATCH /api/Bus/{id}` updates only supplied fields.
- Duplicate patched `BusNo` returns validation error.
- `DELETE /api/Bus/{id}` sets `DeleteFlag = true`.
- Deleted buses no longer appear in list.

Frontend verification:

- `/buses` loads the bus list.
- `/bus/new` creates a valid bus.
- Create form validates required fields.
- Create form validates max length.
- Edit fills form with selected bus data.
- Update uses PATCH and refreshes list.
- Cancel clears edit mode.
- Delete asks for confirmation.
- Delete removes bus from visible list after success.

## Notes

Do not add a new Bus table. Use the existing `Tbl_Bus`.

Use soft delete only. Do not physically delete bus records.

Preserve existing user changes unless the implementation directly requires touching the file.
