# Phase 11 Implementation Plan: Frontend Maintenance and Tailwind Cleanup

## Goal

Modernize and organize the Blazor frontend while preserving existing workflows.

This phase improves UI structure, authentication state, permission-aware navigation, API communication, and styling consistency.

## Scope

Update:

- Blazor feature folders
- Navigation
- API service usage
- Authentication/token handling
- Permission-aware UI visibility
- Styling foundation with Tailwind CSS

Do not change backend business rules in this phase.

## Prerequisites

Phases 6 through 10 should be complete.

Confirm these features exist in the API:

```text
Package
Auth
RolePermission
Dynamic RBAC
AuditLog
```

Confirm Blazor can call authenticated API endpoints with JWT.

## Step 1: Review Current Blazor Structure

Review:

```text
YbsSmartCardSystem.App/Components/Features
YbsSmartCardSystem.App/Components/Layout
YbsSmartCardSystem.App/Components/Pages
YbsSmartCardSystem.App/Services
YbsSmartCardSystem.App/wwwroot
```

Identify and remove unused template pages only if they are no longer routed or needed:

```text
Counter.razor
Weather.razor
```

Do not remove `Home.razor` or `Error.razor` unless replacement pages already exist.

## Step 2: Standardize Feature Folder Names

Target feature folders:

```text
Components/Features/Card
Components/Features/Package
Components/Features/TopUp
Components/Features/BusPayment
Components/Features/Transaction
Components/Features/Auth
Components/Features/RolePermission
Components/Features/AuditLog
```

Current `Bus` and `Terminal` pages may remain if they are still separate maintenance screens.

If keeping them, document that:

```text
Bus + Terminal + Transaction currently support the BusPayment workflow.
```

Do not break existing routes during this cleanup.

## Step 3: Clean ApiService

Review:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Refactor only enough to reduce duplication and improve reliability.

Recommended improvements:

- Use a named HttpClient if already configured.
- Centralize base address setup.
- Centralize JWT bearer token attachment.
- Centralize JSON response handling.
- Keep existing public methods so pages do not break.

Do not rewrite all API calls unless there is test coverage or clear benefit.

## Step 4: Improve Auth State Service

Create or update:

```text
YbsSmartCardSystem.App/Services/AuthStateService.cs
```

It should track:

```text
Token
UserId
UserName
FullName
Roles
Permissions
IsAuthenticated
```

Expose helpers:

```csharp
bool HasPermission(string permissionCode)
bool IsInRole(string roleCode)
void SetLogin(...)
void Logout()
```

For this phase, in-memory token storage is acceptable.

Persistent browser storage can be added later if required.

## Step 5: Make Navigation Permission-Aware

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

Show links based on permissions:

```text
Cards -> Card.View
Packages -> Package.View
TopUp -> TopUp.View or TopUp.Create
Bus Payment -> BusPayment.Create
Transactions -> Transaction.View
Roles/Permissions -> RolePermission.View
Audit Log -> AuditLog.View
```

Always remember:

```text
UI hiding is convenience only. API RBAC remains the real security boundary.
```

## Step 6: Add Login/Logout UI Polish

Update Auth pages:

```text
Components/Features/Auth/Login.razor
Components/Features/Auth/Login.razor.cs
```

Minimum behavior:

- Disable submit while login is running
- Show invalid login message
- Store token and permissions after login
- Redirect to the main dashboard/list page after login
- Provide logout action in layout/navigation

Do not expose JWT token in visible UI.

## Step 7: Standardize Page States

For each feature list/create page, standardize:

```text
Loading state
Empty state
Validation errors
API error message
Success message
Disabled submit while saving
Pagination controls
Delete confirmation
```

Apply to:

```text
Card
Package
TopUp
Bus/Terminal or BusPayment
Transaction
RolePermission
AuditLog
```

## Step 8: Configure Tailwind CSS

Add Tailwind to the Blazor app.

Recommended files:

```text
YbsSmartCardSystem.App/package.json
YbsSmartCardSystem.App/tailwind.config.js
YbsSmartCardSystem.App/Styles/app.css
```

Configure Tailwind content paths for Razor files:

```js
content: [
  "./Components/**/*.{razor,cshtml}",
  "./Pages/**/*.{razor,cshtml}"
]
```

Build output should go to:

```text
YbsSmartCardSystem.App/wwwroot/app.css
```

or another agreed CSS file referenced by `App.razor`.

If Bootstrap is still required by existing layout, keep it temporarily. Do not remove Bootstrap until all affected UI has been verified.

## Step 9: Apply Tailwind Gradually

Start with shared layout and new/maintained pages:

```text
MainLayout
NavMenu
Login
Package
RolePermission
AuditLog
```

Use a quiet operational UI style:

- Clear tables
- Compact forms
- Consistent buttons
- Consistent spacing
- Good contrast
- No decorative landing page
- No large marketing hero sections

Keep the app workflow-first.

## Step 10: Add Shared UI Helpers If Useful

Only if duplication becomes obvious, add small reusable components:

```text
Components/Shared/LoadingState.razor
Components/Shared/ErrorMessage.razor
Components/Shared/PaginationControls.razor
Components/Shared/ConfirmDialog.razor
```

Do not create a large component framework in this phase.

## Step 11: Remove Template/Unused Assets Carefully

After pages are verified, remove unused template content:

```text
Weather page links
Counter page links
Unused Bootstrap icons/classes if no longer used
Unused CSS rules
```

Before deleting Bootstrap files, confirm no layout/page still depends on them.

## Step 12: Manual UI Verification

Run the API and Blazor app.

Verify:

```text
Login works
Logout works
Unauthorized users cannot use protected pages
Navigation changes based on permissions
Card workflow still works
Package workflow works
TopUp workflow works
Bus payment/transaction workflow works
RolePermission management works
AuditLog list works
```

Check desktop and mobile widths.

Make sure text does not overlap or overflow buttons, tables, or forms.

## Do Not Do In Phase 11

- Do not change backend business rules.
- Do not change API routes.
- Do not weaken RBAC checks.
- Do not store JWT tokens insecurely in visible UI.
- Do not remove Bootstrap before verifying replacement styling.
- Do not redesign the app as a marketing site.
- Do not add unrelated features.
- Do not introduce a new frontend framework.

## Verification

Run:

```powershell
dotnet restore
dotnet build
```

If Tailwind tooling is added, run the CSS build command defined in `package.json`.

Run:

```powershell
rg "Counter" YbsSmartCardSystem.App/Components
rg "Weather" YbsSmartCardSystem.App/Components
rg "HasPermission"
rg "AuditLog.View"
rg "Package.View"
```

Expected:

- No unwanted template navigation remains.
- Permission-aware navigation exists.
- Auth state service is used by navigation and API calls.
- Existing workflows still compile.

## Expected Result

- Blazor app is organized by feature.
- Login/logout experience is usable.
- Navigation reflects user permissions.
- Tailwind styling foundation is configured.
- Existing business workflows remain stable.
- Project is ready for Phase 12: deployment preparation.

## Git Milestone

```text
feat: update blazor frontend
```
