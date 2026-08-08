# User Registration, Phone Login, and User Card Info Implementation Plan

## Goal

Add user self-registration with phone OTP verification, then change login to use phone number and password.

After login, show information related to the logged-in user, especially cards created for that user's phone number.

## New User Workflow

```text
User enters username, phone number, and password
System sends OTP to phone number
User enters OTP
If OTP is valid, system creates user account
User logs in with phone number and password
After login, user sees related card information
```

## Scope

Update:

- Database
- Contracts
- Infrastructure OTP reuse
- Domain Auth service
- API Auth controller
- Blazor registration/login pages
- Blazor user dashboard/profile page
- RBAC seed/default role behavior

Do not change card registration OTP workflow except where OTP service can be reused.

## Important Behavior Changes

Current login likely uses:

```text
Username + Password
```

New login should use:

```text
PhoneNumber + Password
```

Registration should require:

```text
Username
PhoneNumber
Password
OTP verification
```

## Authorization Model

New self-registered users should receive a default role.

Recommended default role:

```text
Viewer
```

Viewer should be able to:

```text
View own profile
View own card information
View own transactions if supported
```

Viewer should not be able to:

```text
Register cards
Manage cards
Top up cards
Manage packages
Manage roles/permissions
View audit logs
```

## Phase 1: Database Review and Changes

Review `Tbl_User`.

It should support:

```text
UserId
UserName
PhoneNo
PasswordHash
FullName optional
Email optional
IsActive
CreatedDate
UpdatedDate
DeleteFlag
```

If `PhoneNo` does not exist, add it:

```sql
PhoneNo NVARCHAR(20) NOT NULL
```

Add unique active indexes:

```text
UserName unique where DeleteFlag = 0
PhoneNo unique where DeleteFlag = 0
```

Review OTP table:

```text
Tbl_CardRegistrationOtp
```

If this table is card-specific by name, choose one:

### Option A: Reuse Existing Table Temporarily

Reuse the table with different purpose values:

```text
CardRegistration
UserRegistration
```

This is fastest.

### Option B: Rename/Create Generic OTP Table

Preferred long-term:

```text
Tbl_OtpVerification
```

Suggested columns:

```text
OtpId
PhoneNumber
OtpCodeHash
Purpose
ExpiresAt
VerifiedAt
AttemptCount
MaxAttemptCount
CreatedByUserId nullable
CreatedDate
DeleteFlag
```

For this maintenance project, Option A is acceptable if the existing table already works.

## Phase 2: EF Scaffold

If database schema changed, re-run EF Core Database First scaffold.

Expected updated models:

```text
TblUser
TblCardRegistrationOtp or TblOtpVerification
AppDbContext
```

Do not manually edit generated EF files.

## Phase 3: Auth Contracts

Update:

```text
YbsSmartCardSystem.Contracts/Features/Auth/AuthModels.cs
```

Add registration models:

```csharp
public class UserRegistrationSendOtpRequestModel
{
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UserRegistrationSendOtpResponseModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class UserRegisterRequestModel
{
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}

public class UserRegisterResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
```

Change login request:

```csharp
public class LoginRequestModel
{
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
```

Keep `LoginResponseModel`, but ensure it includes:

```text
UserId
UserName
PhoneNumber
FullName optional
Token
ExpiresAt
Roles
```

## Phase 4: User Dashboard Contracts

Create:

```text
YbsSmartCardSystem.Contracts/Features/Auth/UserDashboardModels.cs
```

Suggested models:

```csharp
public class UserDashboardResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public List<UserCardSummaryModel> Cards { get; set; } = [];
}

public class UserCardSummaryModel
{
    public int CardId { get; set; }
    public string CardNum { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string MobileNo { get; set; } = string.Empty;
    public decimal Balance { get; set; }
}
```

Optional later:

```text
Recent transactions
Recent topups
Package status
```

## Phase 5: Reuse OTP Service

Reuse existing `IOtpService`.

Use purpose:

```text
UserRegistration
```

For development, OTP can continue to be logged through API logs.

For production, SMS provider integration can be added later.

Registration OTP rules:

- OTP expires in 5 minutes
- Max 5 attempts
- Store hash only
- Do not store plain OTP
- Invalidate old unverified OTPs for same phone number and purpose

## Phase 6: Update AuthService

Update:

```text
YbsSmartCardSystem.Domain/Features/Auth/AuthService.cs
```

Add methods:

```csharp
Task<Result<UserRegistrationSendOtpResponseModel>> SendUserRegistrationOtpAsync(UserRegistrationSendOtpRequestModel request)
Task<Result<UserRegisterResponseModel>> RegisterAsync(UserRegisterRequestModel request)
```

Update login method:

```csharp
Result<LoginResponseModel> Login(LoginRequestModel request)
```

or async if needed:

```csharp
Task<Result<LoginResponseModel>> LoginAsync(LoginRequestModel request)
```

### Send Registration OTP Rules

Validate:

- Username required
- Username max 100
- Phone number required
- Phone number max 20
- Password required
- Password minimum length, recommended 6 or 8
- Username must not already exist
- Phone number must not already exist

If valid:

- Send OTP using `IOtpService.SendOtpAsync(phoneNumber, "UserRegistration")`

### Register Rules

Validate:

- Username required
- Phone number required
- Password required
- OTP code required
- Username must not already exist
- Phone number must not already exist
- OTP must verify successfully for `UserRegistration`

If valid:

- Hash password with `IPasswordService`
- Create `TblUser`
- Set `PhoneNo`
- Set `IsActive = true`
- Set `DeleteFlag = false`
- Assign default Viewer role in `Tbl_UserRole`
- Consume OTP
- Write audit log if available, e.g. `UserRegistered`

### Login Rules

Login should find user by:

```text
PhoneNo
```

not username.

Validation:

- Phone number required
- Password required
- User must exist
- User must be active
- User must not be deleted
- Password hash must verify

Failed login message:

```text
Invalid phone number or password.
```

Do not reveal whether phone number exists.

## Phase 7: Update AuthController

Update:

```text
YbsSmartCardSystem.Api/Controllers/AuthController.cs
```

Add endpoints:

```text
POST /api/Auth/Register/SendOtp
POST /api/Auth/Register
POST /api/Auth/Login
GET  /api/Auth/Profile
GET  /api/Auth/Dashboard
```

Access rules:

```text
Register/SendOtp -> AllowAnonymous
Register -> AllowAnonymous
Login -> AllowAnonymous
Profile -> Authorize
Dashboard -> Authorize
```

Dashboard returns cards where:

```text
Tbl_Card.MobileNo == current user's PhoneNo
DeleteFlag == false
```

## Phase 8: Update JWT Claims

Update JWT generation to include phone number:

```text
PhoneNumber claim
```

Recommended claim:

```csharp
new Claim(ClaimTypes.MobilePhone, user.PhoneNumber)
```

Update `CurrentUserService` if needed:

```csharp
string? PhoneNumber { get; }
```

## Phase 9: Update Blazor ApiService

Update:

```text
YbsSmartCardSystem.App/Services/ApiService.cs
```

Add methods:

```csharp
Task<Result<UserRegistrationSendOtpResponseModel>> SendUserRegistrationOtp(UserRegistrationSendOtpRequestModel request)
Task<Result<UserRegisterResponseModel>> Register(UserRegisterRequestModel request)
Task<Result<UserDashboardResponseModel>> GetUserDashboard()
```

Update login to send:

```text
PhoneNumber
Password
```

instead of:

```text
UserName
Password
```

## Phase 10: Update Blazor Login Page

Update:

```text
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor
YbsSmartCardSystem.App/Components/Features/Auth/Login.razor.cs
```

Change input:

```text
Username -> Phone number
Password -> Password
```

Submit login request with:

```text
PhoneNumber
Password
```

After login:

- Store token
- Fetch permissions
- Navigate to dashboard or `/`

Recommended redirect:

```text
/dashboard
```

## Phase 11: Add Blazor Register Page

Create:

```text
YbsSmartCardSystem.App/Components/Features/Auth/Register.razor
YbsSmartCardSystem.App/Components/Features/Auth/Register.razor.cs
```

UI steps:

### Step 1: User Details

Fields:

```text
Username
Phone number
Password
Confirm password
Send OTP button
```

### Step 2: OTP

Fields:

```text
OTP code
Register button
```

After successful register:

- Show success message
- Navigate to login

or optionally:

- Auto-login after register

Recommended first version:

```text
Register success -> go to login
```

## Phase 12: Add Dashboard/Profile Page

Create:

```text
YbsSmartCardSystem.App/Components/Features/Auth/UserDashboard.razor
YbsSmartCardSystem.App/Components/Features/Auth/UserDashboard.razor.cs
```

Route:

```text
/dashboard
```

Show:

```text
Username
Phone number
Cards linked to this phone number
Card number
Owner name
Balance
```

If no card exists for the phone number:

```text
No card has been registered for your phone number yet.
```

Do not show other users' cards.

## Phase 13: Navigation

Update:

```text
YbsSmartCardSystem.App/Components/Layout/NavMenu.razor
```

When not logged in:

```text
Login
Register
```

When logged in:

```text
Dashboard/Profile
Logout
```

Admin/Operator card registration links should remain permission-based:

```text
Card.Register
```

Viewer should only see dashboard/profile and permitted read-only items.

## Phase 14: Validation and Security

Password rules:

- Minimum length 6 or 8
- Hash password only
- Never log password
- Never return password hash

Phone rules:

- Required
- Max 20
- Normalize before saving if possible
- Unique among active users

OTP rules:

- Do not expose OTP except in development logs
- Expire OTP
- Limit attempts
- Consume OTP after successful registration

Dashboard security:

- Use current authenticated user's phone number from database or claims
- Do not accept phone number from query string for dashboard lookup

## Phase 15: Verification

Run:

```powershell
dotnet build -c Release
```

Manual tests:

```text
Register with missing username fails
Register with missing phone fails
Register with weak/missing password fails
Send OTP creates/logs OTP
Wrong OTP fails registration
Expired OTP fails registration
Correct OTP creates user
Duplicate username fails
Duplicate phone number fails
New user gets Viewer role
Login with phone number and password succeeds
Login with username no longer works
Login with wrong password fails
After login dashboard shows cards matching user's phone number
Dashboard does not show cards for other phone numbers
Viewer cannot access card registration
Admin and Operator can still access card registration
```

## Expected Result

- Users can self-register with username, phone number, password, and OTP verification.
- Users login with phone number and password.
- New users receive Viewer role by default.
- After login, users see related card information based on their phone number.
- Admin and Operator card registration remains protected by `Card.Register`.
- Viewer users cannot register cards.
