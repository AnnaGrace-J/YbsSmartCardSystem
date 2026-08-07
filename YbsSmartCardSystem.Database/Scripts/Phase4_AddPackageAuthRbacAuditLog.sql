-- Use YbsSmartCard database
USE YbsSmartCard;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 1. Create Tbl_Package
IF OBJECT_ID('dbo.Tbl_Package', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_Package
    (
        PackageId INT IDENTITY(1,1) NOT NULL,
        PackageCode NVARCHAR(50) NOT NULL,
        PackageName NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        RideLimit INT NULL,
        ValidDays INT NULL,
        Description NVARCHAR(250) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Tbl_Package_IsActive DEFAULT 1,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_Package_CreatedDate DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_Package_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_Package PRIMARY KEY (PackageId)
    );

    CREATE UNIQUE INDEX UX_Tbl_Package_PackageCode_Active
    ON dbo.Tbl_Package(PackageCode)
    WHERE DeleteFlag = 0;

    CREATE INDEX IX_Tbl_Package_IsActive
    ON dbo.Tbl_Package(IsActive);
END;
GO

-- 2. Create Tbl_User
IF OBJECT_ID('dbo.Tbl_User', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_User
    (
        UserId INT IDENTITY(1,1) NOT NULL,
        UserName NVARCHAR(100) NOT NULL,
        FullName NVARCHAR(150) NOT NULL,
        Email NVARCHAR(150) NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        PasswordSalt NVARCHAR(500) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Tbl_User_IsActive DEFAULT 1,
        LastLoginDate DATETIME NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_User_CreatedDate DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_User_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_User PRIMARY KEY (UserId)
    );

    CREATE UNIQUE INDEX UX_Tbl_User_UserName_Active
    ON dbo.Tbl_User(UserName)
    WHERE DeleteFlag = 0;

    CREATE UNIQUE INDEX UX_Tbl_User_Email_Active
    ON dbo.Tbl_User(Email)
    WHERE Email IS NOT NULL AND DeleteFlag = 0;

    CREATE INDEX IX_Tbl_User_IsActive
    ON dbo.Tbl_User(IsActive);
END;
GO

-- 3. Create Tbl_Role
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

    CREATE UNIQUE INDEX UX_Tbl_Role_RoleCode_Active
    ON dbo.Tbl_Role(RoleCode)
    WHERE DeleteFlag = 0;
END;
GO

-- 4. Create Tbl_Permission
IF OBJECT_ID('dbo.Tbl_Permission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_Permission
    (
        PermissionId INT IDENTITY(1,1) NOT NULL,
        PermissionCode NVARCHAR(100) NOT NULL,
        PermissionName NVARCHAR(150) NOT NULL,
        FeatureName NVARCHAR(100) NOT NULL,
        ActionName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(250) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Tbl_Permission_IsActive DEFAULT 1,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_Permission_CreatedDate DEFAULT GETDATE(),
        UpdatedDate DATETIME NULL,
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_Permission_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_Permission PRIMARY KEY (PermissionId)
    );

    CREATE UNIQUE INDEX UX_Tbl_Permission_PermissionCode_Active
    ON dbo.Tbl_Permission(PermissionCode)
    WHERE DeleteFlag = 0;

    CREATE INDEX IX_Tbl_Permission_FeatureName
    ON dbo.Tbl_Permission(FeatureName);

    CREATE INDEX IX_Tbl_Permission_IsActive
    ON dbo.Tbl_Permission(IsActive);
END;
GO

-- 5. Create Tbl_UserRole
IF OBJECT_ID('dbo.Tbl_UserRole', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_UserRole
    (
        UserRoleId INT IDENTITY(1,1) NOT NULL,
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_UserRole_CreatedDate DEFAULT GETDATE(),
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_UserRole_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_UserRole PRIMARY KEY (UserRoleId),
        CONSTRAINT FK_Tbl_UserRole_Tbl_User FOREIGN KEY (UserId) REFERENCES dbo.Tbl_User(UserId),
        CONSTRAINT FK_Tbl_UserRole_Tbl_Role FOREIGN KEY (RoleId) REFERENCES dbo.Tbl_Role(RoleId)
    );

    CREATE UNIQUE INDEX UX_Tbl_UserRole_User_Role_Active
    ON dbo.Tbl_UserRole(UserId, RoleId)
    WHERE DeleteFlag = 0;

    CREATE INDEX IX_Tbl_UserRole_UserId ON dbo.Tbl_UserRole(UserId);
    CREATE INDEX IX_Tbl_UserRole_RoleId ON dbo.Tbl_UserRole(RoleId);
END;
GO

-- 6. Create Tbl_RolePermission
IF OBJECT_ID('dbo.Tbl_RolePermission', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_RolePermission
    (
        RolePermissionId INT IDENTITY(1,1) NOT NULL,
        RoleId INT NOT NULL,
        PermissionId INT NOT NULL,
        CreatedDate DATETIME NOT NULL CONSTRAINT DF_Tbl_RolePermission_CreatedDate DEFAULT GETDATE(),
        DeleteFlag BIT NOT NULL CONSTRAINT DF_Tbl_RolePermission_DeleteFlag DEFAULT 0,
        CONSTRAINT PK_Tbl_RolePermission PRIMARY KEY (RolePermissionId),
        CONSTRAINT FK_Tbl_RolePermission_Tbl_Role FOREIGN KEY (RoleId) REFERENCES dbo.Tbl_Role(RoleId),
        CONSTRAINT FK_Tbl_RolePermission_Tbl_Permission FOREIGN KEY (PermissionId) REFERENCES dbo.Tbl_Permission(PermissionId)
    );

    CREATE UNIQUE INDEX UX_Tbl_RolePermission_Role_Permission_Active
    ON dbo.Tbl_RolePermission(RoleId, PermissionId)
    WHERE DeleteFlag = 0;

    CREATE INDEX IX_Tbl_RolePermission_RoleId ON dbo.Tbl_RolePermission(RoleId);
    CREATE INDEX IX_Tbl_RolePermission_PermissionId ON dbo.Tbl_RolePermission(PermissionId);
END;
GO

-- 7. Create Tbl_AuditLog
IF OBJECT_ID('dbo.Tbl_AuditLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Tbl_AuditLog
    (
        AuditLogId BIGINT IDENTITY(1,1) NOT NULL,
        UserId INT NULL,
        Action NVARCHAR(100) NOT NULL,
        FeatureName NVARCHAR(100) NOT NULL,
        EntityName NVARCHAR(100) NULL,
        EntityId NVARCHAR(100) NULL,
        OldValue NVARCHAR(MAX) NULL,
        NewValue NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(100) NULL,
        UserAgent NVARCHAR(500) NULL,
        CreatedDateTime DATETIME NOT NULL CONSTRAINT DF_Tbl_AuditLog_CreatedDateTime DEFAULT GETDATE(),
        CONSTRAINT PK_Tbl_AuditLog PRIMARY KEY (AuditLogId),
        CONSTRAINT FK_Tbl_AuditLog_Tbl_User FOREIGN KEY (UserId) REFERENCES dbo.Tbl_User(UserId)
    );

    CREATE INDEX IX_Tbl_AuditLog_UserId ON dbo.Tbl_AuditLog(UserId);
    CREATE INDEX IX_Tbl_AuditLog_Action ON dbo.Tbl_AuditLog(Action);
    CREATE INDEX IX_Tbl_AuditLog_FeatureName ON dbo.Tbl_AuditLog(FeatureName);
    CREATE INDEX IX_Tbl_AuditLog_CreatedDateTime ON dbo.Tbl_AuditLog(CreatedDateTime);
END;
GO

-- ===================================================
-- SEED DATA
-- ===================================================

-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM dbo.Tbl_Role WHERE RoleCode = 'Admin' AND DeleteFlag = 0)
    INSERT INTO dbo.Tbl_Role (RoleCode, RoleName, Description, IsSystemRole)
    VALUES ('Admin', 'Administrator', 'System Administrator with full access', 1);

IF NOT EXISTS (SELECT 1 FROM dbo.Tbl_Role WHERE RoleCode = 'Operator' AND DeleteFlag = 0)
    INSERT INTO dbo.Tbl_Role (RoleCode, RoleName, Description, IsSystemRole)
    VALUES ('Operator', 'Operator', 'Standard operator with manage access', 0);

IF NOT EXISTS (SELECT 1 FROM dbo.Tbl_Role WHERE RoleCode = 'Viewer' AND DeleteFlag = 0)
    INSERT INTO dbo.Tbl_Role (RoleCode, RoleName, Description, IsSystemRole)
    VALUES ('Viewer', 'Viewer', 'Read-only access', 0);
GO

-- Seed Permissions Helper Function / Insert block
-- Card.View, Card.Create, Card.Update, Card.Delete
-- Package.View, Package.Create, Package.Update, Package.Delete
-- TopUp.View, TopUp.Create
-- BusPayment.Create
-- Transaction.View
-- RolePermission.View, RolePermission.Manage
-- AuditLog.View

CREATE TABLE #TempPerms (
    Code NVARCHAR(100),
    Name NVARCHAR(150),
    Feature NVARCHAR(100),
    Action NVARCHAR(100)
);

INSERT INTO #TempPerms (Code, Name, Feature, Action) VALUES
('Card.View', 'View Cards', 'Card', 'View'),
('Card.Create', 'Create Cards', 'Card', 'Create'),
('Card.Update', 'Update Cards', 'Card', 'Update'),
('Card.Delete', 'Delete Cards', 'Card', 'Delete'),
('Package.View', 'View Packages', 'Package', 'View'),
('Package.Create', 'Create Packages', 'Package', 'Create'),
('Package.Update', 'Update Packages', 'Package', 'Update'),
('Package.Delete', 'Delete Packages', 'Package', 'Delete'),
('TopUp.View', 'View Top-Ups', 'TopUp', 'View'),
('TopUp.Create', 'Create Top-Ups', 'TopUp', 'Create'),
('BusPayment.Create', 'Create Bus Payment (Tap)', 'BusPayment', 'Create'),
('Transaction.View', 'View Transactions', 'Transaction', 'View'),
('RolePermission.View', 'View Role Permissions', 'RolePermission', 'View'),
('RolePermission.Manage', 'Manage Role Permissions', 'RolePermission', 'Manage'),
('AuditLog.View', 'View Audit Logs', 'AuditLog', 'View');

-- Merge permissions
INSERT INTO dbo.Tbl_Permission (PermissionCode, PermissionName, FeatureName, ActionName)
SELECT t.Code, t.Name, t.Feature, t.Action
FROM #TempPerms t
LEFT JOIN dbo.Tbl_Permission p ON t.Code = p.PermissionCode AND p.DeleteFlag = 0
WHERE p.PermissionId IS NULL;

DROP TABLE #TempPerms;
GO

-- Assign all Permissions to Admin
DECLARE @AdminRoleId INT;
SELECT @AdminRoleId = RoleId FROM dbo.Tbl_Role WHERE RoleCode = 'Admin' AND DeleteFlag = 0;

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.Tbl_RolePermission (RoleId, PermissionId)
    SELECT @AdminRoleId, p.PermissionId
    FROM dbo.Tbl_Permission p
    LEFT JOIN dbo.Tbl_RolePermission rp ON rp.RoleId = @AdminRoleId AND rp.PermissionId = p.PermissionId AND rp.DeleteFlag = 0
    WHERE p.DeleteFlag = 0 AND rp.RolePermissionId IS NULL;
END;
GO
