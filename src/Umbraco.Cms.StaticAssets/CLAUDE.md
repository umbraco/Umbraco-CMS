# Umbraco.Cms.StaticAssets

Static assets and Razor views for Umbraco CMS. This is a Razor Class Library (RCL) that packages all backoffice, login, and website assets for deployment.

**Project Type**: Razor Class Library (NuGet package)
**Target Framework**: net10.0
**SDK**: Microsoft.NET.Sdk.Razor

---

## 1. Architecture

### Project Purpose

This project packages all static assets required for Umbraco CMS at runtime:

1. **Razor Views** - Server-rendered pages for backoffice, login, and error pages
2. **Static Web Assets** - JavaScript, CSS, fonts, images served at runtime
3. **Build Integration** - MSBuild targets that compile TypeScript/frontend projects on demand

### Key Characteristic

**No C# Source Code** - This is a pure asset packaging project. All functionality comes from referenced projects (`Umbraco.Cms.Api.Management`, `Umbraco.Web.Website`).

### Folder Structure

```
Umbraco.Cms.StaticAssets/
├── umbraco/                                # Razor Views (server-rendered)
│   ├── UmbracoBackOffice/
│   │   └── Index.cshtml                    # Backoffice SPA shell (69 lines)
│   ├── UmbracoLogin/
│   │   └── Index.cshtml                    # Login page (99 lines)
│   └── UmbracoWebsite/
│       ├── NoNodes.cshtml                  # "No published content" page
│       ├── NotFound.cshtml                 # 404 error page
│       └── Maintenance.cshtml              # Maintenance mode page
│
├── wwwroot/                                # Static Web Assets
│   ├── App_Plugins/
│   │   └── Umbraco.BlockGridEditor.DefaultCustomViews/  # Block grid demo templates
│   └── umbraco/
│       ├── assets/                         # Logos and branding (see README.md)
│       ├── backoffice/                     # Built backoffice SPA (from Umbraco.Web.UI.Client)
│       │   ├── apps/                       # Application modules
│       │   ├── assets/                     # Fonts, language files
│       │   ├── css/                        # Themes and stylesheets
│       │   └── monaco-editor/              # Code editor assets
│       ├── login/                          # Built login SPA (from Umbraco.Web.UI.Login)
│       └── website/                        # Frontend website assets (fonts, CSS)
│
└── Umbraco.Cms.StaticAssets.csproj         # Build configuration (149 lines)
```

### Project Dependencies

```xml
<ProjectReference Include="..\Umbraco.Cms.Api.Management\Umbraco.Cms.Api.Management.csproj" />
<ProjectReference Include="..\Umbraco.Web.Website\Umbraco.Web.Website.csproj" />
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### Razor Views

| View | Purpose | Line Count |
|------|---------|------------|
| `UmbracoBackOffice/Index.cshtml` | Backoffice SPA entry point with `<umb-app>` web component | 69 |
| `UmbracoLogin/Index.cshtml` | Login page with `<umb-auth>` web component | 99 |
| `UmbracoWebsite/NoNodes.cshtml` | Welcome page when no content published | 59 |
| `UmbracoWebsite/NotFound.cshtml` | 404 error page (debug info in debug mode) | 79 |
| `UmbracoWebsite/Maintenance.cshtml` | Maintenance mode during upgrades | 65 |

### Backoffice Index View (umbraco/UmbracoBackOffice/Index.cshtml)

Key injected services:
- `IBackOfficePathGenerator` - Generates backoffice URL paths
- `IPackageManifestService` - Package manifest discovery
- `IJsonSerializer` - JSON serialization for import maps
- `IProfilerHtml` - MiniProfiler integration

Key features:
- **Import Maps** (line 35): `Html.BackOfficeImportMapScriptAsync()` generates JavaScript module import maps
- **Debug Mode** (line 16, 63-66): `?umbDebug=true` query param enables profiler
- **NoScript Fallback** (lines 40-60): Displays message if JavaScript disabled

### Login View (umbraco/UmbracoLogin/Index.cshtml)

Configures `<umb-auth>` web component with attributes:
- `return-url` - Redirect after login
- `logo-image` / `background-image` - Branding URLs from `BackOfficeGraphicsController`
- `username-is-email` - From `SecuritySettings`
- `allow-user-invite` / `allow-password-reset` - Email capability check
- `disable-local-login` - External login provider configuration

---

## 4. Build System

### Frontend Build Integration (csproj lines 36-90)

The `.csproj` contains MSBuild targets that automatically build frontend projects when assets are missing.

**Backoffice Build** (lines 36-90):
```
BackofficeProjectDirectory = ../Umbraco.Web.UI.Client/
BackofficeAssetsPath = wwwroot/umbraco/backoffice
```

**Login Build** (lines 94-148):
```
LoginProjectDirectory = ../Umbraco.Web.UI.Login/
LoginAssetsPath = wwwroot/umbraco/login
```

### Build Targets

| Target | Purpose |
|--------|---------|
| `BuildStaticAssetsPreconditions` | Checks if build needed (Visual Studio only) |
| `RestoreBackoffice` | Runs `npm i` if package-lock changed |
| `BuildBackoffice` | Runs `npm run build:for:cms` |
| `DefineBackofficeAssets` | Registers assets with StaticWebAssets system |
| `CleanBackoffice` | Removes built assets on `dotnet clean` |

### Build Conditions

- **UmbracoBuild Variable**: When set (CI/CD), skips frontend builds (pre-built assets expected)
- **Visual Studio Detection**: `'$(UmbracoBuild)' == ''` indicates VS build
- **preserve.backoffice Marker**: Skip clean if `preserve.backoffice` file exists in solution root

---

## 5. Static Web Assets

### Asset Categories

| Directory | Contents | Served At |
|-----------|----------|-----------|
| `wwwroot/umbraco/assets/` | Branding logos | Via `BackOfficeGraphicsController` API |
| `wwwroot/umbraco/backoffice/` | Built backoffice SPA | `/umbraco/backoffice/*` |
| `wwwroot/umbraco/login/` | Built login SPA | `/umbraco/login/*` |
| `wwwroot/umbraco/website/` | Website assets (fonts, CSS) | `/umbraco/website/*` |
| `wwwroot/App_Plugins/` | Block editor demo views | `/App_Plugins/*` |

### Logo Assets (wwwroot/umbraco/assets/)

Documented in `wwwroot/umbraco/assets/README.md` (16 lines):

| File | Usage | API Endpoint |
|------|-------|--------------|
| `logo.svg` | Backoffice and public sites | `/umbraco/management/api/v1/security/back-office/graphics/logo` |
| `logo_dark.svg` | Login screen (dark mode) | `.../graphics/login-logo-alternative` |
| `logo_light.svg` | Login screen (light mode) | `.../graphics/login-logo` |
| `logo_blue.svg` | Alternative branding | N/A |

### Block Grid Demo Views (wwwroot/App_Plugins/Umbraco.BlockGridEditor.DefaultCustomViews/)

Pre-built AngularJS templates for block grid editor demos:
- `umbBlockGridDemoHeadlineBlock.html`
- `umbBlockGridDemoImageBlock.html`
- `umbBlockGridDemoRichTextBlock.html`
- `umbBlockGridDemoTwoColumnLayoutBlock.html`

---

## 6. Project-Specific Notes

### Static Web Asset Base Path

```xml
<StaticWebAssetBasePath>/</StaticWebAssetBasePath>
```

Assets are served from root path, not under assembly name.

### Compression Disabled

```xml
<CompressionEnabled>false</CompressionEnabled>
```

Comment notes `MapStaticAssets()` is not used (yet).

### Known Technical Debt

1. **Warning Suppression** (`.csproj:14-16`): `NU5123` - File paths too long for NuGet package. TODO indicates files should be renamed.

2. **Excluded Content** (`.csproj:31`): `wwwroot/umbraco/assets/README.md` explicitly excluded from package.

### Backoffice Localization

The backoffice includes language files for 25+ languages in `wwwroot/umbraco/backoffice/assets/lang/`:
- ar, bs, cs, cy, da, de, en, en-us, es, fr, he, hr, it, ja, ko, nb, nl, pl, pt, pt-br, ro, ru, sv, tr, uk, zh, zh-tw

### Monaco Editor

Full Monaco code editor included at `wwwroot/umbraco/backoffice/monaco-editor/` for rich code editing in backoffice.

### Theming

CSS themes in `wwwroot/umbraco/backoffice/css/`:
- `umb-css.css` - Main styles
- `uui-css.css` - UI library styles
- `dark.theme.css` - Dark theme
- `high-contrast.theme.css` - Accessibility theme
- `umbraco-blockgridlayout.css` - Block grid styles
- `rte-content.css` - Rich text editor content styles

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `umbraco/UmbracoBackOffice/Index.cshtml` | Backoffice entry point |
| `umbraco/UmbracoLogin/Index.cshtml` | Login page |
| `wwwroot/umbraco/assets/README.md` | Asset documentation |
| `Umbraco.Cms.StaticAssets.csproj` | Build targets for frontend |

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Web.UI.Client` | Source for backoffice assets (npm build) |
| `Umbraco.Web.UI.Login` | Source for login assets (npm build) |
| `Umbraco.Cms.Api.Management` | Razor view dependencies |
| `Umbraco.Web.Website` | Razor view dependencies |

### Build Commands (Manual)

```bash
# Build backoffice assets (from Umbraco.Web.UI.Client)
cd src/Umbraco.Web.UI.Client
npm install
npm run build:for:cms

# Build login assets (from Umbraco.Web.UI.Login)
cd src/Umbraco.Web.UI.Login
npm install
npm run build
```

### Preserve Assets During Clean

Create marker file to prevent asset deletion:
```bash
touch preserve.backoffice  # In solution root
touch preserve.login       # In solution root
```
