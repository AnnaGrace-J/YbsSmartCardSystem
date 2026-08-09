# Fix ObjectDisposedException and Infinite Loading - Implementation Plan

## Purpose

Fix the current runtime failure:

```text
System.ObjectDisposedException: Cannot access a disposed object.
```

Symptoms:

- Terminal List keeps loading or shows a generic load error.
- Administration menus keep loading and show no data.
- Blazor Server displays `An unhandled error has occurred.`

This is a Blazor Server circuit/lifecycle issue, not a build issue. The solution must prevent disposed components from being updated by late async work.

## Confirmed Findings

1. The solution builds successfully.
   - `dotnet build YbsSmartCardSystem.slnx -c Release --no-restore` passes with `0` errors.

2. `TerminalList.razor.cs` currently starts a background task:

```csharp
_ = LoadBusOptions();
```

3. `LoadBusOptions` later calls:

```csharp
await InvokeAsync(StateHasChanged);
```

4. If the user leaves Terminal List before bus loading finishes, the component can already be disposed. Then `InvokeAsync(StateHasChanged)` throws `ObjectDisposedException`, kills the Blazor circuit, and later pages can get stuck loading.

## Root Cause

The main root cause is fire-and-forget component work in Blazor Server.

Blazor Server components must not start background UI updates unless they:

- track component disposal,
- use cancellation tokens,
- catch disposal-related exceptions,
- and avoid `StateHasChanged` after disposal.

The current Terminal List background bus load violates that rule.

## Phase 1 - Remove Fire-And-Forget Loading From Terminal List

File:

- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`

Replace this:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadTerminals();
    _ = LoadBusOptions();
}
```

With this:

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadTerminals();
}
```

Do not load bus options on initial page load.

Reason:

- Terminal list rendering does not need bus dropdown data.
- Bus options are only needed when editing a terminal.
- Loading bus options in the background creates the disposed-component exception.

## Phase 2 - Load Bus Options Only When Edit Starts

File:

- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`

Change `EditTerminal` from `void` to `async Task`.

Before setting edit UI, ensure bus options are loaded:

```csharp
private async Task EditTerminal(TerminalModel terminal)
{
    if (busOptions.Data is null)
    {
        await LoadBusOptions();
    }

    editingTerminalId = terminal.TerminalId;
    editTerminalIsActive = terminal.IsActive;
    editTerminal = new TerminalPatchRequestModel
    {
        TerminalSerialNo = terminal.TerminalSerialNo,
        BusId = terminal.BusId,
        IsActive = terminal.IsActive
    };
    message = null;
}
```

In `.razor`, the existing click binding can remain:

```razor
@onclick="() => EditTerminal(localTerminal)"
```

Blazor supports `Task` event handlers.

## Phase 3 - Remove Manual StateHasChanged From Bus Loading

File:

- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`

Change this:

```csharp
private async Task LoadBusOptions()
{
    var result = await ApiService.GetBuses(new BusListRequestModel { PageNo = 1, PageSize = 1000 });
    if (result.IsSuccess)
    {
        busOptions = result;
        await InvokeAsync(StateHasChanged);
    }
}
```

To:

```csharp
private async Task LoadBusOptions()
{
    var result = await ApiService.GetBuses(new BusListRequestModel { PageNo = 1, PageSize = 1000 });
    if (result.IsSuccess)
    {
        busOptions = result;
    }
    else
    {
        message = result.Message ?? "Failed to load bus options.";
        isSuccess = false;
    }
}
```

Reason:

- Event handlers and awaited lifecycle methods re-render automatically.
- Manual `StateHasChanged` is not needed here.
- Removing it removes the disposed-object trigger.

## Phase 4 - Add Try/Finally To Terminal Loading

File:

- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`

Change `LoadTerminals` to always clear loading:

```csharp
private async Task LoadTerminals()
{
    isLoading = true;

    try
    {
        response = await ApiService.GetTerminals(request);
    }
    catch (Exception ex)
    {
        response = new Result<TerminalListResponseModel>
        {
            IsSuccess = false,
            StatusCode = 500,
            Message = $"Failed to load terminals: {ex.Message}"
        };
    }
    finally
    {
        isLoading = false;
    }
}
```

Reason:

- If anything unexpected escapes from `ApiService`, the spinner must still stop.

## Phase 5 - Protect Administration Load Methods From Unhandled Exceptions

Files:

- `YbsSmartCardSystem.App/Components/Features/RolePermission/RoleList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/PermissionList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/RolePermissionManage.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/UserRoleManage.razor.cs`

Current list pages already use `finally`, but they do not catch unexpected exceptions inside the load method.

Add `catch` blocks before `finally`.

Example for roles:

```csharp
catch (Exception ex)
{
    Roles = [];
    TotalCount = 0;
    ErrorMessage = $"Failed to load roles: {ex.Message}";
}
finally
{
    IsLoading = false;
}
```

Example for permissions:

```csharp
catch (Exception ex)
{
    Permissions = [];
    TotalCount = 0;
    ErrorMessage = $"Failed to load permissions: {ex.Message}";
}
finally
{
    IsLoading = false;
}
```

For manage pages:

```csharp
catch (Exception ex)
{
    ErrorMessage = $"Failed to load data: {ex.Message}";
    GroupedPermissions = [];
    SelectedPermissionIds = [];
}
finally
{
    IsLoading = false;
}
```

```csharp
catch (Exception ex)
{
    ErrorMessage = $"Failed to load user roles data: {ex.Message}";
    AvailableRoles = [];
    SelectedRoleIds = [];
}
finally
{
    IsLoading = false;
}
```

Reason:

- A failed HTTP call should become a visible page error, not a Blazor circuit crash.

## Phase 6 - Audit All Components For Fire-And-Forget UI Work

Run:

```bash
rg -n "_ =|InvokeAsync\\(StateHasChanged\\)|Task\\.Run|async void" YbsSmartCardSystem.App/Components YbsSmartCardSystem.App/Services
```

Rules:

- No component should use `_ = SomeAsyncMethod()` for data loading.
- No component should call `InvokeAsync(StateHasChanged)` from a background task unless disposal is handled.
- Avoid `async void` except event signatures that absolutely require it.

For every match:

1. Convert to awaited task if it is part of page loading.
2. Move lazy loading into user-triggered event handlers.
3. Add `try/catch/finally` around loading state.

## Phase 7 - Verify API Connection Separately

Before testing the Blazor pages, verify the API is alive:

1. Open:

```text
https://localhost:7026/swagger
```

2. Confirm `YbsSmartCardSystem.App/appsettings.Development.json` contains:

```json
"BackendApiUrl": "https://localhost:7026"
```

3. If running the app without `Development` environment, fix:

```text
YbsSmartCardSystem.App/appsettings.json
```

It must not stay as:

```json
"BackendApiUrl": "YOUR_BACKEND_API_URL"
```

## Phase 8 - Verify Permissions

Terminal API requires:

- `Terminal.View`

Administration APIs require:

- `RolePermission.View`
- `RolePermission.Manage` for edit/create/delete/save actions

Verify the logged-in admin staff user:

1. Exists in `Tbl_StaffUser`.
2. Is active.
3. Has a role in `Tbl_UserRole`.
4. That role has required permission mappings in `Tbl_RolePermission`.
5. Permission rows in `Tbl_Permission` are active and not deleted.

If the user is a viewer, administration menus must not be available.

## Phase 9 - Restart Both Running Apps

After code changes:

1. Stop both running processes.
2. Start API again.
3. Start Blazor app again.
4. Hard refresh browser.
5. Log in again.

Reason:

- Blazor Server circuits can keep old broken state during hot reload/dev sessions.
- Authentication/permission claims may also be stale until a fresh login.

## Phase 10 - Test Order

Test in this order:

1. Open Terminal List and stay there for 10 seconds.
   - Expected: list, empty state, or clean error.
   - Not acceptable: unhandled circuit error.

2. Open Terminal List, immediately navigate to Dashboard.
   - Expected: no `ObjectDisposedException`.

3. Open Terminal List, click Edit on a terminal.
   - Expected: bus options load only then.

4. Open Administration > Roles.
   - Expected: list, empty state, or clean error.

5. Open Administration > Permissions.
   - Expected: list, empty state, or clean error.

6. Open role permission manage page.
   - Expected: permissions load or visible error.

## Done Criteria

- Console no longer logs `System.ObjectDisposedException`.
- Terminal List does not use `_ = LoadBusOptions()`.
- Terminal List does not call `InvokeAsync(StateHasChanged)` from background loading.
- All affected load methods have `try/catch/finally`.
- Loading indicators always stop.
- API failures show visible page errors.
- Administration pages no longer trigger the Blazor corner error.
- Build passes.
