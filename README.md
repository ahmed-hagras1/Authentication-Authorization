# 🔐 YourAppName — ASP.NET Core Auth Boilerplate

> **First thing to do:** Replace every occurrence of `YourAppName` across the entire solution with your actual business name (e.g., `Shoofly`, `Taskify`, `MedApp`). See the [Rename Guide](#-rename-the-project-for-your-business) for the exact steps.

A production-ready, deeply layered authentication & authorization starter template built on **.NET 8**. It eliminates all repetitive security scaffolding so you can focus immediately on your domain logic. The architecture has been battle-tested with real patterns — not generic tutorials.

---

## 📑 Table of Contents

1. [Why This Boilerplate?](#-why-this-boilerplate)
2. [Architecture & Layer Responsibilities](#-architecture--layer-responsibilities)
3. [Technologies & Patterns](#️-technologies--patterns)
4. [Project Structure (Your Actual Layers)](#-project-structure-your-actual-layers)
5. [How CQRS Works Here](#-how-cqrs-works-here)
6. [The Security Pipeline (What Protects Every Request)](#-the-security-pipeline-what-protects-every-request)
7. [Permissions System (Claim-Based Authorization)](#-permissions-system-claim-based-authorization)
8. [API Endpoints](#-api-endpoints)
9. [Step-by-Step Setup Guide](#-step-by-step-setup-guide)
10. [Rename the Project for Your Business](#-rename-the-project-for-your-business)
11. [Extending the Boilerplate](#-extending-the-boilerplate)
12. [Security Hardening Checklist](#-security-hardening-checklist)
13. [Recommended Next Steps](#-recommended-next-steps)

---

## 💡 Why This Boilerplate?

Every new project needs the same foundation: register users, verify their identity, issue JWT tokens, refresh them safely, revoke sessions on logout, manage roles, and enforce granular permissions. Building that from scratch is 2–3 weeks of careful work that adds zero business value.

This template gives you all of it, wired up correctly, in a 6-layer architecture ready to scale.

---

## 🏛 Architecture & Layer Responsibilities

This solution uses **6 focused layers**. Each has one job. Dependencies always flow inward — outer layers know about inner ones, never the reverse.

```
┌──────────────────────────────────────────────────────────┐
│                   YourAppName.API                        │
│  Controllers · Filters · Middleware · Program.cs         │
│  → Receives HTTP, delegates to MediatR, returns result   │
├──────────────────────────────────────────────────────────┤
│                  YourAppName.Core                        │
│  Commands · Queries · Handlers · Validators · Behaviors  │
│  → All application logic lives here (CQRS hub)           │
├──────────────────────────────────────────────────────────┤
│                 YourAppName.Service                      │
│  IAuthenticationService · IAuthorizationService          │
│  IEmailService · ISmsService · IUserService              │
│  → Orchestrates business operations across repositories  │
├──────────────────────────────────────────────────────────┤
│               YourAppName.Infrastructure                 │
│  AppDbContext · GenericRepository · RefreshTokenRepo     │
│  Seeders · BackgroundService · DI Configuration          │
│  → All data access and external integrations             │
├──────────────────────────────────────────────────────────┤
│                  YourAppName.Data                        │
│  ApplicationUser · ApplicationRole · UserRefreshToken    │
│  JWTSettings · JWTAuthResult · EmailSettings             │
│  → Pure data models, no logic, no dependencies           │
├──────────────────────────────────────────────────────────┤
│                  YourAppName.Shared                      │
│  SharedResourcesKeys · Permissions · Localization .resx  │
│  → Constants and cross-cutting concerns used everywhere  │
└──────────────────────────────────────────────────────────┘
```

## 🏗️ Dependency Rules & Architecture Flow

To maintain a clean separation of concerns and prevent circular dependencies, this solution strictly enforces how projects communicate with one another. 

**Dependency rule in practice:**
* `Core` can reference `Data` and `Shared`.
* `Service` can reference `Data`, `Infrastructure`, and `Shared`.
* `API` references everything.
* `Data` and `Shared` reference nothing inside the solution.

---

## 🛠️ Technologies & Patterns

| Area | Technology | Notes |
|---|---|---|
| Framework | ASP.NET Core 8.0 | LTS release |
| ORM | Entity Framework Core (Code-First) | SQL Server via `UseSqlServer` |
| Identity | ASP.NET Core Identity | Custom `ApplicationUser` + `ApplicationRole` |
| Authentication | JWT Bearer | Access Token + Refresh Token with rotation |
| CQRS | MediatR | Commands and Queries strictly separated |
| Validation | FluentValidation | Runs automatically via MediatR pipeline behavior |
| Mapping | AutoMapper | Used in Query handlers for DTO projection |
| Email | SMTP via `EmailSettings` | Configured from `appsettings.json` |
| SMS | Twilio | `ISmsService` / `SmsService` implementation |
| Rate Limiting | `Microsoft.AspNetCore.RateLimiting` | Fixed window on auth endpoints |
| Localization | `IStringLocalizer<SharedResources>` | English + Arabic (ar-EG) out of the box |
| Background Jobs | `BackgroundService` | Nightly token cleanup at 3:00 AM |
| Logging | Serilog | Console + MS SQL Server sink (already configured in `appsettings.json`) |
| Error Handling | Custom `ErrorHandlerMiddleware` | One place for all exception mapping |

---

## 📁 Project Structure (Your Actual Layers)

```
YourAppName/
│
├── YourAppName.API/
│   ├── Base/
│   │   └── AppControllerBase.cs          ← Smart Mediator injection + NewResult() mapper
│   ├── Controllers/
│   │   ├── AuthController.cs             ← 12 auth endpoints
│   │   └── AuthorizationController.cs   ← 11 authorization/permission endpoints
│   ├── Filters/
│   │   ├── TokenValidationFilter.cs      ← Checks if JWT is revoked in DB (JTI check)
│   │   └── ValidateUserStatusFilter.cs   ← Checks IsActive, SecurityStamp, Lockout, EmailConfirmed
│   ├── MiddleWares/
│   │   └── ErrorHandlerMiddleware.cs     ← Maps exceptions to HTTP status codes
│   ├── Program.cs                        ← Full pipeline setup with seeding + rate limiting
│   └── appsettings.json                  ← Serilog · JWT · Email · Twilio · ConnectionString
│
├── YourAppName.Core/
│   ├── AppMetaData/
│   │   └── Router.cs                     ← All route constants in one place (Api/V1/...)
│   ├── Bases/
│   │   ├── Response.cs                   ← Generic envelope: StatusCode, Data, Message, Errors
│   │   └── ResponseHandler.cs            ← Success/NotFound/BadRequest/Unauthorized factories
│   ├── Behaviors/
│   │   └── ValidationBehavior.cs         ← Auto-validates every Command before handler runs
│   ├── Features/
│   │   ├── Auth/
│   │   │   ├── Commands/
│   │   │   │   ├── Handlers/
│   │   │   │   │   └── AuthenticationCommandHandler.cs  ← Handles all 12 auth commands
│   │   │   │   ├── Models/               ← SignInCommand, LogoutCommand, RefreshTokenCommand...
│   │   │   │   └── Validations/          ← One validator per command
│   │   └── Authorization/
│   │       ├── Commands/
│   │       │   ├── Handlers/
│   │       │   │   └── AuthorizationCommandHandler.cs
│   │       │   ├── Models/               ← AddRoleCommand, UpdateUserRolesCommand...
│   │       │   └── Validations/
│   │       └── Queries/
│   │           ├── Handlers/
│   │           │   └── AuthorizationQueryHandler.cs
│   │           ├── Models/               ← GetRoleListQuery, ManageUserRolesQuery...
│   │           ├── Results/              ← DTOs returned to the controller
│   │           └── Validations/
│   ├── Mapping/
│   │   └── Roles/RoleProfile.cs          ← AutoMapper profile for Role → DTO
│   ├── Wrappers/
│   │   ├── PaginatedResult.cs            ← Ready-to-use pagination wrapper
│   │   └── QueryableExtensions.cs        ← IQueryable → PaginatedResult helper
│   └── Dependencies/
│       └── ModuleCoreDependencies.cs     ← Registers MediatR + FluentValidation + AutoMapper
│
├── YourAppName.Service/
│   ├── Abstracts/
│   │   ├── IAuthenticationService.cs     ← JWT generation, refresh, revoke, password ops
│   │   ├── IAuthorizationService.cs      ← Roles, claims, user status management
│   │   ├── IEmailService.cs
│   │   ├── ISmsService.cs                ← Twilio integration
│   │   └── IUserService.cs               ← Verification codes
│   ├── Implementations/
│   │   ├── AuthenticationService.cs
│   │   ├── AuthorizationService.cs
│   │   ├── EmailService.cs
│   │   ├── SmsService.cs
│   │   └── UserService.cs
│   └── Dependencies/
│       └── ModuleServiceDependencies.cs  ← AddScoped for all services
│
├── YourAppName.Infrastructure/
│   ├── Data/
│   │   └── AppDbContext.cs               ← IdentityDbContext<ApplicationUser, ApplicationRole, string>
│   ├── Abstracts/
│   │   ├── IGenericRepositoryAsync.cs
│   │   └── IRefreshTokenRepository.cs
│   ├── InfrastructureBases/
│   │   └── GenericRepositoryAsync.cs     ← CRUD + transactions + AsNoTracking
│   ├── Repositories/
│   │   └── RefreshTokenRepository.cs
│   ├── BackgroundServices/
│   │   └── TokenCleanupBackgroundService.cs  ← Runs nightly at 3AM, purges expired tokens
│   ├── Seeder/
│   │   ├── RoleSeeder.cs                 ← Seeds roles + all permission claims to Admin
│   │   ├── UserSeeder.cs                 ← Seeds default admin user
│   │   └── CountrySeeder.cs              ← Seeds country lookup table
│   └── Dependencies/
│       ├── ModuleInfrastructureDependencies.cs  ← DbContext + Generic repo + RefreshToken repo
│       └── IdentityDependencies.cs              ← Identity + JWT Bearer configuration
│
├── YourAppName.Data/
│   ├── Entities/
│   │   ├── Country.cs
│   │   └── Identity/
│   │       ├── ApplicationUser.cs        ← Extends IdentityUser: FullName, IsActive, PreferredLanguage, CountryId
│   │       ├── ApplicationRole.cs        ← Extends IdentityRole
│   │       └── UserRefreshToken.cs       ← JWTId (JTI), IsUsed, IsRevoked, ExpiryDate
│   ├── Helpers/
│   │   ├── JWTSettings.cs
│   │   ├── JWTAuthResult.cs              ← AccessToken + RefreshToken returned to client
│   │   ├── EmailSettings.cs
│   │   └── UserClaimModel.cs
│   └── Results/
│       └── Authorization/                ← ManageRoleClaimsResult, ManageUserClaimsResult
│
└── YourAppName.Shared/
    ├── Resources/
    │   ├── SharedResourcesKeys.cs        ← 60+ typed constants for all localized messages
    │   ├── SharedResources.resx          ← English translations
    │   └── SharedResources.ar.resx       ← Arabic translations
    └── Security/
        └── Permissions.cs               ← All permission strings (Roles.View, Users.EditClaims...)
```

---

## ⚡ How CQRS Works Here

**CQRS** splits every operation into a **Command** (write/change state) or a **Query** (read only). **MediatR** is the in-process bus that dispatches them to the correct handler.

### Complete flow for `POST Api/V1/Auth/SignIn`:

```
HTTP POST → AuthController.SignIn([FromBody] SignInCommand)
    │
    ▼
_mediator.Send(command)
    │
    ▼
MediatR Pipeline
    ├── ValidationBehavior<SignInCommand, Response<JWTAuthResult>>
    │       → Runs SignInValidator (FluentValidation)
    │       → If invalid: throws ValidationException
    │       → ErrorHandlerMiddleware catches it → 422 Unprocessable Entity
    │
    ▼  (validation passed)
AuthenticationCommandHandler.Handle(SignInCommand, ct)
    ├── FindByEmailAsync / FindByPhoneNumber
    ├── CheckPasswordAsync
    ├── Checks: IsActive, EmailConfirmed / PhoneNumberConfirmed
    ├── _authenticationService.GetJWTToken(user)
    │       → Builds Claims (NameIdentifier, Email, SecurityStamp, JTI...)
    │       → Signs with SymmetricSecurityKey (your JWT Key)
    │       → Saves UserRefreshToken to DB
    │       → Returns JWTAuthResult { AccessToken, RefreshToken }
    └── return Success(result)  ← ResponseHandler factory method
    │
    ▼
AppControllerBase.NewResult(response)
    → Matches StatusCode → returns OkObjectResult / BadRequestObjectResult / etc.
    │
    ▼
HTTP 200 OK { succeeded: true, data: { accessToken, refreshToken } }
```

### Adding a new Command — the pattern:

```csharp
// 1. Data/Helpers or Core/Features — define the command (in Core layer)
public class RegisterClientCommand : IRequest<Response<string>>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string CompanyName { get; set; }
}

// 2. Core/Features/Auth/Commands/Validations
public class RegisterClientCommandValidator : AbstractValidator<RegisterClientCommand>
{
    public RegisterClientCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(6);
        RuleFor(x => x.CompanyName).NotEmpty();
    }
}

// 3. Core/Features/Auth/Commands/Handlers — extend the existing handler or create a new one
public async Task<Response<string>> Handle(RegisterClientCommand request, CancellationToken ct)
{
    var user = new ApplicationUser { Email = request.Email, UserName = request.Email, FullName = request.CompanyName };
    var result = await _userManager.CreateAsync(user, request.Password);
    if (!result.Succeeded) return BadRequest<string>(result.Errors.First().Description);
    await _userManager.AddToRoleAsync(user, "Client"); // auto-assign role
    return Success<string>(_localizer[SharedResourcesKeys.Created]);
}

// 4. API — add endpoint (no changes to existing code needed)
[HttpPost(Router.ClientRouting.Register)]
[AllowAnonymous]
public async Task<IActionResult> RegisterClient([FromBody] RegisterClientCommand command, CancellationToken ct)
    => NewResult(await Mediator.Send(command, ct));
```

---

## 🛡 The Security Pipeline (What Protects Every Request)

Every authenticated request passes through **4 layers of protection** before reaching a controller action:

```
Incoming Request
      │
      ▼
1. ErrorHandlerMiddleware         ← Catches ALL unhandled exceptions app-wide
      │
      ▼
2. JWT Bearer Middleware           ← Validates token signature, expiry, issuer, audience
   (built-in ASP.NET Core)         If invalid → 401 immediately, no further processing
      │
      ▼
3. ValidateUserStatusFilter        ← IAsyncAuthorizationFilter (runs before action)
   Checks:
   • User still exists in DB
   • user.IsActive == true
   • SecurityStamp matches token (detects password changes, admin resets)
   • Account not locked out (LockoutEnd)
   • Email confirmed (if RequireConfirmedEmail is on)
      │
      ▼
4. TokenValidationFilter           ← IAsyncActionFilter (runs just before action)
   Checks:
   • Reads JTI claim from token
   • Queries UserRefreshTokens: IsRevoked OR IsUsed
   • If true → short-circuits with 401 "session no longer active"
   (This is what makes logout actually work — the token is mathematically
    valid but blocked by the DB record)
      │
      ▼
5. Controller Action               ← Only executes if all 4 layers pass ✅
```

**Why this matters:** Standard JWT is stateless — a stolen token works until it expires. The `TokenValidationFilter` + `UserRefreshToken` table give you **token revocation** without needing Redis or a blocklist service.

---

## 🔑 Permissions System (Claim-Based Authorization)

This project goes beyond simple role-based auth. Every endpoint is protected by a **fine-grained permission claim**, not just a role name.

### How it works:

**1. Permissions are defined as constants in `YourAppName.Shared/Security/Permissions.cs`:**

```csharp
public static class Permissions
{
    public const string Type = "Permission"; // claim type

    public static class Roles
    {
        public const string View   = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit   = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Users
    {
        public const string ViewRoles   = "Permissions.Users.ViewRoles";
        public const string EditRoles   = "Permissions.Users.EditRoles";
        public const string ViewClaims  = "Permissions.Users.ViewClaims";
        public const string EditClaims  = "Permissions.Users.EditClaims";
        public const string ChangeStatus = "Permissions.Users.ChangeStatus";
    }
}
```

**2. `Program.cs` auto-registers every constant as an Authorization Policy via reflection:**

```csharp
// This converts EVERY string in Permissions into a named Policy automatically.
// You never manually call AddPolicy for each one.
var permissionClasses = typeof(Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);
foreach (var module in permissionClasses)
{
    var permissions = module.GetFields(...).Select(fi => fi.GetRawConstantValue()?.ToString());
    foreach (var permission in permissions)
        options.AddPolicy(permission, policy => policy.RequireClaim(Permissions.Type, permission));
}
```

**3. Controllers use the constants directly — no magic strings:**

```csharp
[Authorize(Policy = Permissions.Roles.View)]
[HttpGet(Router.AuthorizationRouting.GetRolesList)]
public async Task<IActionResult> GetRoleList(CancellationToken ct)
    => NewResult(await Mediator.Send(new GetRoleListQuery(), ct));
```

**4. The `RoleSeeder` assigns all permission claims to the Admin role at startup.**

**To add a new permission module** (e.g., Products):

```csharp
// In Permissions.cs — add a new nested class
public static class Products
{
    public const string View   = "Permissions.Products.View";
    public const string Create = "Permissions.Products.Create";
    public const string Edit   = "Permissions.Products.Edit";
    public const string Delete = "Permissions.Products.Delete";
}
// That's it. The reflection loop in Program.cs picks it up automatically.
```

---

## 📍 API Endpoints

All routes follow the pattern: `Api/V1/{Controller}/{Action}`

### Authentication (`Api/V1/Auth/...`)

![Authentication Endpoints](https://raw.githubusercontent.com/ahmed-hagras1/Authentication-Authorization/master/Endpoints/Authentication.png)

| Method | Route | Description | Rate Limited | Auth |
|--------|-------|-------------|:---:|:---:|
| `POST` | `/Auth/SignIn` | Login with Email or Phone + Password | ✅ 5/min | ❌ |
| `POST` | `/Auth/VerifyCode` | Confirm email/phone verification code | ✅ | ❌ |
| `POST` | `/Auth/ResendCode` | Resend verification code (email or SMS) | ✅ | ❌ |
| `POST` | `/Auth/RefreshToken` | Exchange refresh token for new access token | ❌ | ❌ |
| `POST` | `/Auth/RevokeToken` | Revoke a specific refresh token | ❌ | ✅ |
| `POST` | `/Auth/RevokeAllSessions` | Invalidate all sessions for a user | ❌ | ✅ |
| `POST` | `/Auth/Logout` | Revoke current session | ❌ | ✅ |
| `POST` | `/Auth/ForgotPassword` | Send OTP reset code via email or SMS | ✅ | ❌ |
| `POST` | `/Auth/VerifyResetCode` | Verify the OTP reset code | ✅ | ❌ |
| `POST` | `/Auth/ResetPassword` | Set a new password after OTP verification | ✅ | ❌ |
| `POST` | `/Auth/ChangePassword` | Change password while authenticated | ❌ | ✅ |

### Authorization (`Api/V1/Authorization/...`)

![Authorization Endpoints](https://raw.githubusercontent.com/ahmed-hagras1/Authentication-Authorization/master/Endpoints/Authorization.png)

| Method | Route | Description | Required Permission |
|--------|-------|-------------|---|
| `GET` | `/Authorization/Role/List` | Get all roles | `Permissions.Roles.View` |
| `GET` | `/Authorization/Role/{id}` | Get role by ID | `Permissions.Roles.View` |
| `POST` | `/Authorization/Role/Create` | Create a new role | `Permissions.Roles.Create` |
| `PUT` | `/Authorization/Role/Edit` | Edit a role name | `Permissions.Roles.Edit` |
| `DELETE` | `/Authorization/Role/Delete/{id}` | Delete a role (blocks if has users) | `Permissions.Roles.Delete` |
| `GET` | `/Authorization/User-Roles/{id}` | Get user's current role assignments | `Permissions.Users.ViewRoles` |
| `POST` | `/Authorization/User-Roles/Update` | Assign/remove roles from a user | `Permissions.Users.EditRoles` |
| `GET` | `/Authorization/User-Claims/{id}` | Get user's permission claims | `Permissions.Users.ViewClaims` |
| `PUT` | `/Authorization/User-Claims/Update` | Update user's permission claims | `Permissions.Users.EditClaims` |
| `GET` | `/Authorization/Role-Claims/{id}` | Get role's permission claims | `Permissions.Roles.Edit` |
| `PUT` | `/Authorization/Role-Claims/Update` | Update role's permission claims | `Permissions.Roles.Edit` |
| `PUT` | `/Authorization/User-Status/Change` | Activate/deactivate a user account | `Permissions.Users.ChangeStatus` |

---

## 🚀 Step-by-Step Setup Guide

### Step 1 — Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) or SQL Server LocalDB (included with Visual Studio)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (v17.8+) or VS Code with C# Dev Kit
- Optional: [Twilio account](https://www.twilio.com/) for SMS verification

### Step 2 — Clone

```bash
git clone https://github.com/ahmed-hagras1/Authentication-Authorization.git
cd Authentication-Authorization
```

### Step 3 — Configure `appsettings.json`

Open `YourAppName.API/appsettings.json` and fill in every placeholder:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourAppNameDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "Key": "REPLACE_WITH_MIN_32_CHARACTER_SECRET_KEY_HERE",
    "Issuer": "https://yourapp.com",
    "Audience": "https://yourapp.com",
    "DurationInMinutes": 60,
    "RefreshTokenDurationInDays": 30,
    "ValidateIssuer": true,
    "ValidateAudience": true,
    "ValidateLifetime": true,
    "ValidateIssuerSigningKey": true
  },
  "EmailSettings": {
    "Email": "no-reply@yourapp.com",
    "Password": "YOUR_SMTP_APP_PASSWORD",
    "Host": "smtp.gmail.com",
    "Port": 587
  },
  "TwilioSettings": {
    "AccountSID": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "AuthToken": "your_auth_token",
    "FromPhoneNumber": "+1234567890"
  }
}
```

> ⚠️ **Never commit real secrets.** Use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) for development:
> ```bash
> dotnet user-secrets set "JwtSettings:Key" "your-real-secret-key"
> ```
> For production use environment variables, Azure Key Vault, or AWS Secrets Manager.

### Step 4 — Apply Migrations

The `DbContext` lives in `YourAppName.Infrastructure`, so you must target it explicitly.

**Package Manager Console (Visual Studio):**
```powershell
# Set Default Project → YourAppName.Infrastructure
# Set Startup Project → YourAppName.API

Add-Migration InitialCreate -OutputDir Data/Migrations
Update-Database
```

**CLI:**
```bash
dotnet ef migrations add InitialCreate \
  --project YourAppName.Infrastructure \
  --startup-project YourAppName.API \
  --output-dir Data/Migrations

dotnet ef database update \
  --project YourAppName.Infrastructure \
  --startup-project YourAppName.API
```

### Step 5 — Activate the Seeders

The seeders exist but are commented out. Open `Program.cs` and uncomment:

```csharp
await RoleSeeder.SeedAsync(roleManager);   // Creates Admin, User, etc. + assigns all permission claims
await UserSeeder.SeedAsync(userManager, dbContext);  // Creates default admin@yourapp.com / Admin@123
```

Then open `YourAppName.Infrastructure/Seeder/RoleSeeder.cs` and customize the role names:

```csharp
var systemRoles = new List<string> { "Admin", "Manager", "Client" }; // your roles here
```

### Step 6 — Run

```bash
dotnet run --project YourAppName.API
```

Swagger UI opens at `https://localhost:{PORT}/` (it's set as the root by default).

---

## 🏷️ Rename the Project for Your Business

Do this **before writing any business code**. The project uses `YourAppName` as a placeholder throughout.

| What to change | How |
|---|---|
| All namespaces + class names | VS: Edit → Find & Replace in Files (`Ctrl+Shift+H`) → Replace `YourAppName` with `YourBusiness` |
| `.csproj` file names | Rename each file in Explorer, then update references in `.sln` |
| Solution name | Rename `.sln` file |
| Database name | Update `DefaultConnection` in `appsettings.json` |
| JWT `Issuer` / `Audience` | Update to your actual domain |
| Admin email in `UserSeeder` | Change `admin@YourAppName.com` to `admin@yourdomain.com` |
| Swagger title | Update `SwaggerEndpoint` title in `Program.cs` |

**Quick CLI rename (Linux/macOS/WSL):**
```bash
find . -type f \( -name "*.cs" -o -name "*.csproj" -o -name "*.json" -o -name "*.sln" \) \
  ! -path "./.git/*" ! -path "*/bin/*" ! -path "*/obj/*" \
  -exec sed -i 's/YourAppName/YourBusiness/g' {} +
```

---

## 🔧 Extending the Boilerplate

### Add a new user type with its own registration flow

The `Router.cs` already has `ClientRouting.Register` defined as a placeholder:

```csharp
// In Router.cs
public static class ClientRouting
{
    public const string Prefix = Rule + "Client";
    public const string Register = Prefix + "/Register";
}
```

Create `RegisterClientCommand` → `RegisterClientCommandValidator` → extend `AuthenticationCommandHandler` → add endpoint. The seeder auto-assigns the `Client` role. Zero changes to existing files.

### Add a new Permission module

```csharp
// Permissions.cs
public static class Products
{
    public const string View   = "Permissions.Products.View";
    public const string Create = "Permissions.Products.Create";
    public const string Edit   = "Permissions.Products.Edit";
    public const string Delete = "Permissions.Products.Delete";
}
```

The `Program.cs` reflection loop picks it up automatically. Use it on any new controller:

```csharp
[Authorize(Policy = Permissions.Products.Create)]
[HttpPost(Router.ProductRouting.Create)]
public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    => NewResult(await Mediator.Send(command, ct));
```

### Add a localized response key

1. Add a `const string` to `SharedResourcesKeys.cs`
2. Add the English translation to `SharedResources.resx`
3. Add the Arabic translation to `SharedResources.ar.resx`
4. Use it anywhere: `_localizer[SharedResourcesKeys.YourNewKey]`

### Extend ApplicationUser for your domain

```csharp
// In Data/Entities/Identity/ApplicationUser.cs
public string? Address { get; set; }
public string? ProfileImageUrl { get; set; }
public DateTime? DateOfBirth { get; set; }

// Then add a migration:
// Add-Migration AddUserProfileFields
// Update-Database
```

---

## 🛡️ Security Hardening Checklist

Before going live, work through every item:

- [ ] **JWT Key** — minimum 32 characters, cryptographically random. Use a secret manager, never hardcode.
- [ ] **Refresh Token Rotation** — when a refresh token is used, `IsUsed = true` is set. Verify this in `AuthenticationService.GetRefreshToken`.
- [ ] **CORS** — change `AllowAnyOrigin()` to `WithOrigins("https://yourfrontend.com")` in production.
- [ ] **HTTPS** — `RequireHttpsMetadata = false` is fine for local dev. Set to `true` in production.
- [ ] **Rate Limiting** — current `AuthBruteForcePolicy` is 5 req/min. Tune per your threat model.
- [ ] **Password Policy** — currently: min 6 chars, lowercase, digit. Increase `RequiredLength` and enable `RequireNonAlphanumeric` for higher security.
- [ ] **Account Lockout** — set `options.Lockout.MaxFailedAccessAttempts = 5` and `DefaultLockoutTimeSpan` in `IdentityDependencies.cs`.
- [ ] **Email Confirmation** — set `options.SignIn.RequireConfirmedEmail = true` in identity options. The `ValidateUserStatusFilter` already enforces this on every request.
- [ ] **Security Stamp** — `ValidateUserStatusFilter` checks `SecurityStamp` on every request. Do not skip this — it instantly invalidates tokens when a user changes password or is modified by an admin.
- [ ] **Serilog SQL Sink** — update the `connectionString` in `appsettings.json` under `Serilog.WriteTo`. The table `SystemLogs` is auto-created.
- [ ] **Token Cleanup** — `TokenCleanupBackgroundService` runs at 3:00 AM daily via `ExecuteDeleteAsync` (direct SQL, no memory overhead). Verify the schedule fits your timezone needs.
- [ ] **Dependency Scan** — run `dotnet list package --vulnerable` before every release.
- [ ] **Seed Passwords** — the `UserSeeder` default password is `Admin@123`. Change it on first login or via environment variable injection.

---

## 📈 Recommended Next Steps

These are not in the boilerplate yet, prioritized by business impact:

| Priority | Addition | What it Solves |
|---|---|---|
| 🔴 High | **Uncomment & configure RoleSeeder + UserSeeder** | Without seeding, no admin user and no permissions exist |
| 🔴 High | **Restrict CORS in production** | `AllowAnyOrigin` is dangerous in a live environment |
| 🔴 High | **Add your first business domain** | E.g., `Products`, `Orders` — the auth layer is your foundation |
| 🟡 Medium | **Unit Tests** (xUnit + Moq) | Test Handlers in isolation — they have no HTTP dependency |
| 🟡 Medium | **Integration Tests** (WebApplicationFactory) | Test the full HTTP + middleware pipeline |
| 🟡 Medium | **Redis for Refresh Tokens** | Faster token lookup + instant revocation without DB hits |
| 🟡 Medium | **Azure Key Vault / AWS Secrets** | Eliminate all secrets from `appsettings.json` in production |
| 🟡 Medium | **Health Checks** | `builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>()` |
| 🟡 Medium | **Docker + docker-compose** | Reproducible dev environment; ship with confidence |
| 🟢 Low | **GitHub Actions CI/CD** | Auto-build, test, and deploy on push to `main` |
| 🟢 Low | **Swagger JWT Auth header** | Configure SwaggerGen to show the Authorize button |
| 🟢 Low | **Audit Log Table** | Track who changed what and when (add `CreatedBy`, `UpdatedAt` to entities) |
| 🟢 Low | **Soft Delete** | Add `IsDeleted` to entities and filter via EF Core global query filters |

### Enable Swagger JWT Authorization Button

```csharp
// In Program.cs, replace AddSwaggerGen() with:
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});
```

### Health Check Quick Start

```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --project YourAppName.API
```

```csharp
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
app.MapHealthChecks("/health");
```

---

## 🤝 Contributing

1. Fork → `git checkout -b feature/your-feature`
2. Follow the layer conventions strictly — no business logic in controllers, no DB access in handlers
3. Add or update FluentValidation validators and unit tests for your handler
4. Open a pull request with a clear description of what changed and why

---

## 📄 License

MIT — free to use, modify, and ship in personal and commercial products.

---

*Built with 🔐 to give .NET developers a secure foundation they can trust from day one.*
