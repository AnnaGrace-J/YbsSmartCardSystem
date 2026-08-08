SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
USE [YbsSmartCard]
GO

-- 1. Add PhoneNo column as nullable first to handle existing data
IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'PhoneNo' AND Object_ID = Object_ID(N'dbo.Tbl_User'))
BEGIN
    ALTER TABLE [dbo].[Tbl_User] ADD [PhoneNo] NVARCHAR(20) NULL;
END
GO

-- 2. Populate PhoneNo for existing records
UPDATE [dbo].[Tbl_User]
SET [PhoneNo] = [UserName] -- Using UserName as dummy phone number for existing test users like 'admin'
WHERE [PhoneNo] IS NULL;
GO

-- 3. Alter PhoneNo to NOT NULL
ALTER TABLE [dbo].[Tbl_User] ALTER COLUMN [PhoneNo] NVARCHAR(20) NOT NULL;
GO

-- 4. Add Unique Constraint on UserName where DeleteFlag = 0
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_User]') AND name = N'IX_Tbl_User_UserName_Active')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Tbl_User_UserName_Active] 
    ON [dbo].[Tbl_User] ([UserName]) 
    WHERE [DeleteFlag] = 0;
END
GO

-- 5. Add Unique Constraint on PhoneNo where DeleteFlag = 0
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Tbl_User]') AND name = N'IX_Tbl_User_PhoneNo_Active')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_Tbl_User_PhoneNo_Active] 
    ON [dbo].[Tbl_User] ([PhoneNo]) 
    WHERE [DeleteFlag] = 0;
END
GO
