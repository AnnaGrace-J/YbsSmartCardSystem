# Implementation Plan: Fix Bus Duplicate Error and Add Proper Validation

## Goal

Fix this runtime error:

```text
Microsoft.EntityFrameworkCore.DbUpdateException:
Violation of UNIQUE KEY constraint 'UQ__Tbl_Bus__6A0F3A401D8FBE4D'.
Cannot insert duplicate key in object 'dbo.Tbl_Bus'. The duplicate key value is (313).
```

The app should not expose SQL Server errors to the user. Duplicate `BusNo` should be caught by backend validation before `_db.SaveChanges()`, and the frontend should also validate obvious invalid input before calling the API.

## Expected Behavior

When creating or editing a bus:

- Empty `BusNo` should show validation error.
- Empty `BusLicense` should show validation error.
- `BusNo` longer than 50 characters should show validation error.
- `BusLicense` longer than 50 characters should show validation error.
- Duplicate `BusNo` should return a friendly error:

```text
Bus number already exists.
```

Recommended HTTP status:

- `400 Bad Request` for missing/invalid input.
- `404 Not Found` when updating/deleting/getting a bus that does not exist.
- `409 Conflict` when `BusNo` already exists.

## Files To Update

- `YbsSmartCardSystem.Domain/Result.cs`
- `YbsSmartCardSystem.Domain/Features/Bus/BusService.cs`
- `YbsSmartCardSystem.Api/Controllers/BaseController.cs`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusCreate.razor`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusList.razor`
- `YbsSmartCardSystem.App/Components/Features/Bus/BusList.razor.cs`
- `YbsSmartCardSystem.App/Services/ApiService.cs`

## Step 1: Add Status Code Support To `Result<T>`

Update:

`YbsSmartCardSystem.Domain/Result.cs`

Add a status code property.

```csharp
namespace YbsSmartCardSystem.Domain
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public bool IsError { get { return !IsSuccess; } }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
```

Use these values:

```csharp
StatusCodes.Status200OK
StatusCodes.Status400BadRequest
StatusCodes.Status404NotFound
StatusCodes.Status409Conflict
StatusCodes.Status500InternalServerError
```

If the domain project cannot reference `Microsoft.AspNetCore.Http`, use raw numbers instead:

```csharp
200
400
404
409
500
```

Recommended: use raw numbers in the domain project to keep it independent from ASP.NET Core.

## Step 2: Fix `BaseController.Execute`

Update:

`YbsSmartCardSystem.Api/Controllers/BaseController.cs`

Replace the JSON serialize/deserialize logic with a generic method:

```csharp
using Microsoft.AspNetCore.Mvc;
using YbsSmartCardSystem.Domain;

namespace YbsSmartCardSystem.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController : ControllerBase
    {
        [NonAction]
        public IActionResult Execute<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return StatusCode(result.StatusCode, result);
        }
    }
}
```

Important: failed service methods must set `StatusCode`, otherwise failed responses may accidentally return `200`.

## Step 3: Backend Validation In `BusService.Create`

Update:

`YbsSmartCardSystem.Domain/Features/Bus/BusService.cs`

Before creating `TblBus`, validate all input.

```csharp
public Result<BusCreateResponseModel> Create(BusCreateRequestModel request)
{
    try
    {
        if (request is null)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Request data is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.BusNo))
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Bus number is required."
            };
        }

        if (string.IsNullOrWhiteSpace(request.BusLicense))
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Bus license is required."
            };
        }

        var busNo = request.BusNo.Trim();
        var busLicense = request.BusLicense.Trim();

        if (busNo.Length > 50)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Bus number cannot exceed 50 characters."
            };
        }

        if (busLicense.Length > 50)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 400,
                Message = "Bus license cannot exceed 50 characters."
            };
        }

        var isDuplicateBusNo = _db.TblBus
            .AsNoTracking()
            .Any(x => x.BusNo == busNo && x.DeleteFlag == false);

        if (isDuplicateBusNo)
        {
            return new Result<BusCreateResponseModel>
            {
                IsSuccess = false,
                StatusCode = 409,
                Message = "Bus number already exists."
            };
        }

        var bus = new TblBus
        {
            BusNo = busNo,
            BusLicense = busLicense,
            CreatedDate = DateTime.Now,
            DeleteFlag = false
        };

        _db.TblBus.Add(bus);
        _db.SaveChanges();

        return new Result<BusCreateResponseModel>
        {
            IsSuccess = true,
            StatusCode = 200,
            Message = "Bus created successfully.",
            Data = new BusCreateResponseModel
            {
                BusId = bus.BusId,
                BusNo = bus.BusNo,
                BusLicense = bus.BusLicense
            }
        };
    }
    catch (DbUpdateException)
    {
        return new Result<BusCreateResponseModel>
        {
            IsSuccess = false,
            StatusCode = 409,
            Message = "Bus number already exists."
        };
    }
    catch (Exception ex)
    {
        return new Result<BusCreateResponseModel>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = ex.Message
        };
    }
}
```

Why keep the `DbUpdateException` catch if duplicate validation already exists?

Because two users could submit the same `BusNo` at almost the same time. The manual duplicate check improves user experience, but the database unique key is still the final protection.

## Step 4: Backend Validation In `BusService.Patch`

In `Patch`, duplicate check must exclude the current bus:

```csharp
var isDuplicateBusNo = _db.TblBus
    .AsNoTracking()
    .Any(x => x.BusNo == busNo
        && x.BusId != id
        && x.DeleteFlag == false);
```

Suggested validation flow:

```csharp
if (id <= 0)
{
    return new Result<BusModel>
    {
        IsSuccess = false,
        StatusCode = 400,
        Message = "BusId is required."
    };
}

if (request is null)
{
    return new Result<BusModel>
    {
        IsSuccess = false,
        StatusCode = 400,
        Message = "Request data is required."
    };
}

if (request.BusNo is null && request.BusLicense is null)
{
    return new Result<BusModel>
    {
        IsSuccess = false,
        StatusCode = 400,
        Message = "At least one field is required to update."
    };
}

var item = _db.TblBus.FirstOrDefault(x => x.BusId == id && x.DeleteFlag == false);

if (item is null)
{
    return new Result<BusModel>
    {
        IsSuccess = false,
        StatusCode = 404,
        Message = "Bus not found."
    };
}
```

Then validate supplied fields:

```csharp
if (request.BusNo is not null)
{
    if (string.IsNullOrWhiteSpace(request.BusNo))
    {
        return new Result<BusModel>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = "Bus number cannot be empty."
        };
    }

    var busNo = request.BusNo.Trim();

    if (busNo.Length > 50)
    {
        return new Result<BusModel>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = "Bus number cannot exceed 50 characters."
        };
    }

    var isDuplicateBusNo = _db.TblBus
        .AsNoTracking()
        .Any(x => x.BusNo == busNo
            && x.BusId != id
            && x.DeleteFlag == false);

    if (isDuplicateBusNo)
    {
        return new Result<BusModel>
        {
            IsSuccess = false,
            StatusCode = 409,
            Message = "Bus number already exists."
        };
    }

    item.BusNo = busNo;
}
```

For `BusLicense`:

```csharp
if (request.BusLicense is not null)
{
    if (string.IsNullOrWhiteSpace(request.BusLicense))
    {
        return new Result<BusModel>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = "Bus license cannot be empty."
        };
    }

    var busLicense = request.BusLicense.Trim();

    if (busLicense.Length > 50)
    {
        return new Result<BusModel>
        {
            IsSuccess = false,
            StatusCode = 400,
            Message = "Bus license cannot exceed 50 characters."
        };
    }

    item.BusLicense = busLicense;
}
```

Wrap `SaveChanges()` with `DbUpdateException` catch:

```csharp
catch (DbUpdateException)
{
    return new Result<BusModel>
    {
        IsSuccess = false,
        StatusCode = 409,
        Message = "Bus number already exists."
    };
}
```

## Step 5: Frontend Validation In `BusCreate.razor`

Add page-level validation before calling `ApiService.BusCreate`.

Suggested state:

```csharp
private BusCreateRequestModel request = new();
private string? message;
private bool isSaving;
```

Suggested validation:

```csharp
private bool Validate()
{
    if (string.IsNullOrWhiteSpace(request.BusNo))
    {
        message = "Bus number is required.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(request.BusLicense))
    {
        message = "Bus license is required.";
        return false;
    }

    if (request.BusNo.Trim().Length > 50)
    {
        message = "Bus number cannot exceed 50 characters.";
        return false;
    }

    if (request.BusLicense.Trim().Length > 50)
    {
        message = "Bus license cannot exceed 50 characters.";
        return false;
    }

    return true;
}
```

Suggested save:

```csharp
private async Task Save()
{
    if (!Validate())
    {
        return;
    }

    isSaving = true;

    var result = await ApiService.BusCreate(request);
    message = result.Message;

    if (result.IsSuccess)
    {
        request = new BusCreateRequestModel();
        NavigationManager.NavigateTo("/buses");
    }

    isSaving = false;
}
```

In the markup, show `message` on the page:

```razor
@if (!string.IsNullOrWhiteSpace(message))
{
    <div class="alert alert-info">@message</div>
}
```

Also add `maxlength` attributes:

```razor
<input class="form-control" maxlength="50" @bind-value="request.BusNo" />
<input class="form-control" maxlength="50" @bind-value="request.BusLicense" />
```

## Step 6: Frontend Validation In Bus Edit

In `BusList.razor.cs`, validate before PATCH.

Suggested validation:

```csharp
private bool ValidateEdit()
{
    if (string.IsNullOrWhiteSpace(editBus.BusNo))
    {
        message = "Bus number is required.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(editBus.BusLicense))
    {
        message = "Bus license is required.";
        return false;
    }

    if (editBus.BusNo.Trim().Length > 50)
    {
        message = "Bus number cannot exceed 50 characters.";
        return false;
    }

    if (editBus.BusLicense.Trim().Length > 50)
    {
        message = "Bus license cannot exceed 50 characters.";
        return false;
    }

    return true;
}
```

Use it before calling API:

```csharp
private async Task UpdateBus()
{
    if (editingBusId is null)
    {
        return;
    }

    if (!ValidateEdit())
    {
        return;
    }

    isSaving = true;

    var result = await ApiService.BusPatch(editingBusId.Value, editBus);
    message = result.Message;

    if (result.IsSuccess)
    {
        CancelEdit();
        await LoadBuses();
    }

    isSaving = false;
}
```

## Step 7: Improve `ApiService` Error Handling

Update:

`YbsSmartCardSystem.App/Services/ApiService.cs`

Do not use `result!`. Return friendly errors when the API response cannot be parsed.

For create:

```csharp
public async Task<Result<BusCreateResponseModel>> BusCreate(BusCreateRequestModel request)
{
    try
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_baseUrl);

        var response = await httpClient.PostAsJsonAsync(ApiEndpoints.CreateBus, request);
        var result = await response.Content.ReadFromJsonAsync<Result<BusCreateResponseModel>>();

        return result ?? new Result<BusCreateResponseModel>
        {
            IsSuccess = false,
            StatusCode = (int)response.StatusCode,
            Message = "Invalid response from API."
        };
    }
    catch (Exception ex)
    {
        return new Result<BusCreateResponseModel>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = ex.Message
        };
    }
}
```

Apply the same pattern to:

- `GetBuses`
- `BusPatch`
- `BusDelete`

## Step 8: Make Sure API Returns Conflict For Duplicate

After backend fixes, duplicate bus creation should return:

```http
HTTP/1.1 409 Conflict
```

Response body:

```json
{
  "isSuccess": false,
  "isError": true,
  "message": "Bus number already exists.",
  "data": null,
  "statusCode": 409
}
```

Invalid missing input should return:

```http
HTTP/1.1 400 Bad Request
```

Not found should return:

```http
HTTP/1.1 404 Not Found
```

## Verification Checklist

Run:

```powershell
dotnet build YbsSmartCardSystem.slnx
```

Manual API checks:

- Create bus with empty `BusNo`: returns `400`.
- Create bus with empty `BusLicense`: returns `400`.
- Create bus with `BusNo` over 50 chars: returns `400`.
- Create bus with `BusLicense` over 50 chars: returns `400`.
- Create bus with duplicate `BusNo`: returns `409`, not `DbUpdateException`.
- Patch bus to duplicate `BusNo`: returns `409`.
- Patch missing bus id: returns `404`.

Manual UI checks:

- `/bus/new` blocks empty values before calling API.
- `/bus/new` blocks values longer than 50 characters.
- Duplicate `BusNo` shows `"Bus number already exists."` on the page.
- Edit duplicate `BusNo` shows `"Bus number already exists."`.
- No SQL exception text is shown in the browser.

## Notes

Frontend validation improves user experience, but backend validation is mandatory. SQL Server unique constraints must remain in place as final data protection.

Do not remove the unique constraint on `Tbl_Bus.BusNo`. Fix the application logic instead.
