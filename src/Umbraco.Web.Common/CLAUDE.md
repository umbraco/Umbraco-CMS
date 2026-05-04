# Umbraco.Web.Common

Shared ASP.NET Core web functionality for Umbraco CMS. Provides controllers, middleware, application builder extensions, security/identity, localization, and the UmbracoContext request pipeline.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Package ID**: Umbraco.Cms.Web.Common
**Namespace**: Umbraco.Cms.Web.Common
**Dependencies**: Umbraco.Examine.Lucene, Umbraco.PublishedCache.HybridCache, MiniProfiler, Serilog, Asp.Versioning

---

## 1. Architecture

### Project Purpose

This project provides the web layer foundation for Umbraco CMS:

1. **DI Registration** - `AddUmbraco()` entry point for service registration
2. **Application Pipeline** - `UmbracoApplicationBuilder` for middleware ordering
3. **Controllers** - Base classes for frontend and backoffice controllers
4. **UmbracoContext** - Request-scoped context with published content access
5. **Security** - Member sign-in, identity management, authentication middleware
6. **Middleware** - Boot failure handling, preview authentication, request routing

### Folder Structure

```
Umbraco.Web.Common/
├── ApplicationBuilder/
│   ├── IUmbracoApplicationBuilder.cs          # Builder interface
│   ├── IUmbracoPipelineFilter.cs              # Pipeline customization hooks
│   └── UmbracoApplicationBuilder.cs           # Middleware orchestration (170 lines)
├── AspNetCore/
│   ├── AspNetCoreIpResolver.cs                # IP address resolution
│   ├── AspNetCorePasswordHasher.cs            # Identity password hashing
│   ├── AspNetCoreRequestAccessor.cs           # Request access abstraction
│   └── OptionsMonitorAdapter.cs               # Config monitoring wrapper
├── Controllers/
│   ├── IRenderController.cs                   # Frontend controller marker
│   ├── IVirtualPageController.cs              # Virtual page support
│   ├── PluginController.cs                    # Plugin controller base (104 lines)
│   ├── UmbracoApiController.cs                # Legacy API controller (obsolete)
│   ├── UmbracoAuthorizedController.cs         # Backoffice authorized base
│   └── UmbracoController.cs                   # Base MVC controller (13 lines)
├── DependencyInjection/
│   └── UmbracoBuilderExtensions.cs            # AddUmbraco(), AddUmbracoCore() (338 lines)
├── Extensions/
│   ├── BlockGridTemplateExtensions.cs         # Block grid Razor helpers
│   ├── BlockListTemplateExtensions.cs         # Block list Razor helpers
│   ├── HttpRequestExtensions.cs               # Request utility methods
│   ├── ImageCropperTemplateExtensions.cs      # Image crop Razor helpers
│   ├── LinkGeneratorExtensions.cs             # URL generation helpers
│   ├── WebApplicationExtensions.cs            # BootUmbracoAsync() (32 lines)
│   └── [30+ extension classes]
├── Filters/
│   ├── BackOfficeCultureFilter.cs             # Culture detection
│   ├── DisableBrowserCacheAttribute.cs        # Cache control headers
│   ├── UmbracoMemberAuthorizeAttribute.cs     # Member authorization
│   └── ValidateUmbracoFormRouteStringAttribute.cs
├── Localization/
│   ├── UmbracoBackOfficeIdentityCultureProvider.cs
│   └── UmbracoPublishedContentCultureProvider.cs
├── Middleware/
│   ├── BootFailedMiddleware.cs                # Startup failure handling (81 lines)
│   └── PreviewAuthenticationMiddleware.cs     # Preview mode auth (84 lines)
├── Routing/
│   ├── IAreaRoutes.cs                         # Area routing interface
│   ├── IRoutableDocumentFilter.cs             # Content routing filter
│   ├── UmbracoRouteValues.cs                  # Route data container (60 lines)
│   └── UmbracoVirtualPageRoute.cs             # Virtual page routing
├── Security/
│   ├── MemberSignInManager.cs                 # Member sign-in (350 lines)
│   ├── UmbracoSignInManager.cs                # Base sign-in manager
│   ├── IMemberSignInManager.cs                # Sign-in interface
│   └── EncryptionHelper.cs                    # Surface controller encryption
├── Templates/
│   └── TemplateRenderer.cs                    # Razor template rendering
├── UmbracoContext/
│   ├── UmbracoContext.cs                      # Request context (173 lines)
│   └── UmbracoContextFactory.cs               # Context creation (75 lines)
├── UmbracoHelper.cs                           # Template helper (427 lines)
└── Umbraco.Web.Common.csproj                  # Project configuration (48 lines)
```

### Request Pipeline Flow

```
WebApplication.BootUmbracoAsync()
        ↓
UseUmbraco().WithMiddleware()
        ↓
┌──────────────────────────────┐
│ UmbracoApplicationBuilder    │
│  ├─ RunPrePipeline()         │
│  ├─ BootFailedMiddleware     │
│  ├─ UseUmbracoCore()         │
│  ├─ UseStaticFiles()         │
│  ├─ UseRouting()             │
│  ├─ UseAuthentication()      │
│  ├─ UseAuthorization()       │
│  ├─ UseRequestLocalization() │
│  └─ RunPostPipeline()        │
└──────────────────────────────┘
        ↓
WithEndpoints() → Content routing
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### DI Entry Point (DependencyInjection/UmbracoBuilderExtensions.cs)

**AddUmbraco()** (lines 77-123) - Creates `IUmbracoBuilder` and registers core services:
```csharp
public static IUmbracoBuilder AddUmbraco(
    this IServiceCollection services,
    IWebHostEnvironment webHostEnvironment,
    IConfiguration config)
```

Key registrations:
- Sets `DataDirectory` for database paths (line 97-99)
- Creates `HttpContextAccessor` singleton (line 104-105)
- Initializes `AppCaches` with request-scoped cache
- Creates `TypeLoader` for assembly scanning
- Returns `UmbracoBuilder` for fluent configuration

**AddUmbracoCore()** (lines 131-173) - Registers ASP.NET Core-specific services:
- `IHostingEnvironment` → `AspNetCoreHostingEnvironment`
- `IBackOfficeInfo` → `AspNetCoreBackOfficeInfo`
- `IDbProviderFactoryCreator` with SQL syntax providers
- Telemetry providers and application lifetime

**AddWebComponents()** (lines 222-288) - Registers web-specific services:
- Session with `UMB_SESSION` cookie (line 229)
- API versioning configuration
- Password hasher, cookie manager, IP resolver
- `UmbracoHelper`, `UmbracoContextFactory`
- All middleware as singletons

### Application Builder (ApplicationBuilder/UmbracoApplicationBuilder.cs)

Orchestrates middleware registration in correct order.

**WithMiddleware()** (lines 49-65):
1. `RunPrePipeline()` - Custom filters before Umbraco middleware
2. `RegisterDefaultRequiredMiddleware()` - Core middleware stack
3. `RunPostPipeline()` - Custom filters after
4. User-provided middleware callback

**RegisterDefaultRequiredMiddleware()** (lines 70-104):
```csharp
UseUmbracoCoreMiddleware();
AppBuilder.UseUmbracoMediaFileProvider();
AppBuilder.UseUmbracoBackOfficeRewrites();
AppBuilder.UseStaticFiles();
AppBuilder.UseUmbracoPluginsStaticFiles();
AppBuilder.UseRouting();
AppBuilder.UseAuthentication();
AppBuilder.UseAuthorization();
AppBuilder.UseAntiforgery();
AppBuilder.UseRequestLocalization();
AppBuilder.UseSession();
```

**Pipeline Filter Hooks** (IUmbracoPipelineFilter):
- `OnPrePipeline()` - Before any Umbraco middleware
- `OnPreRouting()` - Before UseRouting()
- `OnPostRouting()` - After UseRouting()
- `OnPostPipeline()` - After all Umbraco middleware
- `OnEndpoints()` - Before endpoint mapping

### UmbracoContext (UmbracoContext/UmbracoContext.cs)

Request-scoped container for Umbraco request state.

**Key Properties**:
- `Content` → `IPublishedContentCache` (line 102)
- `Media` → `IPublishedMediaCache` (line 105)
- `Domains` → `IDomainCache` (line 108)
- `PublishedRequest` → Routed content for request (line 111)
- `InPreviewMode` → Preview cookie detected (lines 140-152)
- `OriginalRequestUrl` / `CleanedUmbracoUrl` - Request URLs

**Preview Detection** (lines 154-166):
```csharp
private void DetectPreviewMode()
{
    // Check preview cookie, verify not backoffice request
    var previewToken = _cookieManager.GetCookieValue(Constants.Web.PreviewCookieName);
    _previewing = _previewToken.IsNullOrWhiteSpace() == false;
}
```

### UmbracoHelper (UmbracoHelper.cs)

Template helper for Razor views (scoped lifetime).

**Content Retrieval** (lines 192-317):
- `Content(id)` - Get by int, Guid, string, or Udi
- `Content(ids)` - Batch retrieval
- `ContentAtRoot()` - Root content items

**Media Retrieval** (lines 320-424):
- Same pattern as content

**Dictionary** (lines 102-189):
- `GetDictionaryValue(key)` - Localized string lookup
- `GetDictionaryValueOrDefault(key, defaultValue)`
- `CultureDictionary` - Current culture dictionary

### Controller Base Classes

| Controller | Purpose | Features |
|------------|---------|----------|
| `UmbracoController` | Base MVC controller | Simple base, debug InstanceId |
| `UmbracoAuthorizedController` | Backoffice controllers | `[Authorize(BackOfficeAccess)]`, `[DisableBrowserCache]` |
| `PluginController` | Plugin/package controllers | UmbracoContext, Services, AppCaches, ProfilingLogger |
| `UmbracoApiController` | Legacy API controller | **Obsolete** - Use ASP.NET Core ApiController |
| `IRenderController` | Frontend rendering marker | Route hijacking support |

**PluginController** (lines 18-104):
- Provides `UmbracoContext`, `DatabaseFactory`, `Services`, `AppCaches`
- Static metadata caching with `ConcurrentDictionary<Type, PluginControllerMetadata>`
- Auto-discovers `[PluginController]` and `[IsBackOffice]` attributes

### Member Sign-In (Security/MemberSignInManager.cs)

ASP.NET Core Identity sign-in manager for members.

**Key Features**:
- External login with auto-linking (lines 112-142)
- Two-factor authentication support (lines 162-172)
- Auto-link and create member accounts (lines 180-267)

**Auto-Link Flow** (lines 180-267):
1. Check if auto-link enabled and email available
2. Find or create member by email
3. Call `OnAutoLinking` callback for customization
4. Add external login link
5. Sign in or request 2FA

**Result Types** (lines 320-348):
- `ExternalLoginSignInResult.NotAllowed` - Login refused by callback
- `AutoLinkSignInResult.FailedNoEmail` - No email from provider
- `AutoLinkSignInResult.FailedCreatingUser` - User creation failed
- `AutoLinkSignInResult.FailedLinkingUser` - Link creation failed

### Middleware

**BootFailedMiddleware** (lines 17-81):
- Intercepts requests when `RuntimeLevel == BootFailed`
- Debug mode: Rethrows exception for stack trace
- Production: Shows `BootFailed.html` error page

**PreviewAuthenticationMiddleware** (lines 22-84):
- Adds backoffice identity to principal for preview requests
- Skips client-side requests and backoffice paths
- Uses `IPreviewService.TryGetPreviewClaimsIdentityAsync()`

---

## 4. Routing

### UmbracoRouteValues (Routing/UmbracoRouteValues.cs)

Container for routed request data:
- `PublishedRequest` - The resolved content request
- `ControllerActionDescriptor` - MVC routing info
- `TemplateName` - Resolved template name
- `DefaultActionName` = "Index"

### Route Hijacking

Implement `IRenderController` to handle specific content types:
```csharp
public class ProductController : Controller, IRenderController
{
    public IActionResult Index() => View();
}
```

### Virtual Pages

Implement `IVirtualPageController` for URL-to-content mapping without physical content nodes.

---

## 5. Project-Specific Notes

### Warning Suppressions (csproj lines 10-22)

Multiple analyzer warnings suppressed:
- SA1117, SA1401, SA1134 - StyleCop formatting
- ASP0019 - Header dictionary usage
- CS0618/SYSLIB0051 - Obsolete references
- IDE0040/SA1400 - Access modifiers
- SA1649 - File name matching

### InternalsVisibleTo

```xml
<InternalsVisibleTo>Umbraco.Tests.UnitTests</InternalsVisibleTo>
```

### Known Technical Debt

1. **MVC Global State** (UmbracoBuilderExtensions.cs:210-211): `AddControllersWithViews` modifies global app, order matters
2. **OptionsMonitor Hack** (AspNetCore/OptionsMonitorAdapter.cs:6): Temporary workaround for TypeLoader during ConfigureServices
3. **DisposeResources TODO** (UmbracoContext.cs:168-171): Empty dispose method marked for removal
4. **SignIn Manager Sharing** (MemberSignInManager.cs:319,325): Could share code with backoffice sign-in

### Session Configuration

Default session cookie (lines 227-231):
```csharp
options.Cookie.Name = "UMB_SESSION";
options.Cookie.HttpOnly = true;
```

Can be overridden by calling `AddSession` after `AddWebComponents`.

---

## Quick Reference

### Startup Flow

```csharp
// Program.cs
builder.Services.AddUmbraco(webHostEnvironment, config)
    .AddUmbracoCore()
    .AddMvcAndRazor()
    .AddWebComponents()
    .AddUmbracoProfiler()
    .Build();

await app.BootUmbracoAsync();
app.UseUmbraco()
    .WithMiddleware(u => u.UseBackOffice())
    .WithEndpoints(u => u.UseBackOfficeEndpoints());
```

### Key Interfaces

| Interface | Implementation | Purpose |
|-----------|----------------|---------|
| `IUmbracoContext` | UmbracoContext | Request state |
| `IUmbracoContextFactory` | UmbracoContextFactory | Context creation |
| `IUmbracoContextAccessor` | (in Core) | Context access |
| `IMemberSignInManager` | MemberSignInManager | Member auth |
| `IUmbracoApplicationBuilder` | UmbracoApplicationBuilder | Pipeline config |

### Extension Method Namespaces

Most extensions are in `Umbraco.Extensions` namespace:
- `HttpRequestExtensions` - Request helpers
- `LinkGeneratorExtensions` - URL generation
- `BlockGridTemplateExtensions` - Block grid rendering
- `ImageCropperTemplateExtensions` - Image cropping

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Core` | Interface contracts |
| `Umbraco.Infrastructure` | Service implementations |
| `Umbraco.Examine.Lucene` | Search dependency |
| `Umbraco.PublishedCache.HybridCache` | Caching dependency |
| `Umbraco.Web.UI` | Main web application (references this) |
| `Umbraco.Cms.Api.Common` | API layer (references this) |
