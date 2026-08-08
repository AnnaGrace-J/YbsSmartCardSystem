# Fix Blazor FormName Error Implementation Plan

## Problem

After login, the app shows:

```text
The POST request does not specify which form is being submitted.
To fix this, ensure <form> elements have a @formname attribute with any unique value,
or pass a FormName parameter if using <EditForm>.
```

This happens in Blazor .NET 8 when a submitted form cannot be uniquely identified during server/static rendering.

## Goal

Add unique `FormName` values to all Blazor `EditForm` components so form submission works reliably.

## Scope

Update Blazor `.razor` files only.

Do not change API behavior, authentication logic, database code, or business services.

## Step 1: Find All EditForm Components

Run from the project root:

```powershell
rg "<EditForm" YbsSmartCardSystem.App
```

Review every result.

## Step 2: Fix Login Form

Open:

```text
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor
```

Find:

```razor
<EditForm Model="Model" OnValidSubmit="HandleSubmit">
```

Change it to:

```razor
<EditForm Model="Model" OnValidSubmit="HandleSubmit" FormName="LoginForm">
```

If the form does not already include an antiforgery token, add this inside the `EditForm`:

```razor
<AntiforgeryToken />
```

Expected shape:

```razor
<EditForm Model="Model" OnValidSubmit="HandleSubmit" FormName="LoginForm">
    <AntiforgeryToken />
    <DataAnnotationsValidator />
    ...
</EditForm>
```

## Step 3: Add FormName To Other Forms

Add a unique `FormName` to every other `EditForm`.

Recommended names:

```text
CardCreateForm
PackageCreateForm
TopUpCreateForm
BusCreateForm
TerminalCreateForm
TransactionCreateForm
RoleCreateForm
RolePermissionManageForm
UserRoleManageForm
```

Example:

```razor
<EditForm Model="Model" OnValidSubmit="HandleSubmit" FormName="CardCreateForm">
```

Each form name must be unique within the app.

## Step 4: Add AntiforgeryToken Where Needed

For each `EditForm`, add:

```razor
<AntiforgeryToken />
```

near the top of the form body unless it already exists.

Do not add duplicate antiforgery tokens inside the same form.

## Step 5: Check Common Files

Likely files to review:

```text
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor
YbsSmartCardSystem.App/Components/Features/Card/CardCreate.razor
YbsSmartCardSystem.App/Components/Features/Package/PackageCreate.razor
YbsSmartCardSystem.App/Components/Features/TopUp/TopUpCreate.razor
YbsSmartCardSystem.App/Components/Features/Bus/BusCreate.razor
YbsSmartCardSystem.App/Components/Features/Terminal/TerminalCreate.razor
YbsSmartCardSystem.App/Components/Features/Transaction/TransactionCreate.razor
YbsSmartCardSystem.App/Components/Features/RolePermission/RoleCreate.razor
YbsSmartCardSystem.App/Components/Features/RolePermission/RolePermissionManage.razor
YbsSmartCardSystem.App/Components/Features/RolePermission/UserRoleManage.razor
```

Only update files that actually contain `EditForm`.

## Step 6: Verify

Run:

```powershell
dotnet build YbsSmartCardSystem.App/YbsSmartCardSystem.App.csproj -c Release --no-restore
```

Then run the app and test:

```text
Login form
Card create form
Package create form
TopUp create form
Bus/Terminal create forms
Transaction create form
Role/permission management forms
```

## Expected Result

- Login no longer throws the form-name POST error.
- All Blazor forms have unique `FormName` values.
- Form submissions continue to call the same existing handlers.
- No API or database behavior changes.

## Do Not Do

- Do not change authentication service logic.
- Do not change API login endpoint.
- Do not remove antiforgery setup.
- Do not rename routes.
- Do not change models or contracts.
