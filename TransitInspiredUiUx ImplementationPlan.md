# Transit-Inspired UI/UX Implementation Plan

## Goal

Refresh the YBS Smart Card System UI using the provided TransitPay-style screens as inspiration.

The design should feel cleaner, more modern, and more transit/payment focused, but it must not change the project's existing workflows just to copy the screenshots.

## Design Inspiration Summary

Take inspiration from:

- Left sidebar navigation
- Clean light background
- Strong blue primary color
- Mint/turquoise active navigation state
- Card-style dashboard panels
- Large balance/card summary display
- Recent transactions/trips panel
- Compact tables with filters
- Payment card visual treatment
- Quick action tiles
- Read-only commuter/viewer dashboard style
- Clear separation between navigation, page title, and page content

Do not copy exact text, data, routes, or unrelated actions like `Buy Pass` if the feature does not exist.

## Core Rule

Do not change business workflows to match the design.

The UI must support the current YBS workflows:

```text
Login
Register with OTP
Dashboard/profile
Card registration with OTP
One card per phone number
Card list/details
TopUp
Bus payment/tap
Transaction history
Bus list
Terminal list
Role/permission management
Audit log
```

If Package feature is removed, do not show package/buy-pass UI.

## Visual Direction

### Color Palette

Use a restrained transit/payment palette:

```text
Primary blue: deep YBS/Transit blue
Accent mint: active nav and positive status
Dark navy: headings and important numbers
Soft gray/lavender: page background and sidebar background
White: cards and panels
Red/rose: destructive or warning actions
```

Avoid turning the entire app into a single blue-only theme. Use mint and neutral colors to balance the UI.

### Typography

Use a clean sans-serif font already present or configured:

```text
Inter
system-ui
Segoe UI
Arial
```

Recommended hierarchy:

```text
Page title: 28-36px
Section title: 20-24px
Card labels: 12-14px
Body/table text: 14-16px
Important balance/value: 32-48px
```

Do not use oversized marketing hero typography inside dashboard pages.

### Layout Style

Use:

```text
Fixed left sidebar on desktop
Top header/content title area
Main content area with max-width where needed
Dashboard grids
Cards/panels with 8px or 12px radius
Subtle borders and shadows
```

Avoid:

```text
Nested cards inside cards
Marketing landing-page hero sections
Decorative gradient blobs
Overly rounded pill-heavy UI everywhere
```

## Application Shell

Update the main app shell inspired by the screenshots.

### Sidebar

Sidebar should include:

```text
YBS Smart Card / TransitPay-style brand area
Current user summary
Permission-aware navigation links
Logout
Optional quick tap/payment shortcut for staff if allowed
```

Navigation should remain permission-based:

```text
Viewer: Dashboard, My Card, Bus List, Terminal List, History if allowed, Logout
Operator: Card Registration, Card List, TopUp, Bus Payment, Transactions, Bus/Terminal view, Logout
Admin: All management pages including RBAC and AuditLog
```

Do not show links the current user cannot use.

### Header

Header should show context-aware summary:

```text
Page title
Current card balance if viewer has a card
Notification icon placeholder optional
User avatar/initials optional
```

Keep header simple.

## Dashboard Design

Create separate dashboard experiences by user type.

### Viewer Dashboard

Inspired by the overview screen.

Show:

```text
Primary card balance panel
Linked card number
Owner name
Phone number
Recent transactions
Bus list shortcut
Terminal list shortcut
No card state if user has no card
```

If viewer has one card:

```text
Show one large card visual
Show balance
Show recent transactions
```

If viewer has no card:

```text
Show clear empty state:
"No card is registered for your phone number yet."
```

Do not add actions like Add Funds or Buy Pass unless the backend workflow exists and the viewer has permission.

### Admin/Operator Dashboard

Show operational summary:

```text
Total cards
Today's card registrations
Today's topups
Today's bus payments
Recent transactions
Quick actions based on permissions
```

Quick actions may include:

```text
Register Card
TopUp
Bus Payment
View Transactions
```

Do not show viewer-only commuter UI as-is for admin/operator.

## Card UI

Use the card visual from the screenshots as inspiration.

### Viewer My Card Page

Show:

```text
Large physical-card-style panel
Card number masked except last 4 digits
Balance
Owner name
Status
Phone number
Recent transactions or topups
```

Do not include actions like:

```text
Rename Card
Report Lost
Set Auto-Reload
Add Funds
```

unless those workflows actually exist.

### Staff Card List Page

Keep existing staff workflow, but improve presentation:

```text
Search/filter area
Table with card number, owner, phone, balance, status
Create/Register Card button if Card.Register permission
Clear edit/delete actions if allowed
```

### Card Registration Page

Preserve current OTP workflow:

```text
Phone number
Send OTP
OTP code
Owner name
Create card
Generated card number result
```

Design it as a focused stepper-style card:

```text
Step 1: Verify phone
Step 2: Enter OTP
Step 3: Create card
```

## Transaction/History UI

Inspired by the payment history screen.

Update transaction pages with:

```text
Page title and short subtitle
Search input
Type/date filters
Clean table
Debit/credit style badges
Pagination
Amount aligned right
```

For YBS:

```text
TopUp = Credit
Bus payment = Debit
```

Optional summary panel:

```text
Total spent this month
Total topup this month
Recent activity count
```

Do not create fake chart data. Only show charts if data is available.

## Bus Payment / Tap Simulation UI

Inspired by the tap-to-pay screen.

Keep existing bus payment workflow.

Design:

```text
Centered payment simulation panel
Large circular tap target
Terminal/card fields if required by current workflow
Primary action button
Result state: success, insufficient balance, terminal inactive
```

Do not make the UI only decorative. It must still collect whatever the existing bus payment API requires.

## Bus And Terminal Viewer UI

Since viewers can view Bus and Terminal lists:

### Viewer Bus List

Read-only:

```text
Bus number
License
Status if available
Search
Pagination
```

Hide:

```text
Create
Edit
Delete
```

### Viewer Terminal List

Read-only:

```text
Terminal serial number
Assigned bus
Active status
Search
Pagination
```

Hide staff-only actions.

## Role-Based UI Rules

Use existing auth state and permissions.

UI should check:

```text
UserType
Permissions
Roles
```

Examples:

```text
Card.Register -> show card registration
Card.View -> show card list
TopUp.Create -> show topup action
BusPayment.Create -> show tap/payment page
Bus.View -> show bus list
Terminal.View -> show terminal list
RolePermission.View -> show roles/permissions
AuditLog.View -> show audit log
```

Viewer should never see staff-only management actions.

## Component Structure

Recommended reusable components:

```text
Components/Shared/AppShell
Components/Shared/PageHeader
Components/Shared/StatCard
Components/Shared/BalanceCard
Components/Shared/DataTable
Components/Shared/EmptyState
Components/Shared/PermissionView
Components/Shared/StatusBadge
Components/Shared/LoadingState
```

Keep these small. Do not build a large design system before the app needs it.

## Styling Plan

Use Tailwind if already configured.

Recommended utility patterns:

```text
bg-slate-50 or bg-indigo-50 for page background
bg-white border border-slate-200 for panels
text-slate-950 for headings
text-slate-500 for secondary text
bg-blue-700 for primary buttons/cards
bg-teal-300 or bg-cyan-300 for active nav
rounded-lg or rounded-xl
shadow-sm for subtle depth
```

Avoid:

```text
Overusing gradients
Huge rounded 3xl panels everywhere
Dark full-app dashboard unless specifically needed
Changing workflow to fit quick-action cards
```

## Page-by-Page Update Plan

### 1. Main Layout

Update:

```text
MainLayout.razor
NavMenu.razor
MainLayout.razor.css
NavMenu.razor.css
```

Target:

```text
Desktop left sidebar
Responsive mobile navigation
User summary
Permission-aware nav items
Clean header area
```

### 2. Login/Register

Keep current forms.

Improve:

```text
Centered auth panel
Clear field labels
Good error state
Loading state
OTP step presentation
```

### 3. Viewer Dashboard

Update:

```text
UserDashboard.razor
```

Target:

```text
Balance card
Card summary
Recent transactions
Bus/Terminal shortcuts
No-card empty state
```

### 4. Card Pages

Update:

```text
CardList.razor
CardCreate.razor
```

Target:

```text
Staff table layout
Step-based card registration
Card-number generation result panel
```

### 5. TopUp Pages

Update:

```text
TopUpCreate.razor
TopUpList.razor
```

Target:

```text
Clear topup form
History table with credit badges
Search/filter if supported
```

### 6. Transaction Pages

Update:

```text
TransactionCreate.razor
TransactionList.razor
```

Target:

```text
Tap-to-pay style simulation
History table with debit badges
Result feedback states
```

### 7. Bus/Terminal Pages

Update:

```text
BusList.razor
TerminalList.razor
```

Target:

```text
Read-only mode for Viewer
Management mode for Admin/Operator
```

### 8. Admin Pages

Update:

```text
RolePermission pages
AuditLog pages
```

Target:

```text
Operational tables
Filters
Clear action buttons
Compact, admin-friendly layout
```

## Responsive Behavior

Desktop:

```text
Fixed sidebar
Main content grid
Right-side panels where useful
```

Tablet:

```text
Sidebar can collapse
Cards stack into 2 columns
```

Mobile:

```text
Top nav or collapsible menu
Single-column layout
Tables become scrollable
Buttons remain full-width where needed
```

## Accessibility Requirements

Ensure:

```text
Buttons have text labels
Inputs have labels
Contrast is readable
Focus states are visible
Tables have headers
Icons are not the only source of meaning
Loading and error states are visible
```

## Do Not Do

- Do not add Package/Buy Pass UI because Package is being removed.
- Do not add Add Funds unless it maps to existing TopUp workflow.
- Do not add QR code unless it exists in the business requirements.
- Do not change routes just for design.
- Do not expose staff actions to Viewer.
- Do not hide API security behind UI only.
- Do not use fake data in production UI.
- Do not remove existing working workflows.

## Verification Checklist

Visual:

```text
Sidebar looks consistent
Active nav is clear
Cards/panels have consistent spacing
Text does not overlap
Tables are readable
Mobile layout works
```

Workflow:

```text
Viewer login works
Viewer dashboard shows own card
Viewer can view Bus list
Viewer can view Terminal list
Viewer cannot register card
Admin/Operator can register card
Card registration OTP still works
TopUp still works
Bus payment still works
Transaction history still works
RBAC pages still work for Admin
AuditLog still works for Admin
```

Build:

```powershell
dotnet build -c Release
npm.cmd run build:css
```

## Expected Result

The app should feel like a modern transit smart-card dashboard inspired by the screenshots, while still preserving the real YBS workflows, permissions, and business rules.

The design should improve clarity and usability without forcing unsupported features into the system.
