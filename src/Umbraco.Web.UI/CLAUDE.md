# Umbraco.Web.UI

Main ASP.NET Core web application for running Umbraco CMS. This is the development test site that references all Umbraco packages and provides a minimal startup configuration for testing and development.

**Project Type**: ASP.NET Core Web Application
**SDK**: Microsoft.NET.Sdk.Web
**Target Framework**: net10.0
**IsPackable**: false (not published as NuGet package)
**Namespace**: Umbraco.Cms.Web.UI

---

## 1. Architecture

### Project Purpose

This is the **runnable Umbraco instance** used for development and testing. It:

1. **References All Umbraco Packages** - Via the `Umbraco.Cms` meta-package
2. **Provides Minimal Startup** - Simple `Program.cs` with builder pattern
3. **Includes Development Tools** - Backoffice development mode support
4. **Demonstrates Patterns** - Example composers, views, and block templates

### Folder Structure

```
Umbraco.Web.UI/
├── Composers/
│   ├── ControllersAsServicesComposer.cs    # DI validation composer (66 lines)
│   └── UmbracoAppAuthenticatorComposer.cs  # 2FA example (commented out)
├── Properties/
│   └── launchSettings.json                  # Debug profiles (29 lines)
├── Views/
│   ├── _ViewImports.cshtml                  # Global view imports
│   ├── page.cshtml                          # Generic page template
│   ├── BlockPage.cshtml                     # Block page template
│   ├── BlockTester.cshtml                   # Block testing template
│   └── Partials/
│       ├── blockgrid/                       # Block grid partials
│       │   ├── area.cshtml
│       │   ├── areas.cshtml
│       │   ├── items.cshtml
│       │   └── default.cshtml
│       ├── blocklist/                       # Block list partials
│       │   ├── Components/textBlock.cshtml
│       │   └── default.cshtml
│       └── singleblock/                     # Single block partials
│           └── default.cshtml
├── wwwroot/
│   └── favicon.ico                          # Site favicon
├── umbraco/                                 # Runtime data directory
│   ├── Data/                                # SQLite database, temp files
│   │   ├── Umbraco.sqlite.db               # Development database
│   │   └── TEMP/                           # ModelsBuilder, DistCache
│   ├── Logs/                               # Serilog JSON logs
│   └── models/                             # Generated content models
├── appsettings.json                         # Production config
├── appsettings.Development.json             # Development overrides
├── appsettings.template.json                # Template for new installs
├── appsettings.Development.template.json    # Dev template
├── appsettings-schema.json                  # JSON schema reference
├── appsettings-schema.Umbraco.Cms.json      # Full Umbraco schema (71KB)
├── umbraco-package-schema.json              # Package manifest schema (495KB)
├── Program.cs                               # Application entry point (33 lines)
└── Umbraco.Web.UI.csproj                    # Project file (75 lines)
```

### Project Dependencies

```xml
<ProjectReference Include="..\Umbraco.Cms\Umbraco.Cms.csproj" />
<ProjectReference Include="..\Umbraco.Cms.DevelopmentMode.Backoffice\..." />
```

- **Umbraco.Cms** - Meta-package referencing all Umbraco packages
- **Umbraco.Cms.DevelopmentMode.Backoffice** - Hot reload for backoffice development

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

### Running the Application

```bash
# Run with Kestrel (recommended)
dotnet run --project src/Umbraco.Web.UI

# Run with watch for auto-reload
dotnet watch --project src/Umbraco.Web.UI

# Run from Visual Studio
# Profile: "Umbraco.Web.UI" (https://localhost:44339)
# Profile: "IIS Express" (http://localhost:11000)
```

### Database

SQLite is configured by default for development:
- Location: `src/Umbraco.Web.UI/umbraco/Data/Umbraco.sqlite.db`
- Connection string configured in `appsettings.json`

---

## 3. Key Components

### Program.cs (33 lines)

Minimal Umbraco startup using builder pattern:

```csharp
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
#if UseDeliveryApi
    .AddDeliveryApi()
#endif
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
```

**Preprocessor Directives**:
- `#if UseDeliveryApi` - Enable/disable Delivery API
- `#if UseHttpsRedirect` - Enable/disable HTTPS redirection

### ControllersAsServicesComposer (lines 36-38)

Registers all controllers in the DI container for validation purposes:

```csharp
public void Compose(IUmbracoBuilder builder) => builder.Services
    .AddMvc()
    .AddControllersAsServicesWithoutChangingActivator();
```

**Purpose**: Detects ambiguous constructors in CMS controllers that would cause issues with `ServiceBasedControllerActivator`. Not shipped in `Umbraco.Templates`.

### Launch Profiles (Properties/launchSettings.json)

| Profile | URL | Command |
|---------|-----|---------|
| Umbraco.Web.UI | https://localhost:44339, http://localhost:11000 | Project |
| IIS Express | http://localhost:11000, SSL port 44339 | IISExpress |

---

## 4. Configuration

### appsettings.template.json

Default configuration for new installations:

| Setting | Value | Description |
|---------|-------|-------------|
| `ModelsMode` | `InMemoryAuto` | Auto-generate models in memory |
| `DefaultUILanguage` | `en-us` | Backoffice language |
| `HideTopLevelNodeFromPath` | `true` | Clean URLs |
| `TimeOut` | `00:20:00` | Session timeout |
| `UseHttps` | `false` | HTTPS enforcement |
| `UsernameIsEmail` | `true` | Email as username |
| `UserPassword.RequiredLength` | `10` | Minimum password length |

### appsettings.Development.template.json

Development-specific overrides:

| Setting | Value | Description |
|---------|-------|-------------|
| `Hosting.Debug` | `true` | Debug mode enabled |
| `LuceneDirectoryFactory` | `TempFileSystemDirectoryFactory` | Examine indexes in temp |
| Console logging | `Async` sink | Serilog console output |
| Examine log levels | `Debug` | Detailed Examine logging |

### Auto-Copy Build Target (csproj lines 65-73)

MSBuild targets automatically copy template files if missing (appsettings.json and appsettings.Development.json).

---

## 5. ModelsBuilder

### InMemoryAuto Mode

The project uses `InMemoryAuto` mode:
- Models auto-generated at runtime
- Source stored in `umbraco/Data/TEMP/InMemoryAuto/`
- Compiled assembly in `Compiled/` subdirectory
- `models.hash` tracks content type changes

### Generated Models Location

```
umbraco/models/*.generated.cs    # Source files (for reference)
umbraco/Data/TEMP/InMemoryAuto/  # Runtime compilation
```

### Razor Compilation Settings (csproj lines 50-51)

Razor compilation is disabled for InMemoryAuto mode (`RazorCompileOnBuild=false`, `RazorCompileOnPublish=false`).

---

## 6. ICU Globalization

### App-Local ICU (csproj lines 39-40)

Uses app-local ICU (`Microsoft.ICU.ICU4C.Runtime` v72.1.0.3) for consistent globalization across platforms.

**Note**: Ensure ICU version matches between package reference and runtime option. Changes must also be made to `Umbraco.Templates`.

---

## 7. Project-Specific Notes

### Development vs Production

This project is for **development only**:
- `IsPackable=false` - Not published as NuGet
- `EnablePackageValidation=false` - No package validation
- References `DevelopmentMode.Backoffice` for hot reload

### Package Version Management

```xml
<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>
```

Does NOT use central package management. Versions specified directly:
- `Microsoft.EntityFrameworkCore.Design` - For EF Core migrations tooling
- `Microsoft.Build.Tasks.Core` - Security fix for EFCore.Design dependency
- `Microsoft.ICU.ICU4C.Runtime` - Globalization

### Umbraco Targets Import (csproj lines 19-20)

Imports shared build configuration from `Umbraco.Cms.Targets` project (props and targets).

### Excluded Views (csproj lines 55-57)

Three Umbraco views excluded from content (UmbracoInstall, UmbracoLogin, UmbracoBackOffice) as they come from `Umbraco.Cms.StaticAssets` RCL.

### Known Technical Debt

1. **Warning Suppression** (csproj lines 12-16): `SA1119` - Unnecessary parenthesis to fix

### Runtime Data (umbraco/ directory)

| Directory | Contents | Git Status |
|-----------|----------|------------|
| `umbraco/Data/` | SQLite database, MainDom locks | Ignored |
| `umbraco/Logs/` | Serilog JSON trace logs | Ignored |
| `umbraco/models/` | Generated content models | Ignored |
| `umbraco/Data/TEMP/` | ModelsBuilder, DistCache | Ignored |

---

## Quick Reference

### Essential Commands

```bash
# Run development site
dotnet run --project src/Umbraco.Web.UI

# Clean runtime data (reset database)
rm -rf src/Umbraco.Web.UI/umbraco/Data

# Reset to fresh install
rm src/Umbraco.Web.UI/appsettings.json
rm src/Umbraco.Web.UI/appsettings.Development.json
```

### Important Files

| File | Purpose |
|------|---------|
| `Program.cs` | Application entry point |
| `appsettings.template.json` | Default configuration |
| `appsettings.Development.template.json` | Development overrides |
| `launchSettings.json` | Debug profiles |
| `Composers/ControllersAsServicesComposer.cs` | DI validation |

### URLs

| Environment | URL |
|-------------|-----|
| Development (Kestrel) | https://localhost:44339 |
| Development (HTTP) | http://localhost:11000 |
| Backoffice | /umbraco |
| Installer | /install (on first run) |

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Cms` | Meta-package (all dependencies) |
| `Umbraco.Cms.DevelopmentMode.Backoffice` | Hot reload support |
| `Umbraco.Cms.StaticAssets` | Backoffice/login views |
| `Umbraco.Cms.Targets` | Build configuration |
| `Umbraco.Web.Common` | Web functionality |
