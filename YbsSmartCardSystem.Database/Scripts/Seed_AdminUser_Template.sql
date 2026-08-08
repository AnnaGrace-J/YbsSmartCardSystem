SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- 1. Insert admin user if not exists (Username: admin, Password Hash for 'admin123')
IF NOT EXISTS (SELECT 1 FROM Tbl_User WHERE UserName = 'admin')
BEGIN
    INSERT INTO Tbl_User (UserName, PasswordHash, FullName, Email, PhoneNo, IsActive, CreatedDate, DeleteFlag)
    VALUES (
        'admin', 
        'AQAAAAIAAYagAAAAEOpPL6z2IJiWUgTJ+IhLO/BHcsDqY/KihKO0PTjsjMDbpk5fEYQfSN3rqD9vFfbsdA==', 
        'System Administrator', 
        'admin@ybs.com', 
        '09123456789', 
        1, 
        GETDATE(), 
        0
    );
END
GO

-- 2. Link admin user to Admin role (RoleId = 1) in Tbl_UserRole if not exists
DECLARE @AdminUserId INT;
SELECT @AdminUserId = UserId FROM Tbl_User WHERE UserName = 'admin';

IF @AdminUserId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM Tbl_UserRole WHERE UserId = @AdminUserId AND RoleId = 1)
    BEGIN
        INSERT INTO Tbl_UserRole (UserId, RoleId, CreatedDate, DeleteFlag)
        VALUES (@AdminUserId, 1, GETDATE(), 0);
    END
END
GO
