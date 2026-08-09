# Fix Terminal and Administration Loading Issues - Implementation Plan

## Purpose

Fix these current UI failures:

- Terminal menu shows `Error Loading Terminals` with the generic message `Unable to load terminals. Please check your permission and API connection.`
- Administration pages keep loading, show no data, and Blazor displays `An unhandled error has occurred.`

Do not redesign workflows. This is a reliability and diagnostics fix for API calls, permissions, and page loading states.

## Most Likely Root Causes

1. `ApiService` reads JSON before checking HTTP status on many endpoints.
   - If API returns `401`, `403`, HTML error content, or an empty response, `ReadFromJsonAsync` can throw.
   - A thrown/failed API call leaves page list properties as `null`, so pages continue showing the loading spinner.

2. Terminal page loading state is mixed with API result state.
   - `response.Data == null` is used as loading.
   - A failed response with no useful message becomes the generic terminal error.

3. Administration pages use nullable list values as loading state.
   - `Roles == null` and `Permissions == null` mean loading.
   - On failed API result, the code sets `ErrorMessage` but leaves the list as `null`, so the spinner stays forever.

4. Permissions may be missing or not assigned.
   - Terminal endpoint requires `Terminal.View`.
   - Administration role/permission endpoints require `RolePermission.View`.
   - Management actions require `RolePermission.Manage`.

5. API may not be running on the configured backend URL.
   - Blazor app config currently expects API at `https://localhost:7026`.

## Phase 1 - Confirm Runtime Setup

1. Start both projects:
   - API: `YbsSmartCardSystem.Api` on `https://localhost:7026`
   - App: `YbsSmartCardSystem.App` on `https://localhost:7232`

2. Open Swagger:
   - `https://localhost:7026/swagger`
   - Confirm it loads.

3. Confirm app config:
   - File: `YbsSmartCardSystem.App/appsettings.Development.json`
   - `BackendApiUrl` must be:

```json
"BackendApiUrl": "https://localhost:7026"
```

4. If using non-development environment, fix `YbsSmartCardSystem.App/appsettings.json`.
   - It currently has placeholder value:

```json
"BackendApiUrl": "YOUR_BACKEND_API_URL"
```

Replace with the real API URL for that environment.

## Phase 2 - Add Safe API Response Handling

Update `YbsSmartCardSystem.App/Services/ApiService.cs`.

Create a private helper:

```csharp
private static async Task<Result<T>> ReadResultAsync<T>(HttpResponseMessage response, string action)
{
    if (!response.IsSuccessStatusCode)
    {
        return new Result<T>
        {
            IsSuccess = false,
            StatusCode = (int)response.StatusCode,
            Message = GetHttpErrorMessage(response, action)
        };
    }

    try
    {
        var result = await response.Content.ReadFromJsonAsync<Result<T>>();
        return result ?? new Result<T>
        {
            IsSuccess = false,
            StatusCode = (int)response.StatusCode,
            Message = "Invalid response from API."
        };
    }
    catch
    {
        return new Result<T>
        {
            IsSuccess = false,
            StatusCode = (int)response.StatusCode,
            Message = $"API returned an invalid response while trying to {action}."
        };
    }
}
```

Keep or add this helper:

```csharp
private static string GetHttpErrorMessage(HttpResponseMessage response, string action)
{
    return (int)response.StatusCode switch
    {
        401 => $"Please log in again to {action}.",
        403 => $"You do not have permission to {action}.",
        _ => $"API returned {(int)response.StatusCode} while trying to {action}."
    };
}
```

Refactor these methods first:

- `GetTerminals`
- `GetBuses`
- `GetRoles`
- `GetRoleById`
- `RoleCreate`
- `RolePatch`
- `RoleDelete`
- `GetPermissions(PermissionListRequestModel request)`
- `GetUserRoles`
- `UpdateUserRoles`
- `GetRolePermissions`
- `UpdateRolePermissions`

Example:

```csharp
var response = await httpClient.GetAsync(url);
return await ReadResultAsync<RoleListResponseModel>(response, "load roles");
```

## Phase 3 - Fix Terminal Page Loading State

Update:

- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor`
- `YbsSmartCardSystem.App/Components/Features/Terminal/TerminalList.razor.cs`

Required changes:

1. Add explicit loading state:

```csharp
private bool isLoading = true;
```

2. Set `isLoading` in `LoadTerminals`:

```csharp
private async Task LoadTerminals()
{
    isLoading = true;
    response = await ApiService.GetTerminals(request);
    isLoading = false;
}
```

3. Render logic:
   - If `isLoading`, show spinner.
   - Else if `!response.IsSuccess`, show `response.Message`.
   - Else if `response.Data is null`, show a controlled API response error.
   - Else render empty/list state.

4. Keep bus dropdown loading independent from terminal list.
   - Do not block terminal list while loading bus options.
   - If bus options fail, only disable/hide edit bus dropdown message, not the whole terminal list.

## Phase 4 - Fix Administration List Pages

Update:

- `YbsSmartCardSystem.App/Components/Features/RolePermission/RoleList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/RoleList.razor`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/PermissionList.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/PermissionList.razor`

Required changes:

1. Add explicit loading state:

```csharp
private bool IsLoading { get; set; } = true;
```

2. In failed API responses, assign an empty list:

```csharp
Roles = [];
TotalCount = 0;
ErrorMessage = result.Message ?? "Failed to load roles.";
```

```csharp
Permissions = [];
TotalCount = 0;
ErrorMessage = result.Message ?? "Failed to load permissions.";
```

3. Always clear loading in `finally`:

```csharp
private async Task LoadRoles()
{
    IsLoading = true;
    ErrorMessage = null;

    try
    {
        var result = await Api.GetRoles(Request);
        if (result.IsSuccess && result.Data != null)
        {
            Roles = result.Data.Roles;
            TotalCount = result.Data.TotalCount;
        }
        else
        {
            Roles = [];
            TotalCount = 0;
            ErrorMessage = result.Message ?? "Failed to load roles.";
        }
    }
    finally
    {
        IsLoading = false;
    }
}
```

4. In `.razor`, use `IsLoading` for spinner instead of `Roles == null` or `Permissions == null`.

## Phase 5 - Fix Administration Manage Pages

Update:

- `YbsSmartCardSystem.App/Components/Features/RolePermission/RolePermissionManage.razor.cs`
- `YbsSmartCardSystem.App/Components/Features/RolePermission/UserRoleManage.razor.cs`

Required changes:

1. Wrap `LoadData` in `try/finally`.
2. Set safe empty collections on failure.
3. Always set `IsLoading = false` in `finally`.
4. Wrap save handlers in `try/finally` so `IsSaving` does not stay true forever.

## Phase 6 - Verify Permissions in Database

Confirm these permission records exist in `Tbl_Permission`:

- `Terminal.View`
- `Terminal.Create`
- `Terminal.Update`
- `Terminal.Delete`
- `RolePermission.View`
- `RolePermission.Manage`

Confirm admin role has at least:

- `Terminal.View`
- `RolePermission.View`
- `RolePermission.Manage`

Confirm operator role has terminal access if required:

- `Terminal.View`
- `Terminal.Create`
- `Terminal.Update`

Confirm viewer role does not get administration permissions.

If the logged-in admin still gets `403`, fix role-permission seed data or assign missing permissions through database records.

## Phase 7 - Backend Query Safety

Review `YbsSmartCardSystem.Domain/Features/Terminal/TerminalService.cs`.

Current list query depends on bus relationship:

```csharp
.Where(x => x.DeleteFlag == false && x.Bus.DeleteFlag == false)
```

If terminals can ever have invalid/missing bus references, make the query resilient:

- Keep `BusId` required if the workflow requires every terminal to have a bus.
- Otherwise allow safe projection with null bus handling.

Recommended if bus is required:

```csharp
.Where(x => !x.DeleteFlag && !x.Bus.DeleteFlag)
```

Recommended if unassigned terminals are allowed later:

```csharp
.Where(x => !x.DeleteFlag && (x.Bus == null || !x.Bus.DeleteFlag))
```

Only choose the second option if the database/model is changed to allow nullable `BusId`.

## Phase 8 - Validation

Run:

```bash
dotnet build YbsSmartCardSystem.slnx -c Release --no-restore
```

Manual checks:

1. Log in as admin.
2. Open Terminal menu.
   - Expected: terminal list, empty state, or exact permission/API error.
   - Not acceptable: infinite spinner or generic unhandled Blazor banner.
3. Open Administration > Roles.
4. Open Administration > Permissions.
5. Open role permission manage screen.
6. Temporarily test with an account lacking permission.
   - Expected: clean `You do not have permission...` message.
   - Not acceptable: spinner forever.

## Done Criteria

- Terminal menu never spins forever.
- Administration menus never spin forever after API failure.
- Blazor corner error no longer appears for normal API failures.
- `401` tells user to log in again.
- `403` tells user permission is missing.
- Admin can load roles, permissions, and terminals.
- Viewer can only access allowed viewer pages plus bus/terminal view pages if intended.
- Build succeeds with zero errors.
