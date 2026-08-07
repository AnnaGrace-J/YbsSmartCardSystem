# Phase 4 Implementation Plan: Database First Schema Update

## Goal

Update the SQL Server database schema so the maintained system can support:

- Package management
- Authentication
- Dynamic RBAC
- AuditLog

This phase updates the database only. EF Core scaffolding happens in Phase 5.

## Scope

Keep existing tables and behavior for:

```text
Tbl_Card
Tbl_Bus
Tbl_Terminal
Tbl_TopUp
Tbl_Transaction
```

Add or confirm tables for:

```text
Tbl_Package
Tbl_User
Tbl_Role
Tbl_Permission
Tbl_UserRole
Tbl_RolePermission
Tbl_AuditLog
```

## Important Rules

- SQL Server is the source of truth.
- Do not edit EF Core scaffolded files manually.
- Do not run `Scaffold-DbContext` in this phase.
- Do not modify C# services/controllers yet.
- Preserve existing data where possible.
- Use soft delete fields consistently.

## Pre-Work

### 1. Back Up The Database

Create a SQL Server backup before applying schema changes.

Recommended backup name:

```text
YbsSmartCard_PrePhase4_YYYYMMDD.bak
```

### 2. Inspect Existing Schema

Confirm the current schema for:

```text
Tbl_Card
Tbl_Bus
Tbl_Terminal
Tbl_TopUp
Tbl_Transaction
```

Check:

- Primary keys
- Foreign keys
- Unique indexes
- Soft delete columns
- Created/updated date columns
- Decimal precision for money fields

### 3. Decide Whether Package Already Exists

If a package table already exists, align it with the design below instead of creating a duplicate table.

## Table Design

### Tbl_Package

Purpose: store card package/pass definitions.

Suggested columns:

```sql
PackageId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
PackageCode NVARCHAR(50) NOT NULL,
PackageName NVARCHAR(100) NOT NULL,
Price DECIMAL(18,2) NOT NULL,
RideLimit INT NULL,
ValidDays INT NULL,
Description NVARCHAR(250) NULL,
IsActive BIT NOT NULL DEFAULT 1,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
UpdatedDate DATETIME NULL,
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested indexes:

```sql
UNIQUE PackageCode where DeleteFlag = 0
INDEX IsActive
```

If SQL Server filtered unique indexes are allowed:

```sql
CREATE UNIQUE INDEX UX_Tbl_Package_PackageCode_Active
ON Tbl_Package(PackageCode)
WHERE DeleteFlag = 0;
```

### Tbl_User

Purpose: store application users for login.

Suggested columns:

```sql
UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
UserName NVARCHAR(100) NOT NULL,
FullName NVARCHAR(150) NOT NULL,
Email NVARCHAR(150) NULL,
PasswordHash NVARCHAR(500) NOT NULL,
PasswordSalt NVARCHAR(500) NULL,
IsActive BIT NOT NULL DEFAULT 1,
LastLoginDate DATETIME NULL,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
UpdatedDate DATETIME NULL,
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested indexes:

```sql
UNIQUE UserName where DeleteFlag = 0
UNIQUE Email where Email is not null and DeleteFlag = 0
INDEX IsActive
```

### Tbl_Role

Purpose: store roles such as Admin, Operator, Viewer.

Suggested columns:

```sql
RoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
RoleCode NVARCHAR(50) NOT NULL,
RoleName NVARCHAR(100) NOT NULL,
Description NVARCHAR(250) NULL,
IsSystemRole BIT NOT NULL DEFAULT 0,
IsActive BIT NOT NULL DEFAULT 1,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
UpdatedDate DATETIME NULL,
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested indexes:

```sql
UNIQUE RoleCode where DeleteFlag = 0
```

### Tbl_Permission

Purpose: store permission records for Dynamic RBAC.

Suggested columns:

```sql
PermissionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
PermissionCode NVARCHAR(100) NOT NULL,
PermissionName NVARCHAR(150) NOT NULL,
FeatureName NVARCHAR(100) NOT NULL,
ActionName NVARCHAR(100) NOT NULL,
Description NVARCHAR(250) NULL,
IsActive BIT NOT NULL DEFAULT 1,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
UpdatedDate DATETIME NULL,
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested indexes:

```sql
UNIQUE PermissionCode where DeleteFlag = 0
INDEX FeatureName
INDEX IsActive
```

Recommended permission code format:

```text
Feature.Action
```

Examples:

```text
Card.View
Card.Create
Card.Update
Card.Delete
Package.View
Package.Create
Package.Update
Package.Delete
TopUp.Create
Transaction.View
BusPayment.Create
RolePermission.Manage
AuditLog.View
```

### Tbl_UserRole

Purpose: map users to roles.

Suggested columns:

```sql
UserRoleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
UserId INT NOT NULL,
RoleId INT NOT NULL,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested relationships:

```sql
UserId -> Tbl_User.UserId
RoleId -> Tbl_Role.RoleId
```

Suggested indexes:

```sql
UNIQUE UserId + RoleId where DeleteFlag = 0
INDEX UserId
INDEX RoleId
```

### Tbl_RolePermission

Purpose: map roles to permissions.

Suggested columns:

```sql
RolePermissionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
RoleId INT NOT NULL,
PermissionId INT NOT NULL,
CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
DeleteFlag BIT NOT NULL DEFAULT 0
```

Suggested relationships:

```sql
RoleId -> Tbl_Role.RoleId
PermissionId -> Tbl_Permission.PermissionId
```

Suggested indexes:

```sql
UNIQUE RoleId + PermissionId where DeleteFlag = 0
INDEX RoleId
INDEX PermissionId
```

### Tbl_AuditLog

Purpose: record important system actions.

Suggested columns:

```sql
AuditLogId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
UserId INT NULL,
Action NVARCHAR(100) NOT NULL,
FeatureName NVARCHAR(100) NOT NULL,
EntityName NVARCHAR(100) NULL,
EntityId NVARCHAR(100) NULL,
OldValue NVARCHAR(MAX) NULL,
NewValue NVARCHAR(MAX) NULL,
IpAddress NVARCHAR(100) NULL,
UserAgent NVARCHAR(500) NULL,
CreatedDateTime DATETIME NOT NULL DEFAULT GETDATE()
```

Suggested relationship:

```sql
UserId -> Tbl_User.UserId nullable
```

Suggested indexes:

```sql
INDEX UserId
INDEX Action
INDEX FeatureName
INDEX CreatedDateTime
```

## Seed Data

Add initial roles:

```text
Admin
Operator
Viewer
```

Add initial permissions:

```text
Card.View
Card.Create
Card.Update
Card.Delete
Package.View
Package.Create
Package.Update
Package.Delete
TopUp.View
TopUp.Create
BusPayment.Create
Transaction.View
RolePermission.View
RolePermission.Manage
AuditLog.View
```

Assign all permissions to `Admin`.

Create one initial admin user only if the project has an agreed secure password hashing approach.

If password hashing is not implemented yet, do not seed a real password. Instead, document the admin creation step for Phase 7.

## Migration Script

Create a SQL script file:

```text
YbsSmartCardSystem.Database/Scripts/Phase4_AddPackageAuthRbacAuditLog.sql
```

The script should:

- Use `IF OBJECT_ID(...) IS NULL` guards before creating tables.
- Use named constraints.
- Use named indexes.
- Avoid dropping existing tables.
- Avoid destructive changes to existing columns.
- Be safe to review before execution.

Example pattern:

```sql
IF OBJECT_ID('dbo.Tbl_Role', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_Role
    (
        RoleId INT IDENTITY(1,1) NOT NULL,
        RoleCode NVARCHAR(50) NOT NULL,
        RoleName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NULL,
        IsSystemRole BIT NOT NULL CONSTRAINT DF_Tbl_Role_IsSystemRole DEFAULT 0,
        IsActive BIT NOT NULL CONSTRAINT DF_Tbl_Role_IsActive DEFAULT 1,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_Role_CreatedDate DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_Role_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_Role PRIMARY KEY (RoleId)
    );
END;
```

## Existing Table Review

Review whether these existing tables need small non-destructive updates:

### Tbl_Card

Confirm columns:

```text
CardId
CardNum
OwnerName
MobileNo
Balance
CreatedDate
UpdatedDate
DeleteFlag
```

### Tbl_TopUp

Confirm columns:

```text
TopUpId
TopUpNo
CardId
Amount
TopUpDate
Remark
DeleteFlag
```

### Tbl_Transaction

Confirm columns:

```text
TransactionId
TransactionNo
CardId
TerminalId
Amount
TransactionDate
DeleteFlag
```

### Tbl_Bus and Tbl_Terminal

Confirm the Bus/Terminal relationship is correct and supports bus tap payment.

Also check why both `TblBu.cs` and `TblBus.cs` currently exist after scaffolding. This may indicate a naming/scaffold issue that should be corrected during Phase 5.

## Do Not Do In Phase 4

- Do not run EF Core scaffolding.
- Do not edit generated C# database models.
- Do not update Domain services.
- Do not update API controllers.
- Do not update Blazor pages.
- Do not implement JWT.
- Do not implement permission checks.
- Do not implement audit writing.
- Do not store plain text passwords.
- Do not make destructive database changes without a backup.

## Verification

After applying the SQL script to a development database, verify with SQL queries:

```sql
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN
(
    'Tbl_Package',
    'Tbl_User',
    'Tbl_Role',
    'Tbl_Permission',
    'Tbl_UserRole',
    'Tbl_RolePermission',
    'Tbl_AuditLog'
);
```

Verify foreign keys:

```sql
SELECT name
FROM sys.foreign_keys
WHERE parent_object_id IN
(
    OBJECT_ID('dbo.Tbl_UserRole'),
    OBJECT_ID('dbo.Tbl_RolePermission'),
    OBJECT_ID('dbo.Tbl_AuditLog')
);
```

Verify seed permissions:

```sql
SELECT PermissionCode, FeatureName, ActionName
FROM dbo.Tbl_Permission
WHERE DeleteFlag = 0
ORDER BY FeatureName, ActionName;
```

## Expected Result

- Database schema supports Package, Auth, Dynamic RBAC, and AuditLog.
- Existing business tables remain intact.
- Database is ready for Phase 5 EF Core scaffolding.
- No C# runtime behavior has changed yet.

## Git Milestone

```text
db: update schema for auth rbac and audit log
```
