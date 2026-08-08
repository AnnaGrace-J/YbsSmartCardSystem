USE [YbsSmartCard];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. Check for Duplicate MobileNo in Tbl_Card
    IF EXISTS (
        SELECT MobileNo
        FROM dbo.Tbl_Card
        WHERE DeleteFlag = 0 AND MobileNo IS NOT NULL
        GROUP BY MobileNo
        HAVING COUNT(*) > 1
    )
    BEGIN
        RAISERROR('Duplicate MobileNo found in active Tbl_Card records. Please resolve duplicates before applying this script.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    -- 2. Add Unique Index for MobileNo on Tbl_Card
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Tbl_Card_MobileNo_Active' AND object_id = OBJECT_ID('dbo.Tbl_Card'))
    BEGIN
        CREATE UNIQUE INDEX UX_Tbl_Card_MobileNo_Active
        ON dbo.Tbl_Card(MobileNo)
        WHERE DeleteFlag = 0 AND MobileNo IS NOT NULL;
    END

    -- 3. Create Tbl_StaffUser
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_StaffUser]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[Tbl_StaffUser](
            [StaffUserId] [int] IDENTITY(1,1) NOT NULL,
            [UserName] [nvarchar](100) NOT NULL,
            [FullName] [nvarchar](150) NOT NULL,
            [PhoneNo] [nvarchar](20) NULL,
            [Email] [nvarchar](150) NULL,
            [PasswordHash] [nvarchar](500) NOT NULL,
            [PasswordSalt] [nvarchar](500) NULL,
            [IsActive] [bit] NOT NULL,
            [LastLoginDate] [datetime] NULL,
            [CreatedDate] [datetime] NOT NULL,
            [UpdatedDate] [datetime] NULL,
            [DeleteFlag] [bit] NOT NULL,
         CONSTRAINT [PK_Tbl_StaffUser] PRIMARY KEY CLUSTERED 
        (
            [StaffUserId] ASC
        ))
    END

    -- 4. Create Tbl_ViewerUser
    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_ViewerUser]') AND type in (N'U'))
    BEGIN
        CREATE TABLE [dbo].[Tbl_ViewerUser](
            [ViewerUserId] [int] IDENTITY(1,1) NOT NULL,
            [UserName] [nvarchar](100) NOT NULL,
            [FullName] [nvarchar](150) NOT NULL,
            [PhoneNo] [nvarchar](20) NOT NULL,
            [PasswordHash] [nvarchar](500) NOT NULL,
            [PasswordSalt] [nvarchar](500) NULL,
            [IsActive] [bit] NOT NULL,
            [LastLoginDate] [datetime] NULL,
            [CreatedDate] [datetime] NOT NULL,
            [UpdatedDate] [datetime] NULL,
            [DeleteFlag] [bit] NOT NULL,
         CONSTRAINT [PK_Tbl_ViewerUser] PRIMARY KEY CLUSTERED 
        (
            [ViewerUserId] ASC
        ))
    END

    -- 5. Migrate Data from Tbl_User
    IF OBJECT_ID('dbo.Tbl_User') IS NOT NULL
    BEGIN
        -- Migrate Staff Users (those who have an entry in Tbl_UserRole)
        SET IDENTITY_INSERT [dbo].[Tbl_StaffUser] ON;
        INSERT INTO [dbo].[Tbl_StaffUser] (StaffUserId, UserName, FullName, PhoneNo, Email, PasswordHash, PasswordSalt, IsActive, LastLoginDate, CreatedDate, UpdatedDate, DeleteFlag)
        SELECT u.UserId, u.UserName, u.FullName, u.PhoneNo, u.Email, u.PasswordHash, u.PasswordSalt, u.IsActive, u.LastLoginDate, u.CreatedDate, u.UpdatedDate, u.DeleteFlag
        FROM [dbo].[Tbl_User] u
        WHERE EXISTS (SELECT 1 FROM [dbo].[Tbl_UserRole] ur WHERE ur.UserId = u.UserId);
        SET IDENTITY_INSERT [dbo].[Tbl_StaffUser] OFF;

        -- Migrate Viewer Users (those without roles)
        SET IDENTITY_INSERT [dbo].[Tbl_ViewerUser] ON;
        INSERT INTO [dbo].[Tbl_ViewerUser] (ViewerUserId, UserName, FullName, PhoneNo, PasswordHash, PasswordSalt, IsActive, LastLoginDate, CreatedDate, UpdatedDate, DeleteFlag)
        SELECT u.UserId, u.UserName, u.FullName, u.PhoneNo, u.PasswordHash, u.PasswordSalt, u.IsActive, u.LastLoginDate, u.CreatedDate, u.UpdatedDate, u.DeleteFlag
        FROM [dbo].[Tbl_User] u
        WHERE NOT EXISTS (SELECT 1 FROM [dbo].[Tbl_UserRole] ur WHERE ur.UserId = u.UserId);
        SET IDENTITY_INSERT [dbo].[Tbl_ViewerUser] OFF;
    END

    -- 6. Add UserType to Tbl_AuditLog
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_AuditLog]') AND name = 'UserType')
    BEGIN
        ALTER TABLE [dbo].[Tbl_AuditLog] ADD [UserType] [nvarchar](20) NULL;
    END

    -- Update UserType for existing audit logs based on the user id
    UPDATE al
    SET al.UserType = 'Staff'
    FROM [dbo].[Tbl_AuditLog] al
    INNER JOIN [dbo].[Tbl_StaffUser] su ON al.UserId = su.StaffUserId;

    UPDATE al
    SET al.UserType = 'Viewer'
    FROM [dbo].[Tbl_AuditLog] al
    INNER JOIN [dbo].[Tbl_ViewerUser] vu ON al.UserId = vu.ViewerUserId;

    -- 7. Drop Foreign Keys referencing Tbl_User
    DECLARE @DropFKs NVARCHAR(MAX) = '';
    SELECT @DropFKs += 'ALTER TABLE ' + QUOTENAME(OBJECT_SCHEMA_NAME(parent_object_id)) + '.' + QUOTENAME(OBJECT_NAME(parent_object_id)) + 
                       ' DROP CONSTRAINT ' + QUOTENAME(name) + ';' + CHAR(13)
    FROM sys.foreign_keys
    WHERE referenced_object_id = OBJECT_ID('dbo.Tbl_User');

    IF @DropFKs <> ''
    BEGIN
        EXEC sp_executesql @DropFKs;
    END

    -- 8. Drop Tbl_User table
    IF OBJECT_ID('dbo.Tbl_User') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[Tbl_User];
    END

    -- 9. Add Foreign Key for Tbl_UserRole to Tbl_StaffUser
    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tbl_UserRole_Tbl_StaffUser]') AND parent_object_id = OBJECT_ID(N'[dbo].[Tbl_UserRole]'))
    BEGIN
        ALTER TABLE [dbo].[Tbl_UserRole]  WITH CHECK ADD  CONSTRAINT [FK_Tbl_UserRole_Tbl_StaffUser] FOREIGN KEY([UserId])
        REFERENCES [dbo].[Tbl_StaffUser] ([StaffUserId]);
        ALTER TABLE [dbo].[Tbl_UserRole] CHECK CONSTRAINT [FK_Tbl_UserRole_Tbl_StaffUser];
    END

    -- 10. Drop Tbl_Package
    IF OBJECT_ID('dbo.Tbl_Package') IS NOT NULL
    BEGIN
        DROP TABLE [dbo].[Tbl_Package];
    END
    
    -- 11. Add Unique Constraints for Staff and Viewer
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Tbl_StaffUser_UserName_Active' AND object_id = OBJECT_ID('dbo.Tbl_StaffUser'))
    BEGIN
        CREATE UNIQUE INDEX UX_Tbl_StaffUser_UserName_Active
        ON dbo.Tbl_StaffUser(UserName)
        WHERE DeleteFlag = 0;
    END
    
    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Tbl_ViewerUser_UserName_Active' AND object_id = OBJECT_ID('dbo.Tbl_ViewerUser'))
    BEGIN
        CREATE UNIQUE INDEX UX_Tbl_ViewerUser_UserName_Active
        ON dbo.Tbl_ViewerUser(UserName)
        WHERE DeleteFlag = 0;
    END

    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UX_Tbl_ViewerUser_PhoneNo_Active' AND object_id = OBJECT_ID('dbo.Tbl_ViewerUser'))
    BEGIN
        CREATE UNIQUE INDEX UX_Tbl_ViewerUser_PhoneNo_Active
        ON dbo.Tbl_ViewerUser(PhoneNo)
        WHERE DeleteFlag = 0;
    END

    COMMIT TRANSACTION;
    PRINT 'Migration completed successfully.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
    DECLARE @ErrorState INT = ERROR_STATE();

    RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH
GO
