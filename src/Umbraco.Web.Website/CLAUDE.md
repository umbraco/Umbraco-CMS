# Umbraco.Web.Website

Front-end website functionality for Umbraco CMS. Provides Surface controllers, member authentication/registration, content routing, and Razor view engine support for rendering published content.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Package ID**: Umbraco.Cms.Web.Website
**Namespace**: Umbraco.Cms.Web.Website
**Dependencies**: Umbraco.Web.Common

---

## 1. Architecture

### Project Purpose

This project provides the front-end website layer:

1. **Surface Controllers** - POST/GET controllers for forms and user interactions
2. **Member Authentication** - Login, registration, profile, 2FA, external logins
3. **Content Routing** - Dynamic route value transformation for Umbraco content
4. **View Engines** - Custom Razor view locations and profiling wrappers
5. **Public Access** - Protected content with member authentication

### Folder Structure

```
Umbraco.Web.Website/
├── ActionResults/
│   ├── RedirectToUmbracoPageResult.cs        # Redirect to content by key (151 lines)
│   ├── RedirectToUmbracoUrlResult.cs         # Redirect to current URL
│   └── UmbracoPageResult.cs                  # Return current page with ViewData
├── Cache/
│   └── PartialViewCacheInvalidators/
│       └── MemberPartialViewCacheInvalidator.cs
├── Collections/
│   ├── SurfaceControllerTypeCollection.cs
│   └── SurfaceControllerTypeCollectionBuilder.cs
├── Controllers/
│   ├── SurfaceController.cs                  # Base front-end controller (113 lines)
│   ├── UmbLoginController.cs                 # Member login (128 lines)
│   ├── UmbRegisterController.cs              # Member registration
│   ├── UmbProfileController.cs               # Member profile management
│   ├── UmbLoginStatusController.cs           # Login status checking
│   ├── UmbExternalLoginController.cs         # OAuth/external login
│   ├── UmbTwoFactorLoginController.cs        # 2FA authentication
│   ├── RenderNoContentController.cs          # No content page
│   └── UmbracoRenderingDefaultsOptions.cs    # Default rendering options
├── DependencyInjection/
│   ├── UmbracoBuilderExtensions.cs           # AddWebsite() (91 lines)
│   └── UmbracoBuilder.MemberIdentity.cs      # AddMemberExternalLogins() (22 lines)
├── Extensions/
│   ├── HtmlHelperRenderExtensions.cs         # Razor render helpers
│   ├── LinkGeneratorExtensions.cs            # URL generation
│   ├── TypeLoaderExtensions.cs               # Surface controller discovery
│   ├── UmbracoApplicationBuilder.Website.cs  # UseWebsite() middleware
│   └── WebsiteUmbracoBuilderExtensions.cs
├── Middleware/
│   └── BasicAuthenticationMiddleware.cs      # Basic auth support
├── Models/
│   ├── LoginModel.cs                         # (in Web.Common)
│   ├── RegisterModel.cs                      # Registration form
│   ├── ProfileModel.cs                       # Profile form
│   ├── RegisterModelBuilder.cs               # Model builder
│   ├── ProfileModelBuilder.cs                # Profile builder
│   ├── MemberModelBuilderBase.cs             # Builder base class
│   ├── MemberModelBuilderFactory.cs          # Factory for builders
│   └── NoNodesViewModel.cs                   # Empty site view model
├── Routing/
│   ├── UmbracoRouteValueTransformer.cs       # Dynamic route transformer (330 lines)
│   ├── UmbracoRouteValuesFactory.cs          # Creates UmbracoRouteValues
│   ├── ControllerActionSearcher.cs           # Finds controller actions
│   ├── PublicAccessRequestHandler.cs         # Protected content handling
│   ├── FrontEndRoutes.cs                     # Route definitions
│   ├── EagerMatcherPolicy.cs                 # Early route matching
│   ├── NotFoundSelectorPolicy.cs             # 404 handling
│   ├── SurfaceControllerMatcherPolicy.cs     # Surface controller matching
│   ├── IControllerActionSearcher.cs
│   ├── IPublicAccessRequestHandler.cs
│   └── IUmbracoRouteValuesFactory.cs
├── Security/
│   ├── MemberAuthenticationBuilder.cs        # Auth configuration
│   └── MemberExternalLoginsBuilder.cs        # External login providers
├── ViewEngines/
│   ├── ProfilingViewEngine.cs                # MiniProfiler wrapper
│   ├── ProfilingViewEngineWrapperMvcViewOptionsSetup.cs
│   ├── PluginRazorViewEngineOptionsSetup.cs  # Plugin view locations
│   └── RenderRazorViewEngineOptionsSetup.cs  # Render view locations
└── Umbraco.Web.Website.csproj                # Project file (34 lines)
```

### Request Flow

```
HTTP Request → UmbracoRouteValueTransformer.TransformAsync()
                    ↓
              Check UmbracoContext exists
                    ↓
              Check routable document filter
                    ↓
              RouteRequestAsync() → IPublishedRouter
                    ↓
              UmbracoRouteValuesFactory.CreateAsync()
                    ↓
              PublicAccessRequestHandler (protected content)
                    ↓
              Check for Surface controller POST (ufprt)
                    ↓ (if POST)
              HandlePostedValues() → Surface Controller
                    ↓ (else)
              Return route values → Render Controller
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### SurfaceController (Controllers/SurfaceController.cs)

Base class for front-end form controllers (113 lines).

**Key Features**:
- `[AutoValidateAntiforgeryToken]` applied by default (line 19)
- Extends `PluginController` with `IPublishedUrlProvider`
- `CurrentPage` property for accessing routed content (lines 40-53)
- Redirect helpers for content by key/entity (lines 58-102)
- Supports query strings on redirects

### UmbracoRouteValueTransformer (Routing/UmbracoRouteValueTransformer.cs)

Dynamic route value transformer for front-end content routing (330 lines).

**TransformAsync Flow** (lines 120-194):
1. Check `UmbracoContext` exists (skip client-side requests)
2. Check `IRoutableDocumentFilter.IsDocumentRequest()` (skip static files)
3. Check no existing dynamic routing active
4. Check `IDocumentUrlService.HasAny()` → `RenderNoContentController` if empty
5. `RouteRequestAsync()` → create published request via `IPublishedRouter`
6. `UmbracoRouteValuesFactory.CreateAsync()` → resolve controller/action
7. `PublicAccessRequestHandler.RewriteForPublishedContentAccessAsync()` → handle protected content
8. Store `UmbracoRouteValues` in `HttpContext.Features`
9. Check for Surface controller POST via `ufprt` parameter
10. Return route values for controller/action

**Surface Controller POST Handling** (lines 244-309):
- Detects `ufprt` (Umbraco Form Post Route Token) in request
- Decrypts via `EncryptionHelper.DecryptAndValidateEncryptedRouteString()`
- Extracts controller, action, area from encrypted data
- Routes to Surface controller action

**Reserved Keys for ufprt** (lines 321-327):
- `c` = Controller
- `a` = Action
- `ar` = Area

### AddWebsite() (DependencyInjection/UmbracoBuilderExtensions.cs)

Main DI registration entry point (91 lines).

**Key Registrations** (lines 34-90): Surface controller discovery, view engine setup, routing services, matcher policies, member models, distributed cache, ModelsBuilder, and member identity.

### Member Controllers

Built-in Surface controllers for member functionality:

| Controller | Purpose | Key Action |
|------------|---------|------------|
| `UmbLoginController` | Member login | `HandleLogin(LoginModel)` |
| `UmbRegisterController` | Member registration | `HandleRegister(RegisterModel)` |
| `UmbProfileController` | Profile management | `HandleUpdate(ProfileModel)` |
| `UmbLoginStatusController` | Login status | `HandleLogout()` |
| `UmbExternalLoginController` | OAuth login | `ExternalLogin(provider)` |
| `UmbTwoFactorLoginController` | 2FA verification | `HandleTwoFactorLogin()` |

### UmbLoginController (Controllers/UmbLoginController.cs)

Member login Surface controller (128 lines).

**HandleLogin** (lines 53-114): Validates credentials, handles 2FA/lockout, redirects on success. Uses encrypted redirect URLs (lines 120-126).

### RedirectToUmbracoPageResult (ActionResults/RedirectToUmbracoPageResult.cs)

Action result for redirecting to Umbraco content (151 lines).

Implements `IKeepTempDataResult` to preserve TempData. Resolves content by `Guid` key or `IPublishedContent`, supports query strings.

---

## 4. View Engine Configuration

**Custom View Locations**:
- `RenderRazorViewEngineOptionsSetup` - /Views/{controller}/{action}.cshtml
- `PluginRazorViewEngineOptionsSetup` - /App_Plugins/{area}/Views/...
- `ProfilingViewEngine` - MiniProfiler wrapper for view timing

---

## 5. Project-Specific Notes

**Warning Suppressions** (csproj lines 10-18): ASP0019, CS0618, SA1401, SA1649, IDE1006

**InternalsVisibleTo**: Umbraco.Tests.UnitTests, Umbraco.Tests.Integration

**Known Technical Debt**:
1. Load balanced setup needs review (UmbracoBuilderExtensions.cs:47)
2. UmbracoContext re-assignment pattern (UmbracoRouteValueTransformer.cs:229-232)
3. Obsolete constructor without IDocumentUrlService (removal in Umbraco 18)

**Surface Controller Form Token (ufprt)**: Encrypted route token prevents tampering. Hidden field includes controller/action/area encrypted via Data Protection, decrypted in `UmbracoRouteValueTransformer.GetFormInfo()`.

**Public Access**: `PublicAccessRequestHandler` checks member authentication, redirects to login if needed.

---

## Quick Reference

**Startup**:
```csharp
builder.CreateUmbracoBuilder().AddWebsite().AddMembersIdentity().Build();
app.UseUmbraco().WithMiddleware(u => u.UseWebsite()).WithEndpoints(u => u.UseWebsiteEndpoints());
```

### Key Interfaces

| Interface | Implementation | Purpose |
|-----------|----------------|---------|
| `IControllerActionSearcher` | ControllerActionSearcher | Find controller actions |
| `IUmbracoRouteValuesFactory` | UmbracoRouteValuesFactory | Create route values |
| `IPublicAccessRequestHandler` | PublicAccessRequestHandler | Protected content |
| `IRoutableDocumentFilter` | RoutableDocumentFilter | Filter routable requests |

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Web.Common` | Base controllers, UmbracoContext |
| `Umbraco.Core` | Interfaces, routing contracts |
| `Umbraco.Infrastructure` | Service implementations |
| `Umbraco.Web.UI` | Main web application (references this) |
