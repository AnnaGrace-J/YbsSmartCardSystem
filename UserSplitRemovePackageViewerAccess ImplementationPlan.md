# User Split, One Card Per Phone, Remove Package, and Viewer Bus/Terminal Access Implementation Plan

## Goal

Update the system rules and structure:

- One phone number can have only one card.
- Split users into two database tables:
  - Staff users: Admin and Operator
  - Viewer users: normal registered users
- Remove the Package feature from the system.
- Allow Viewer users to view Bus and Terminal lists only.

Do not implement code in this planning phase.

## Target Business Rules

### Card Rule

```text
One phone number = one card
```

If a card already exists for a phone number, the system must block another card registration for that phone number.

This applies to:

```text
Admin card registration
Operator card registration
Any future card registration flow
```

### User Rule

Use separate tables for different user types:

```text
Tbl_StaffUser
Tbl_ViewerUser
```

Staff users are:

```text
Admin
Operator
```

Viewer users are:

```text
Self-registered public users
```

### Feature Removal Rule

Remove Package from:

```text
Database
Contracts
Domain
API
Blazor App
Permissions
Navigation
Seeds
Plans/docs where practical
```

### Viewer Access Rule

Viewer users can view:

```text
Own dashboard/profile
Own card information
Bus list
Terminal list
```

Viewer users cannot:

```text
Create cards
Update cards
Delete cards
Top up
Create bus payment
Manage buses
Manage terminals
Manage roles/permissions
View audit logs
Access admin/operator workflows
```

## Phase 1: Confirm Final Database Design

Before changing code, confirm the database table design.

### Staff User Table

Create:

```text
Tbl_StaffUser
```

Suggested columns:

```text
StaffUserId INT IDENTITY(1,1) PRIMARY KEY
UserName NVARCHAR(100) NOT NULL
FullName NVARCHAR(150) NOT NULL
PhoneNo NVARCHAR(20) NULL
Email NVARCHAR(150) NULL
PasswordHash NVARCHAR(500) NOT NULL
IsActive BIT NOT NULL DEFAULT 1
LastLoginDate DATETIME NULL
CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
UpdatedDate DATETIME NULL
DeleteFlag BIT NOT NULL DEFAULT 0
```

Recommended indexes:

```text
Unique active UserName
Unique active PhoneNo where PhoneNo is not null
Unique active Email where Email is not null
```

### Viewer User Table

Create:

```text
Tbl_ViewerUser
```

Suggested columns:

```text
ViewerUserId INT IDENTITY(1,1) PRIMARY KEY
UserName NVARCHAR(100) NOT NULL
PhoneNo NVARCHAR(20) NOT NULL
PasswordHash NVARCHAR(500) NOT NULL
IsActive BIT NOT NULL DEFAULT 1
LastLoginDate DATETIME NULL
CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
UpdatedDate DATETIME NULL
DeleteFlag BIT NOT NULL DEFAULT 0
```

Recommended indexes:

```text
Unique active UserName
Unique active PhoneNo
```

## Phase 2: Decide RBAC Mapping

The current RBAC tables probably reference `Tbl_User`.

Because users are split, choose one of these designs.

### Recommended Design: Staff Users Use RBAC, Viewers Use Fixed Permissions

Keep RBAC for:

```text
Tbl_StaffUser
Admin
Operator
```

Viewer users do not need full dynamic RBAC. They get fixed viewer access:

```text
ViewerDashboard.View
Bus.View
Terminal.View
```

Pros:

- Simpler
- Cleaner separation
- Staff permissions stay flexible
- Viewer behavior is predictable

Cons:

- If viewer permissions need to become dynamic later, additional mapping tables may be needed.

### Alternative Design: Separate UserRole Tables

Create:

```text
Tbl_StaffUserRole
Tbl_ViewerUserRole
```

Use both with `Tbl_Role`.

This is more flexible but more complex.

Recommended for this project: use the simpler staff-RBAC plus fixed-viewer-access approach unless there is a real requirement for dynamic viewer roles.

## Phase 3: Database Migration

Create a SQL script:

```text
YbsSmartCardSystem.Database/Scripts/SplitUsersRemovePackageOneCardPerPhone.sql
```

The script should:

- Create `Tbl_StaffUser`.
- Create `Tbl_ViewerUser`.
- Migrate existing Admin/Operator users from `Tbl_User` to `Tbl_StaffUser`.
- Migrate existing Viewer users from `Tbl_User` to `Tbl_ViewerUser`.
- Preserve password hashes.
- Preserve active/delete flags.
- Preserve created dates where possible.
- Update or recreate role assignment mappings for staff users.
- Add unique filtered index for one active card per phone number.
- Remove or disable Package tables and seed data.

### One Card Per Phone Index

Add a filtered unique index:

```sql
CREATE UNIQUE INDEX UX_Tbl_Card_MobileNo_Active
ON dbo.Tbl_Card(MobileNo)
WHERE DeleteFlag = 0 AND MobileNo IS NOT NULL;
```

Before adding this index, find duplicates:

```sql
SELECT MobileNo, COUNT(*) AS CardCount
FROM dbo.Tbl_Card
WHERE DeleteFlag = 0 AND MobileNo IS NOT NULL
GROUP BY MobileNo
HAVING COUNT(*) > 1;
```

Do not apply the unique index until duplicates are resolved.

## Phase 4: Remove Package Database Objects

If package data is no longer needed:

```text
Tbl_Package
Package permissions
Package audit action constants
Package seed data
```

Recommended safer approach:

- First remove Package from code/UI.
- Leave `Tbl_Package` in database temporarily.
- Drop the table only after backup and confirmation.

Do not drop package data without a backup.

## Phase 5: Scaffold EF Models

After schema changes, re-run EF Core Database First scaffold.

Expected generated models:

```text
TblStaffUser
TblViewerUser
AppDbContext
```

Expected removed or unused model:

```text
TblPackage
```

Do not manually edit scaffolded files.

## Phase 6: Update Auth Design

Login should support two user types.

### Staff Login

Admin and Operator can log in using:

```text
Phone number or username
Password
```

or keep current phone-only login if preferred.

Staff users get:

```text
UserType = Staff
StaffUserId
Roles
Permissions
```

### Viewer Login

Viewer users log in using:

```text
Phone number
Password
```

Viewer users get:

```text
UserType = Viewer
ViewerUserId
Fixed viewer permissions
```

JWT should include:

```text
UserId
UserType
UserName
PhoneNumber
Roles if staff
```

Important: Staff IDs and Viewer IDs may overlap, so `UserType` must be included in the JWT and current-user service.

## Phase 7: Update Current User Service

Update current user abstraction to expose:

```csharp
int? UserId { get; }
string? UserType { get; }
string? UserName { get; }
string? PhoneNumber { get; }
bool IsStaff { get; }
bool IsViewer { get; }
```

Use `UserType` to avoid mixing staff and viewer records.

## Phase 8: Update Permissions

Remove Package permissions:

```text
Package.View
Package.Create
Package.Update
Package.Delete
```

Add or confirm viewer-readable permissions:

```text
Bus.View
Terminal.View
ViewerDashboard.View
```

Recommended access:

```text
Admin -> all staff permissions
Operator -> Card.Register, Card.View, Bus.View, Terminal.View, Transaction.View, TopUp as needed
Viewer -> Bus.View, Terminal.View, ViewerDashboard.View only
```

If viewers use fixed permissions, these do not need to be stored in `Tbl_RolePermission`; they can be returned by AuthService for viewer users.

## Phase 9: Update Card Registration

Update card creation validation:

```text
MobileNo is required
MobileNo must be OTP verified
MobileNo must not already have an active card
```

Before creating a new card:

```csharp
Any active Tbl_Card where MobileNo == request.MobileNo
```

If exists:

```text
Return 409 Conflict
Message: A card already exists for this phone number.
```

Keep the database unique index as the final safety net.

## Phase 10: Update Viewer Dashboard

Viewer dashboard should load cards by:

```text
current viewer user's PhoneNo
```

Because one phone number can only have one card, the dashboard can show:

```text
No card found
or
One card summary
```

Optional sections:

```text
Recent transactions for that card
Recent topups for that card
Bus list
Terminal list
```

Do not allow viewer to pass arbitrary phone number in query string.

## Phase 11: Remove Package Code

Remove or disable:

```text
YbsSmartCardSystem.Contracts/Features/Package
YbsSmartCardSystem.Domain/Features/Package
YbsSmartCardSystem.Api/Controllers/PackageController.cs
YbsSmartCardSystem.App/Components/Features/Package
Package methods in ApiService
Package links in NavMenu
Package permission constants
Package audit action constants
Package plan references where practical
```

After removal:

```powershell
rg "Package"
```

Review remaining references. Keep only historical docs if desired.

## Phase 12: Update Bus and Terminal Access

Bus list endpoint:

```text
GET /api/Bus
GET /api/Bus/{id}
```

Terminal list endpoint:

```text
GET /api/Terminal
GET /api/Terminal/{id}
```

Allow:

```text
Admin
Operator
Viewer
```

Only staff should be able to:

```text
POST /api/Bus
PATCH /api/Bus/{id}
DELETE /api/Bus/{id}
POST /api/Terminal
PATCH /api/Terminal/{id}
DELETE /api/Terminal/{id}
```

Viewer UI:

- Show Bus list link.
- Show Terminal list link.
- Hide create/edit/delete buttons.

## Phase 13: Update Blazor UI

Remove:

```text
Package navigation
Package pages
Package create/list buttons
```

Update viewer navigation:

```text
Dashboard
Bus List
Terminal List
Logout
```

Update staff navigation:

```text
Based on permissions
```

For Bus/Terminal pages:

- If current user is Viewer, render read-only table.
- If Admin/Operator, show existing create/edit/delete actions based on permissions.

## Phase 14: Update AuditLog

Audit logs should identify user type:

```text
Staff
Viewer
```

If current `Tbl_AuditLog.UserId` points to old `Tbl_User`, decide one:

### Option A: Add UserType

Add:

```text
UserType NVARCHAR(20) NULL
```

Keep `UserId` as numeric ID and use `UserType` to interpret it.

### Option B: Add Separate Columns

Add:

```text
StaffUserId INT NULL
ViewerUserId INT NULL
```

Recommended: Option A is simpler for this maintenance project.

## Phase 15: Testing Checklist

Database:

```text
Duplicate active card phone numbers are detected before unique index
Unique card phone index works
Staff users migrated correctly
Viewer users migrated correctly
Package references removed or disabled
```

Auth:

```text
Admin login works
Operator login works
Viewer login works
JWT contains UserType
Viewer cannot access staff-only endpoints
```

Card:

```text
Admin can create first card for phone number
Operator can create first card for phone number
Second card for same phone number returns 409
Viewer cannot create card
```

Viewer:

```text
Viewer can view dashboard
Viewer can view own card
Viewer can view bus list
Viewer can view terminal list
Viewer cannot create/edit/delete bus
Viewer cannot create/edit/delete terminal
Viewer cannot access card registration
Viewer cannot access package pages because package is removed
```

Package removal:

```text
PackageController removed
PackageService removed
Package pages removed
Package navigation removed
Package permissions removed
Build has no package compile references
```

## Recommended Implementation Order

1. Backup database.
2. Detect and resolve duplicate active card phone numbers.
3. Add unique active card phone index.
4. Add `Tbl_StaffUser` and `Tbl_ViewerUser`.
5. Migrate existing users.
6. Adjust RBAC mappings for staff users.
7. Scaffold EF models.
8. Update AuthService/JWT/CurrentUserService for `UserType`.
9. Update card creation one-card-per-phone validation.
10. Remove Package backend code.
11. Remove Package frontend code.
12. Update Bus/Terminal viewer access.
13. Update dashboard/profile.
14. Build and smoke test.

## Expected Result

- Each phone number can have only one active card.
- Admin and Operator users live in staff user table.
- Viewer users live in viewer user table.
- Package feature is removed.
- Viewers can view Bus and Terminal lists.
- Viewers cannot perform staff-only actions.
- Existing Admin and Operator workflows continue to work.
