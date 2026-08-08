# Phase 12 Implementation Plan: Deployment Preparation

## Goal

Prepare the maintained YBS Smart Card System for staging or production deployment.

This phase focuses on configuration, security, database deployment, logging, hosting readiness, and release checks.

## Scope

Prepare deployment for:

- ASP.NET Core Web API
- Blazor App
- SQL Server database
- JWT authentication
- Dynamic RBAC
- AuditLog
- Serilog/file logging

Do not add new business features in this phase.

## Prerequisites

Phases 1 through 11 should be complete.

Confirm:

```text
API builds
Blazor app builds
Database schema is updated
EF Core models are scaffolded
Authentication works
Dynamic RBAC works
AuditLog works
Core workflows work
```

## Step 1: Remove Secrets From appsettings.json

Review:

```text
YbsSmartCardSystem.Api/appsettings.json
YbsSmartCardSystem.Api/appsettings.Development.json
YbsSmartCardSystem.App/appsettings.json
YbsSmartCardSystem.App/appsettings.Development.json
```

Do not commit production secrets:

```text
SQL usernames/passwords
JWT signing keys
API keys
External service credentials
Production URLs if sensitive
```

Use environment variables, user secrets, or deployment platform secrets.

Recommended API config keys:

```text
ConnectionStrings__DbConnection
Jwt__Issuer
Jwt__Audience
Jwt__SigningKey
Jwt__ExpiryMinutes
Serilog__WriteTo__0__Args__path
```

## Step 2: Configure Production appsettings

Keep `appsettings.json` safe and generic.

Recommended:

```json
{
  "AllowedHosts": "*",
  "Jwt": {
    "Issuer": "YbsSmartCardSystem",
    "Audience": "YbsSmartCardSystem",
    "ExpiryMinutes": 60
  }
}
```

Do not include the production JWT signing key in the file.

Use `appsettings.Development.json` only for local development values.

## Step 3: Configure SQL Server Deployment

Prepare database deployment scripts under:

```text
YbsSmartCardSystem.Database/Scripts
```

Expected scripts:

```text
Phase4_AddPackageAuthRbacAuditLog.sql
Seed_Roles_Permissions.sql
Seed_AdminUser_Template.sql
```

Rules:

- Scripts must be reviewable.
- Avoid destructive changes.
- Use `IF EXISTS` / `IF NOT EXISTS` guards.
- Do not include real production passwords.
- Do not include plain text admin passwords.

For admin user creation, use a generated password hash.

## Step 4: Configure JWT Production Settings

Production JWT signing key must be:

- Stored outside source control
- At least 32 characters
- Random and hard to guess
- Different per environment

Confirm token settings:

```text
Issuer
Audience
SigningKey
ExpiryMinutes
ClockSkew
```

Recommended:

```text
Access token expiry: 30 to 120 minutes
Clock skew: small, such as 1 to 5 minutes
```

## Step 5: Configure Serilog File Logging

If Serilog is not fully implemented yet, add or finalize it in Infrastructure/API.

Production logging should include:

```text
Timestamp
Level
Message
Exception
Request path
UserId if available
Correlation/request ID if available
```

Recommended file path should be configurable:

```text
logs/ybs-api-.txt
```

Use rolling files:

```text
Daily rolling
Retention limit
File size limit
```

Do not log passwords, JWT tokens, signing keys, or connection strings.

## Step 6: Configure HTTPS

Production API and App should run behind HTTPS.

Confirm:

```text
app.UseHttpsRedirection()
HSTS enabled outside Development
TLS certificate configured on hosting environment
```

For reverse proxy hosting, confirm forwarded headers are configured if needed:

```text
X-Forwarded-For
X-Forwarded-Proto
```

This matters for correct IP address in AuditLog.

## Step 7: Configure CORS If API And App Are Hosted Separately

If API and Blazor App use different origins, configure CORS in API.

Example origins:

```text
https://ybs-api.example.com
https://ybs-app.example.com
```

Rules:

- Do not use wildcard origins with credentials.
- Allow only required methods.
- Allow Authorization header.

Typical allowed methods:

```text
GET
POST
PUT
PATCH
DELETE
```

## Step 8: Configure Blazor Backend URL

Review:

```text
YbsSmartCardSystem.App/appsettings.json
```

Set:

```text
BackendApiUrl
```

For production, use environment-specific config.

Do not hardcode localhost in production.

## Step 9: Confirm Authorization Defaults

Review API authorization behavior:

- Login must allow anonymous access.
- Protected endpoints must require JWT.
- Permission-based endpoints must require proper Dynamic RBAC permissions.
- Missing token should return 401.
- Valid token without permission should return 403.

Confirm permissions are seeded for Admin.

Confirm at least one admin user exists.

## Step 10: Prepare Publish Commands

From solution root, validate publish commands.

API:

```powershell
dotnet publish YbsSmartCardSystem.Api/YbsSmartCardSystem.Api.csproj -c Release -o publish/api
```

Blazor App:

```powershell
dotnet publish YbsSmartCardSystem.App/YbsSmartCardSystem.App.csproj -c Release -o publish/app
```

If Tailwind is used, ensure CSS build runs before publish or is integrated into the build process.

## Step 11: Add Release Checklist

Create:

```text
docs/release-checklist.md
```

Checklist should include:

```text
Build Release configuration
Run tests
Apply database scripts
Verify seed roles/permissions
Create admin user
Set environment variables
Verify JWT settings
Verify logging path permissions
Verify HTTPS
Verify CORS
Smoke test login
Smoke test Card
Smoke test Package
Smoke test TopUp
Smoke test Bus payment
Smoke test Transaction history
Smoke test RolePermission
Smoke test AuditLog
Backup database before deployment
Record deployed version/commit
```

## Step 12: Add Environment Variable Documentation

Create:

```text
docs/environment-variables.md
```

Document required variables:

```text
ConnectionStrings__DbConnection
Jwt__Issuer
Jwt__Audience
Jwt__SigningKey
Jwt__ExpiryMinutes
BackendApiUrl
ASPNETCORE_ENVIRONMENT
```

Include example names, not real secret values.

## Step 13: Add Health Check Endpoint Optional

Optional but recommended:

```text
GET /health
```

Use ASP.NET Core health checks.

At minimum, check:

```text
API process is running
Database connection works
```

Do not expose sensitive diagnostics in public health output.

## Step 14: Final Security Review

Search for secrets:

```powershell
rg "Password=|User Id=|SigningKey|secret|Token|sa;" .
```

Review any matches manually.

Also check:

```text
No plain text passwords in seed scripts
No production JWT key in JSON files
No database password in generated DbContext
No sensitive values in AuditLog
No anonymous access to protected controllers
```

## Step 15: Final Smoke Test

In a staging-like environment:

```text
Login as Admin
Create card
Create package
Top up card
Perform bus tap/payment
View transaction history
Manage role permissions
Login as limited user
Confirm denied actions return 403
View audit logs as Admin
Confirm limited user cannot view audit logs
```

## Do Not Do In Phase 12

- Do not add new business features.
- Do not weaken authorization to simplify deployment.
- Do not commit production secrets.
- Do not commit real admin passwords.
- Do not expose detailed exception pages in production.
- Do not expose sensitive health check details publicly.
- Do not make destructive database changes without backup.

## Verification

Run:

```powershell
dotnet restore
dotnet build -c Release
dotnet publish YbsSmartCardSystem.Api/YbsSmartCardSystem.Api.csproj -c Release -o publish/api
dotnet publish YbsSmartCardSystem.App/YbsSmartCardSystem.App.csproj -c Release -o publish/app
```

If restore fails because NuGet is unavailable, record the exact error.

Run secret scan:

```powershell
rg "Password=|User Id=|SigningKey|secret|Token|sa;" .
```

Expected:

- No production secrets are committed.
- Release build succeeds.
- Publish output is produced.
- Deployment documentation exists.

## Expected Result

- API and Blazor app are ready for staging/production deployment.
- Secrets are externalized.
- Database deployment scripts are prepared.
- JWT, logging, HTTPS, CORS, and environment configuration are documented.
- Release checklist is available.
- Project maintenance roadmap is complete.

## Git Milestone

```text
chore: prepare deployment settings
```
