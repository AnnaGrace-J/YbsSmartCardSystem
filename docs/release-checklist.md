# Release Checklist

This checklist describes the steps to prepare, deploy, and verify a production/staging release of the YBS Smart Card System.

## 1. Pre-Deployment Configuration & Build
- [ ] Ensure `ASPNETCORE_ENVIRONMENT` is set correctly for target environment (e.g. `Staging` or `Production`).
- [ ] Configure environment variables/secrets on hosting platform (see `docs/environment-variables.md` for required keys).
- [ ] Verify that no production secrets (passwords, keys, connection strings) are stored in `appsettings.json`.
- [ ] Run automated tests locally to verify clean codebase state.
- [ ] Build the API with Release configuration:
  ```powershell
  dotnet publish YbsSmartCardSystem.Api/YbsSmartCardSystem.Api.csproj -c Release -o publish/api
  ```
- [ ] Build and watch/compile Tailwind CSS for the Blazor App:
  ```powershell
  npm run build:css
  ```
- [ ] Build the Blazor App with Release configuration:
  ```powershell
  dotnet publish YbsSmartCardSystem.App/YbsSmartCardSystem.App.csproj -c Release -o publish/app
  ```

## 2. Database Migration & Seeding
- [ ] Create a backup of the existing database before running updates.
- [ ] Apply the database schema creation/update scripts:
  - `YbsSmartCardSystem.Database/Scripts/Phase4_AddPackageAuthRbacAuditLog.sql`
- [ ] Apply the roles and permissions seed script:
  - `YbsSmartCardSystem.Database/Scripts/Seed_Roles_Permissions.sql`
- [ ] Apply the administrator account seed script:
  - `YbsSmartCardSystem.Database/Scripts/Seed_AdminUser_Template.sql`
- [ ] Verify that exactly 23 permissions exist in the `Tbl_Permission` table and are linked to the `Admin` role (RoleId = 1).

## 3. Deployment & Hosting Verification
- [ ] Deploy published API binaries to web server.
- [ ] Deploy published Blazor App binaries to web server.
- [ ] Verify SSL/TLS certificates are active and HTTPS is enforced for both sites.
- [ ] If using reverse proxies (IIS ARR, Nginx, Cloudflare), verify forwarded headers configuration.
- [ ] Verify CORS setup on the API matches the hosted origin of the Blazor App.

## 4. Post-Deployment Smoke Tests
- [ ] Navigate to the Blazor App URL (should redirect to `/login` if not logged in).
- [ ] Log in using the system administrator account (`admin` / `admin123`).
- [ ] Verify the sidebar displays links only for authorized permissions.
- [ ] **Smoke Test Cards**: Create a card, edit card details, and perform pagination on the card list.
- [ ] **Smoke Test Packages**: Register a package, edit details, and verify list page.
- [ ] **Smoke Test Topups**: Perform card top-up and check top-up history.
- [ ] **Smoke Test Transactions**: Tap a card on bus terminal (Bus Payment page) and verify transaction list updates.
- [ ] **Smoke Test Role/Permissions**: View roles, permissions, and change user assignments.
- [ ] **Smoke Test Audit Log**: Open Audit Logs page and confirm that all above actions were logged.
- [ ] Verify that log files are created under `logs/ybs-api-*.txt` and rotate daily.
