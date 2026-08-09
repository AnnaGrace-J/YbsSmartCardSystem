using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YbsSmartCardSystem.Database.Migrations
{
    public partial class RemoveEmailFullName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Tbl_StaffUser");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Tbl_StaffUser");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Tbl_ViewerUser");

            migrationBuilder.Sql(@"
DELETE FROM dbo.Tbl_UserRole;
DELETE FROM dbo.Tbl_StaffUser;
DELETE FROM dbo.Tbl_ViewerUser;

DBCC CHECKIDENT ('dbo.Tbl_StaffUser', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tbl_ViewerUser', RESEED, 0);
DBCC CHECKIDENT ('dbo.Tbl_UserRole', RESEED, 0);

INSERT INTO dbo.Tbl_StaffUser (UserName, PhoneNo, PasswordHash, IsActive, CreatedDate, DeleteFlag)
VALUES ('Admin', '09979558847', 'AQAAAAIAAYagAAAAELF1hLK4Z03e1nmjbDEQK4iGgxjSc9XwFFWovg65J9pgL0RYt5wrezlPJxqBPI+0Fg==', 1, GETDATE(), 0);

DECLARE @AdminId INT = SCOPE_IDENTITY();
DECLARE @AdminRoleId INT = (SELECT RoleId FROM dbo.Tbl_Role WHERE RoleCode = 'ADMIN');

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.Tbl_UserRole (UserId, RoleId, CreatedDate, DeleteFlag)
    VALUES (@AdminId, @AdminRoleId, GETDATE(), 0);
END

INSERT INTO dbo.Tbl_StaffUser (UserName, PhoneNo, PasswordHash, IsActive, CreatedDate, DeleteFlag)
VALUES ('Operator', '09449693537', 'AQAAAAIAAYagAAAAEAjaqLSId3/KpRMK+h9NWp8jYTA50UZEy+9A3CBZmE0qJ6sV9y8AXWMcWHi7mQgroQ==', 1, GETDATE(), 0);

DECLARE @OperatorId INT = SCOPE_IDENTITY();
DECLARE @OperatorRoleId INT = (SELECT RoleId FROM dbo.Tbl_Role WHERE RoleCode = 'OPERATOR');

IF @OperatorRoleId IS NOT NULL
BEGIN
    INSERT INTO dbo.Tbl_UserRole (UserId, RoleId, CreatedDate, DeleteFlag)
    VALUES (@OperatorId, @OperatorRoleId, GETDATE(), 0);
END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Tbl_ViewerUser",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Tbl_StaffUser",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Tbl_StaffUser",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}
