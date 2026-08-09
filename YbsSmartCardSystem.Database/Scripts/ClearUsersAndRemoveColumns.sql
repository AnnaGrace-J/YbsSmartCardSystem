USE [YbsSmartCardSystem];
GO

-- 1. Remove Columns from Tbl_StaffUser and Tbl_ViewerUser
IF COL_LENGTH('dbo.Tbl_StaffUser', 'FullName') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Tbl_StaffUser DROP COLUMN FullName;
END
GO

IF COL_LENGTH('dbo.Tbl_StaffUser', 'Email') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Tbl_StaffUser DROP COLUMN Email;
END
GO

IF COL_LENGTH('dbo.Tbl_ViewerUser', 'FullName') IS NOT NULL
BEGIN
    ALTER TABLE dbo.Tbl_ViewerUser DROP COLUMN FullName;
END
GO

-- 2. Clear all user data
DELETE FROM dbo.Tbl_UserRole;
DELETE FROM dbo.Tbl_StaffUser;
DELETE FROM dbo.Tbl_ViewerUser;
GO

-- 3. Reset Identity on tables
DBCC CHECKIDENT ('dbo.Tbl_StaffUser', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tbl_ViewerUser', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tbl_UserRole', RESEED, 0);
GO

-- 4. Create Admin Account
INSERT INTO dbo.Tbl_StaffUser (UserName, PhoneNo, PasswordHash, IsActive, CreatedDate, DeleteFlag)
VALUES ('Admin', '09979558847', 'AQAAAAIAAYagAAAAELF1hLK4Z03e1nmjbDEQK4iGgxjSc9XwFFWovg65J9pgL0RYt5wrezlPJxqBPI+0Fg==', 1, GETDATE(), 0);

DECLARE @AdminId INT = SCOPE_IDENTITY();
DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Tbl_Role WHERE RoleCode = 'ADMIN');

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.Tbl_UserRole (UserId, RoleId, CreatedDate, DeleteFlag)
    VALUES (@AdminId, @AdminRoleId, GETDATE(), 0);
END
GO

-- 5. Create Operator Account
INSERT INTO dbo.Tbl_StaffUser (UserName, PhoneNo, PasswordHash, IsActive, CreatedDate, DeleteFlag)
VALUES ('Operator', '09449693537', 'AQAAAAIAAYagAAAAEAjaqLSId3/KpRMK+h9NWp8jYTA50UZEy+9A3CBZmE0qJ6sV9y8AXWMcWHi7mQgroQ==', 1, GETDATE(), 0);

DECLARE @OperatorId INT = SCOPE_IDENTITY();
DECLARE @OperatorRoleId INT = (SELECT RoleId FROM dbo.Tbl_Role WHERE RoleCode = 'OPERATOR');

IF @OperatorRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.Tbl_UserRole (UserId, RoleId, CreatedDate, DeleteFlag)
    VALUES (@OperatorId, @OperatorRoleId, GETDATE(), 0);
END
GO
