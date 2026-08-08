SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE [YbsSmartCard]
GO

IF NOT EXISTS (SELECT 1 FROM Tbl_Permission WHERE PermissionCode = 'Card.Register')
BEGIN
    INSERT INTO Tbl_Permission (PermissionCode, PermissionName, FeatureName, ActionName, Description, IsActive, CreatedDate, DeleteFlag)
    VALUES ('Card.Register', 'Register Cards', 'Card', 'Register', 'Ability to register new cards with OTP', 1, GETDATE(), 0)
END

DECLARE @PermId INT = (SELECT PermissionId FROM Tbl_Permission WHERE PermissionCode = 'Card.Register');
DECLARE @AdminRoleId INT = (SELECT RoleId FROM Tbl_Role WHERE RoleCode = 'Admin');
DECLARE @OperatorRoleId INT = (SELECT RoleId FROM Tbl_Role WHERE RoleCode = 'Operator');

-- Add to Admin
IF @AdminRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Tbl_RolePermission WHERE RoleId = @AdminRoleId AND PermissionId = @PermId)
BEGIN
    INSERT INTO Tbl_RolePermission (RoleId, PermissionId, CreatedDate, DeleteFlag)
    VALUES (@AdminRoleId, @PermId, GETDATE(), 0)
END

-- Add to Operator
IF @OperatorRoleId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Tbl_RolePermission WHERE RoleId = @OperatorRoleId AND PermissionId = @PermId)
BEGIN
    INSERT INTO Tbl_RolePermission (RoleId, PermissionId, CreatedDate, DeleteFlag)
    VALUES (@OperatorRoleId, @PermId, GETDATE(), 0)
END
GO
