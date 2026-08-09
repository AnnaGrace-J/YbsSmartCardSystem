using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YbsSmartCardSystem.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Tbl_TopUp",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Tbl_Terminal",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Tbl_Card",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Tbl_Bus",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_TopUp_CreatedBy",
                table: "Tbl_TopUp",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Terminal_CreatedBy",
                table: "Tbl_Terminal",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Card_CreatedBy",
                table: "Tbl_Card",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Tbl_Bus_CreatedBy",
                table: "Tbl_Bus",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_TblBus_TblStaffUser",
                table: "Tbl_Bus",
                column: "CreatedBy",
                principalTable: "Tbl_StaffUser",
                principalColumn: "StaffUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblCard_TblStaffUser",
                table: "Tbl_Card",
                column: "CreatedBy",
                principalTable: "Tbl_StaffUser",
                principalColumn: "StaffUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblTerminal_TblStaffUser",
                table: "Tbl_Terminal",
                column: "CreatedBy",
                principalTable: "Tbl_StaffUser",
                principalColumn: "StaffUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TblTopUp_TblStaffUser",
                table: "Tbl_TopUp",
                column: "CreatedBy",
                principalTable: "Tbl_StaffUser",
                principalColumn: "StaffUserId");

            // Custom Backfill Script
            migrationBuilder.Sql(@"
                -- Backfill Tbl_Card
                UPDATE c SET c.CreatedBy = a.UserId
                FROM Tbl_Card c
                INNER JOIN (
                    SELECT EntityId, UserId, ROW_NUMBER() OVER(PARTITION BY EntityId ORDER BY CreatedDateTime ASC) as rn
                    FROM Tbl_AuditLog
                    WHERE EntityName = 'TblCard' AND Action IN ('Create', 'CardRegister') AND UserId IS NOT NULL
                ) a ON c.CardId = TRY_CAST(a.EntityId AS INT) AND a.rn = 1;

                -- Backfill Tbl_Bus
                UPDATE b SET b.CreatedBy = a.UserId
                FROM Tbl_Bus b
                INNER JOIN (
                    SELECT EntityId, UserId, ROW_NUMBER() OVER(PARTITION BY EntityId ORDER BY CreatedDateTime ASC) as rn
                    FROM Tbl_AuditLog
                    WHERE EntityName = 'TblBus' AND Action = 'Create' AND UserId IS NOT NULL
                ) a ON b.BusId = TRY_CAST(a.EntityId AS INT) AND a.rn = 1;

                -- Backfill Tbl_Terminal
                UPDATE t SET t.CreatedBy = a.UserId
                FROM Tbl_Terminal t
                INNER JOIN (
                    SELECT EntityId, UserId, ROW_NUMBER() OVER(PARTITION BY EntityId ORDER BY CreatedDateTime ASC) as rn
                    FROM Tbl_AuditLog
                    WHERE EntityName = 'TblTerminal' AND Action = 'Create' AND UserId IS NOT NULL
                ) a ON t.TerminalId = TRY_CAST(a.EntityId AS INT) AND a.rn = 1;

                -- Backfill Tbl_TopUp
                UPDATE t SET t.CreatedBy = a.UserId
                FROM Tbl_TopUp t
                INNER JOIN (
                    SELECT EntityId, UserId, ROW_NUMBER() OVER(PARTITION BY EntityId ORDER BY CreatedDateTime ASC) as rn
                    FROM Tbl_AuditLog
                    WHERE EntityName = 'TblTopUp' AND Action = 'TopUp' AND UserId IS NOT NULL
                ) a ON t.TopUpId = TRY_CAST(a.EntityId AS INT) AND a.rn = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TblBus_TblStaffUser",
                table: "Tbl_Bus");

            migrationBuilder.DropForeignKey(
                name: "FK_TblCard_TblStaffUser",
                table: "Tbl_Card");

            migrationBuilder.DropForeignKey(
                name: "FK_TblTerminal_TblStaffUser",
                table: "Tbl_Terminal");

            migrationBuilder.DropForeignKey(
                name: "FK_TblTopUp_TblStaffUser",
                table: "Tbl_TopUp");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_TopUp_CreatedBy",
                table: "Tbl_TopUp");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Terminal_CreatedBy",
                table: "Tbl_Terminal");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Card_CreatedBy",
                table: "Tbl_Card");

            migrationBuilder.DropIndex(
                name: "IX_Tbl_Bus_CreatedBy",
                table: "Tbl_Bus");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tbl_TopUp");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tbl_Terminal");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tbl_Card");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tbl_Bus");
        }
    }
}
