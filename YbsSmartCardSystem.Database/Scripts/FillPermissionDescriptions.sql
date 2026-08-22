SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

-- Populate Description for existing permissions (idempotent, matches by PermissionCode)

UPDATE Tbl_Permission SET Description = 'Ability to view card list and details'        WHERE PermissionCode = 'Card.View';
UPDATE Tbl_Permission SET Description = 'Ability to register new cards with OTP'        WHERE PermissionCode = 'Card.Register';
UPDATE Tbl_Permission SET Description = 'Ability to register new cards'                 WHERE PermissionCode = 'Card.Create';
UPDATE Tbl_Permission SET Description = 'Ability to edit card details'                  WHERE PermissionCode = 'Card.Update';
UPDATE Tbl_Permission SET Description = 'Ability to delete cards'                       WHERE PermissionCode = 'Card.Delete';

UPDATE Tbl_Permission SET Description = 'Ability to view top-up list and details'       WHERE PermissionCode = 'TopUp.View';
UPDATE Tbl_Permission SET Description = 'Ability to perform card top-up'                WHERE PermissionCode = 'TopUp.Create';

UPDATE Tbl_Permission SET Description = 'Ability to tap cards on bus terminal'          WHERE PermissionCode = 'BusPayment.Create';
UPDATE Tbl_Permission SET Description = 'Ability to view transaction history'           WHERE PermissionCode = 'Transaction.View';

UPDATE Tbl_Permission SET Description = 'Ability to view bus list and details'          WHERE PermissionCode = 'Bus.View';
UPDATE Tbl_Permission SET Description = 'Ability to register new buses'                 WHERE PermissionCode = 'Bus.Create';
UPDATE Tbl_Permission SET Description = 'Ability to edit bus details'                   WHERE PermissionCode = 'Bus.Update';
UPDATE Tbl_Permission SET Description = 'Ability to delete buses'                       WHERE PermissionCode = 'Bus.Delete';

UPDATE Tbl_Permission SET Description = 'Ability to view terminal list and details'     WHERE PermissionCode = 'Terminal.View';
UPDATE Tbl_Permission SET Description = 'Ability to register new terminals'             WHERE PermissionCode = 'Terminal.Create';
UPDATE Tbl_Permission SET Description = 'Ability to edit terminal details'              WHERE PermissionCode = 'Terminal.Update';
UPDATE Tbl_Permission SET Description = 'Ability to delete terminals'                   WHERE PermissionCode = 'Terminal.Delete';

UPDATE Tbl_Permission SET Description = 'Ability to view role permissions'              WHERE PermissionCode = 'RolePermission.View';
UPDATE Tbl_Permission SET Description = 'Ability to edit roles, permissions, and assignments' WHERE PermissionCode = 'RolePermission.Manage';

UPDATE Tbl_Permission SET Description = 'Ability to view system audit logs'             WHERE PermissionCode = 'AuditLog.View';
GO