SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Seed Roles
IF NOT EXISTS (SELECT 1 FROM Tbl_Role WHERE RoleCode = 'Admin')
BEGIN
    INSERT INTO Tbl_Role (RoleCode, RoleName, Description, IsActive, CreatedDate, DeleteFlag)
    VALUES ('Admin', 'Administrator', 'Full system control', 1, GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Tbl_Role WHERE RoleCode = 'Operator')
BEGIN
    INSERT INTO Tbl_Role (RoleCode, RoleName, Description, IsActive, CreatedDate, DeleteFlag)
    VALUES ('Operator', 'Operator', 'Perform basic smart card operations', 1, GETDATE(), 0);
END

IF NOT EXISTS (SELECT 1 FROM Tbl_Role WHERE RoleCode = 'Viewer')
BEGIN
    INSERT INTO Tbl_Role (RoleCode, RoleName, Description, IsActive, CreatedDate, DeleteFlag)
    VALUES ('Viewer', 'Viewer', 'Read-only access to system', 1, GETDATE(), 0);
END
GO

-- Helper table to insert permissions idempotently
CREATE TABLE #TempPermissions (
    PermissionCode VARCHAR(100),
    PermissionName VARCHAR(150),
    FeatureName VARCHAR(100),
    ActionName VARCHAR(50),
    Description VARCHAR(250)
);

INSERT INTO #TempPermissions (PermissionCode, PermissionName, FeatureName, ActionName, Description)
VALUES
('Card.View', 'View Cards', 'Card', 'View', 'Ability to view card list and details'),
('Card.Register', 'Register Cards', 'Card', 'Register', 'Ability to register new cards with OTP'),
('Card.Create', 'Create Cards', 'Card', 'Create', 'Ability to register new cards'),
('Card.Update', 'Update Cards', 'Card', 'Update', 'Ability to edit card details'),
('Card.Delete', 'Delete Cards', 'Card', 'Delete', 'Ability to delete cards'),

('Package.View', 'View Packages', 'Package', 'View', 'Ability to view package list and details'),
('Package.Create', 'Create Packages', 'Package', 'Create', 'Ability to register new packages'),
('Package.Update', 'Update Packages', 'Package', 'Update', 'Ability to edit package details'),
('Package.Delete', 'Delete Packages', 'Package', 'Delete', 'Ability to delete packages'),

('TopUp.View', 'View Top-Ups', 'TopUp', 'View', 'Ability to view top-up list and details'),
('TopUp.Create', 'Create Top-Ups', 'TopUp', 'Create', 'Ability to perform card top-up'),

('BusPayment.Create', 'Create Bus Payment (Tap)', 'Transaction', 'Create', 'Ability to tap cards on bus terminal'),
('Transaction.View', 'View Transactions', 'Transaction', 'View', 'Ability to view transaction history'),

('Bus.View', 'View Buses', 'Bus', 'View', 'Ability to view bus list and details'),
('Bus.Create', 'Create Buses', 'Bus', 'Create', 'Ability to register new buses'),
('Bus.Update', 'Update Buses', 'Bus', 'Update', 'Ability to edit bus details'),
('Bus.Delete', 'Delete Buses', 'Bus', 'Delete', 'Ability to delete buses'),

('Terminal.View', 'View Terminals', 'Terminal', 'View', 'Ability to view terminal list and details'),
('Terminal.Create', 'Create Terminals', 'Terminal', 'Create', 'Ability to register new terminals'),
('Terminal.Update', 'Update Terminals', 'Terminal', 'Update', 'Ability to edit terminal details'),
('Terminal.Delete', 'Delete Terminals', 'Terminal', 'Delete', 'Ability to delete terminals'),

('RolePermission.View', 'View Role Permissions', 'RolePermission', 'View', 'Ability to view role permissions'),
('RolePermission.Manage', 'Manage Role Permissions', 'RolePermission', 'Manage', 'Ability to edit roles, permissions, and assignments'),

('AuditLog.View', 'View Audit Logs', 'AuditLog', 'View', 'Ability to view system audit logs');

-- Insert permissions if they do not exist
INSERT INTO Tbl_Permission (PermissionCode, PermissionName, FeatureName, ActionName, Description, IsActive, CreatedDate, DeleteFlag)
SELECT tp.PermissionCode, tp.PermissionName, tp.FeatureName, tp.ActionName, tp.Description, 1, GETDATE(), 0
FROM #TempPermissions tp
WHERE NOT EXISTS (SELECT 1 FROM Tbl_Permission p WHERE p.PermissionCode = tp.PermissionCode);

-- Link all permissions to Admin Role (RoleId = 1)
INSERT INTO Tbl_RolePermission (RoleId, PermissionId, CreatedDate, DeleteFlag)
SELECT 1, p.PermissionId, GETDATE(), 0
FROM Tbl_Permission p
WHERE NOT EXISTS (
    SELECT 1 
    FROM Tbl_RolePermission rp 
    WHERE rp.RoleId = 1 AND rp.PermissionId = p.PermissionId
);

DROP TABLE #TempPermissions;
GO
