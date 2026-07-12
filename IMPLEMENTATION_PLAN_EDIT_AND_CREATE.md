# Implementation Plan: Card Edit With PATCH and Complete Create Validation

## Goal

Complete the Card edit feature using the existing `PATCH api/Card/{id}` endpoint, and make the Card create feature complete with client-side and backend validation.

Do not replace the PATCH flow with PUT. The edit feature must use `CardPatchRequestModel` and the existing `CardController.CardPatch` endpoint.

## Current Project Shape

The solution has four projects:

- `YbsSmartCardSystem.Api`: ASP.NET Core Web API.
- `YbsSmartCardSystem.App`: Blazor Server UI.
- `YbsSmartCardSystem.Domain`: service logic and request/response models.
- `YbsSmartCardSystem.Database`: EF Core database context and table models.

The relevant files are:

- `YbsSmartCardSystem.Api/Controllers/CardController.cs`
- `YbsSmartCardSystem.Api/Controllers/BaseController.cs`
- `YbsSmartCardSystem.App/Components/Features/Card/CardList.razor`
- `YbsSmartCardSystem.App/Components/Features/Card/CardList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/Card/CardCreate.razor`
- `YbsSmartCardSystem.App/Services/ApiService.cs`
- `YbsSmartCardSystem.Domain/Features/Card/CardService.cs`
- `YbsSmartCardSystem.Domain/Features/Card/Models/CardList.cs`

## Important Existing Issues To Fix First

### 1. `CardList.razor.cs` has invalid method placement

`EditCard` is currently outside the `CardList` partial class and has no return type. Move it inside the class and give it a valid return type.

Recommended shape:

```csharp
private void EditCard(CardModel card)
{
    // copy selected card into edit model
}
```

### 2. The edit model type is wrong

The list table contains `CardModel` items, but the current edit method expects `CardPatchResponseModel`.

Use:

```csharp
private CardPatchRequestModel editCard = new();
private int? editingCardId;
```

`CardPatchResponseModel` is not needed for this UI flow.

### 3. `CardService.Update` references a commented-out model

`CardService.Update(int id, CardUpdateRequestModel request)` still exists, but `CardUpdateRequestModel` is commented out in `CardList.cs`.

Options:

- Preferred: remove the unused `Update` method if PUT update is not needed.
- Alternative: uncomment/restore `CardUpdateRequestModel`.

Since this task requires PATCH for edit, removing the unused `Update` method is acceptable if nothing calls it.

## Edit Feature Requirements

### UI Behavior

On `/cards`:

1. Show the card list.
2. Each row has an `Edit` button.
3. Clicking `Edit` copies the selected card into an edit form.
4. The edit form should show:
   - Card number
   - Owner name
   - Mobile number
   - Balance
5. The form should have:
   - `Update`
   - `Cancel`
6. Clicking `Cancel` clears edit mode.
7. Clicking `Update` sends a PATCH request.
8. On success:
   - show success message,
   - clear edit mode,
   - reload the card list.
9. On failure:
   - show the validation/API message,
   - keep the edit form open.

### Suggested `CardList.razor.cs` Fields

```csharp
private CardListRequestModel request = new();
private Result<CardListResponseModel> response = new();
private CardPatchRequestModel editCard = new();
private int? editingCardId;
private string? message;
private bool isSaving;
```

### Suggested Methods

```csharp
private async Task LoadCards()
{
    response = await ApiService.GetCards(request);
}

private void EditCard(CardModel card)
{
    editingCardId = card.CardId;
    editCard = new CardPatchRequestModel
    {
        CardNum = card.CardNum,
        OwnerName = card.OwnerName,
        MobileNo = card.MobileNo,
        Balance = card.Balance
    };
    message = null;
}

private void CancelEdit()
{
    editingCardId = null;
    editCard = new CardPatchRequestModel();
    message = null;
}

private async Task UpdateCard()
{
    if (editingCardId is null)
    {
        return;
    }

    isSaving = true;

    var result = await ApiService.CardPatch(editingCardId.Value, editCard);
    message = result.Message;

    if (result.IsSuccess)
    {
        CancelEdit();
        await LoadCards();
    }

    isSaving = false;
}
```

Also reset `rowNo` before rendering the list, or avoid using a mutable `rowNo` field inside markup because it can produce wrong numbering on re-render.

## API Service Requirements

Add PATCH support to `YbsSmartCardSystem.App/Services/ApiService.cs`.

### Endpoint Helper

Use a method instead of the current string placeholder:

```csharp
public static string CardDetail(int cardId) => $"api/Card/{cardId}";
```

### PATCH Method

Add:

```csharp
public async Task<Result<CardModel>> CardPatch(int id, CardPatchRequestModel request)
{
    var httpClient = _httpClientFactory.CreateClient();
    httpClient.BaseAddress = new Uri(_baseUrl);

    var response = await httpClient.PatchAsJsonAsync(ApiEndpoints.CardDetail(id), request);
    var result = await response.Content.ReadFromJsonAsync<Result<CardModel>>();

    return result ?? new Result<CardModel>
    {
        IsSuccess = false,
        Message = "Invalid response from API."
    };
}
```

If `PatchAsJsonAsync` is unavailable, use `HttpRequestMessage`:

```csharp
var httpRequest = new HttpRequestMessage(HttpMethod.Patch, ApiEndpoints.CardDetail(id))
{
    Content = JsonContent.Create(request)
};

var response = await httpClient.SendAsync(httpRequest);
```

Add any required `using` statements.

## Create Feature Requirements

The create feature should validate both in the UI and in the domain service.

Backend validation is required even if client-side validation exists.

### Backend Validation In `CardService.Create`

In `YbsSmartCardSystem.Domain/Features/Card/CardService.cs`, validate before creating the entity:

1. Request must not be null.
2. `CardNum` is required.
3. `OwnerName` is required.
4. `MobileNo` must not exceed 20 characters when provided.
5. `CardNum` must be unique among active cards where `DeleteFlag == false`.
6. New cards should start with `Balance = 0` unless the model is intentionally changed to accept an initial balance.

Suggested validation:

```csharp
if (request is null)
{
    return new Result<CardCreateResponseModel>
    {
        IsSuccess = false,
        Message = "Request data is required."
    };
}

if (string.IsNullOrWhiteSpace(request.CardNum))
{
    return new Result<CardCreateResponseModel>
    {
        IsSuccess = false,
        Message = "Card number is required."
    };
}

if (string.IsNullOrWhiteSpace(request.OwnerName))
{
    return new Result<CardCreateResponseModel>
    {
        IsSuccess = false,
        Message = "Owner name is required."
    };
}

if (!string.IsNullOrWhiteSpace(request.MobileNo) && request.MobileNo.Length > 20)
{
    return new Result<CardCreateResponseModel>
    {
        IsSuccess = false,
        Message = "Mobile number cannot exceed 20 characters."
    };
}

var isDuplicateCardNum = _db.TblCards
    .AsNoTracking()
    .Any(x => x.CardNum == request.CardNum && x.DeleteFlag == false);

if (isDuplicateCardNum)
{
    return new Result<CardCreateResponseModel>
    {
        IsSuccess = false,
        Message = "Card number already exists."
    };
}
```

When saving:

```csharp
var card = new TblCard
{
    CardNum = request.CardNum.Trim(),
    OwnerName = request.OwnerName.Trim(),
    MobileNo = string.IsNullOrWhiteSpace(request.MobileNo) ? null : request.MobileNo.Trim(),
    Balance = 0,
    CreatedDate = DateTime.Now,
    DeleteFlag = false
};
```

### UI Validation In `CardCreate.razor`

Add local validation before calling the API:

- Card number is required.
- Owner name is required.
- Mobile number cannot exceed 20 characters.

Show the message in the page instead of only using `alert`.

Suggested fields:

```csharp
private CardCreateRequestModel request = new();
private string? message;
private bool isSaving;
```

Suggested validation method:

```csharp
private bool Validate()
{
    if (string.IsNullOrWhiteSpace(request.CardNum))
    {
        message = "Card number is required.";
        return false;
    }

    if (string.IsNullOrWhiteSpace(request.OwnerName))
    {
        message = "Owner name is required.";
        return false;
    }

    if (!string.IsNullOrWhiteSpace(request.MobileNo) && request.MobileNo.Length > 20)
    {
        message = "Mobile number cannot exceed 20 characters.";
        return false;
    }

    return true;
}
```

Suggested save flow:

```csharp
private async Task Save()
{
    if (!Validate())
    {
        return;
    }

    isSaving = true;

    var response = await ApiService.CardCreate(request);
    message = response.Message;

    if (response.IsSuccess)
    {
        request = new CardCreateRequestModel();
        NavigationManager.NavigateTo("/cards");
    }

    isSaving = false;
}
```

## Recommended Base Controller Cleanup

Current `BaseController.Execute(object data)` serializes and deserializes the result just to read `IsSuccess`.

Replace it with a generic version:

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

This makes API responses more predictable and removes unnecessary Newtonsoft JSON conversion.

## Error Handling In `ApiService`

Avoid returning `result!` from API calls. If the API returns unexpected content, return a friendly failed result.

Example:

```csharp
return result ?? new Result<CardCreateResponseModel>
{
    IsSuccess = false,
    Message = "Invalid response from API."
};
```

Consider wrapping HTTP calls in `try/catch` so the UI shows a friendly message if the API is offline.

## Verification Checklist

Run:

```powershell
dotnet build YbsSmartCardSystem.slnx
```

Then manually verify:

- `/card/new` with empty card number shows validation.
- `/card/new` with empty owner name shows validation.
- `/card/new` with mobile number longer than 20 characters shows validation.
- Creating a duplicate card number returns a friendly API error.
- Valid card creation succeeds and navigates back to `/cards`.
- `/cards` loads card data.
- Clicking `Edit` fills the edit form with selected card data.
- Clicking `Cancel` clears the edit form.
- Clicking `Update` calls PATCH and updates the card.
- Invalid edit values show a friendly error and keep the form open.

## Notes

Do not implement edit with PUT. Use PATCH only.

Do not remove soft-delete behavior. All list, duplicate checks, get, patch, and delete operations should respect `DeleteFlag == false`.

Be careful with existing uncommitted changes. Preserve user work unless a change is directly required for this implementation.
