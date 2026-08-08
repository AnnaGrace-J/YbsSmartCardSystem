# Environment Variables Reference

To keep secrets secure and separate configurations between environments, use the following environment variables.

## ASP.NET Core Web API Configuration

| Environment Variable | Description | Example / Recommended Value |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Defines host environment. Set to `Production` or `Staging` for deployments. | `Production` |
| `ConnectionStrings__DbConnection` | Connection string to SQL Server database. Must contain production SQL credentials. | `Server=tcp:sqlserver.ybs.local,1433;Database=YbsSmartCard;User Id=ybs_api;Password=StrongPassword123;TrustServerCertificate=True;` |
| `Jwt__Issuer` | The issuer claim of the JWT access tokens. | `YbsSmartCardSystem` |
| `Jwt__Audience` | The audience claim of the JWT access tokens. | `YbsSmartCardSystem` |
| `Jwt__SigningKey` | Secret key used to sign JWT tokens. **Must be a cryptographically secure random string of at least 32 characters (256-bit).** | `M2NmM2QxNDAtOTUzOS00YTNjLThmMDgtZTMxNTI3N2Y0MGMw` |
| `Jwt__ExpiryMinutes` | Lifetime of the access token in minutes. Recommended to keep between 30 and 120. | `60` |
| `Cors__AllowedOrigins__0` | The origin of the Blazor App to allow cross-origin requests from. | `https://ybs-app.ybs.com` |

## Blazor App Configuration

| Environment Variable | Description | Example / Recommended Value |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | Defines host environment. | `Production` |
| `BackendApiUrl` | The base URL of the ASP.NET Core Web API. Used by Blazor's API services. | `https://ybs-api.ybs.com` |
