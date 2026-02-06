# Umbraco CMS - Multi-Project Repository

Enterprise-grade CMS built on .NET 10.0. This repository contains 21 production projects organized in a layered architecture with clear separation of concerns.

**Repository**: https://github.com/umbraco/Umbraco-CMS
**License**: MIT
**Main Branch**: `main`

---

## 1. Overview

### What This Repository Contains

**21 Production Projects** organized in 3 main categories:

1. **Core Architecture** (Domain & Infrastructure)
   - `Umbraco.Core` - Interface contracts, domain models, notifications
   - `Umbraco.Infrastructure` - Service implementations, data access, caching

2. **Web & APIs** (Presentation Layer)
   - `Umbraco.Web.UI` - Main ASP.NET Core web application
   - `Umbraco.Web.Common` - Shared web functionality, controllers, middleware
   - `Umbraco.Cms.Api.Management` - Backoffice Management API (REST)
   - `Umbraco.Cms.Api.Delivery` - Content Delivery API (headless)
   - `Umbraco.Cms.Api.Common` - Shared API infrastructure

3. **Specialized Features** (Pluggable Modules)
   - Persistence: EF Core (modern), NPoco (legacy) for SQL Server & SQLite
   - Caching: `PublishedCache.HybridCache` (in-memory + distributed)
   - Search: `Examine.Lucene` (full-text search)
   - Imaging: `Imaging.ImageSharp` v1 & v2 (image processing)
   - Other: Static assets, targets, development tools

**6 Test Projects**:
- `Umbraco.Tests.Common` - Shared test utilities
- `Umbraco.Tests.UnitTests` - Unit tests
- `Umbraco.Tests.Integration` - Integration tests
- `Umbraco.Tests.Benchmarks` - Performance benchmarks
- `Umbraco.Tests.AcceptanceTest` - E2E tests
- `Umbraco.Tests.AcceptanceTest.UmbracoProject` - Test instance

### Key Technologies

- **.NET 10.0** - Target framework for all projects
- **ASP.NET Core** - Web framework
- **Entity Framework Core** - Modern ORM
- **OpenIddict** - OAuth 2.0/OpenID Connect authentication
- **Microsoft.AspNetCore.OpenApi** - OpenAPI document generation
- **Swashbuckle.AspNetCore.SwaggerUI** - Swagger UI for API documentation
- **Lucene.NET** - Full-text search via Examine
- **ImageSharp** - Image processing

---

## 2. Repository Structure

```
Umbraco-CMS/
├── src/                                    # 21 production projects
│   ├── Umbraco.Core/                      # Domain contracts (interfaces only)
│   │   └── CLAUDE.md                      # ⭐ Core architecture guide
│   ├── Umbraco.Infrastructure/            # Service implementations
│   ├── Umbraco.Web.Common/                # Web utilities
│   ├── Umbraco.Web.UI/                    # Main web application
│   ├── Umbraco.Cms.Api.Management/        # Management API
│   ├── Umbraco.Cms.Api.Delivery/          # Delivery API (headless)
│   ├── Umbraco.Cms.Api.Common/            # Shared API infrastructure
│   │   └── CLAUDE.md                      # ⭐ API patterns guide
│   ├── Umbraco.PublishedCache.HybridCache/ # Content caching
│   ├── Umbraco.Examine.Lucene/            # Search indexing
│   ├── Umbraco.Cms.Persistence.EFCore/    # EF Core data access
│   ├── Umbraco.Cms.Persistence.EFCore.Sqlite/
│   ├── Umbraco.Cms.Persistence.EFCore.SqlServer/
│   ├── Umbraco.Cms.Persistence.Sqlite/    # Legacy SQLite
│   ├── Umbraco.Cms.Persistence.SqlServer/ # Legacy SQL Server
│   ├── Umbraco.Cms.Imaging.ImageSharp/    # Image processing v1
│   ├── Umbraco.Cms.Imaging.ImageSharp2/   # Image processing v2
│   ├── Umbraco.Cms.StaticAssets/          # Embedded assets
│   ├── Umbraco.Cms.DevelopmentMode.Backoffice/
│   ├── Umbraco.Cms.Targets/               # NuGet targets
│   └── Umbraco.Cms/                       # Meta-package
│
├── tests/                                  # 6 test projects
│   ├── Umbraco.Tests.Common/
│   ├── Umbraco.Tests.UnitTests/
│   ├── Umbraco.Tests.Integration/
│   ├── Umbraco.Tests.Benchmarks/
│   ├── Umbraco.Tests.AcceptanceTest/
│   └── Umbraco.Tests.AcceptanceTest.UmbracoProject/
│
├── templates/                              # Project templates
│   └── Umbraco.Templates/
│
├── tools/                                  # Build tools
│   └── Umbraco.JsonSchema/
│
├── umbraco.sln                            # Main solution file
├── Directory.Build.props                  # Shared build configuration
├── Directory.Packages.props               # Centralized package versions
├── .editorconfig                          # Code style
└── .globalconfig                          # Roslyn analyzers
```

### Architecture Layers

**Dependency Flow** (unidirectional, always flows inward):

```
Web.UI → Web.Common → Infrastructure → Core
                ↓
          Api.Management → Api.Common → Infrastructure → Core
                ↓
          Api.Delivery → Api.Common → Infrastructure → Core
```

**Key Principle**: Core has NO dependencies (pure contracts). Infrastructure implements Core. Web/APIs depend on Infrastructure.

### Project Dependencies

**Core Layer**:
- `Umbraco.Core` → No dependencies (only Microsoft.Extensions.*)

**Infrastructure Layer**:
- `Umbraco.Infrastructure` → `Umbraco.Core`
- `Umbraco.PublishedCache.*` → `Umbraco.Infrastructure`
- `Umbraco.Examine.Lucene` → `Umbraco.Infrastructure`
- `Umbraco.Cms.Persistence.*` → `Umbraco.Infrastructure`

**Web Layer**:
- `Umbraco.Web.Common` → `Umbraco.Infrastructure` + caching + search
- `Umbraco.Web.UI` → `Umbraco.Web.Common` + all features

**API Layer**:
- `Umbraco.Cms.Api.Common` → `Umbraco.Web.Common`
- `Umbraco.Cms.Api.Management` → `Umbraco.Cms.Api.Common`
- `Umbraco.Cms.Api.Delivery` → `Umbraco.Cms.Api.Common`

---

## 3. Teamwork & Collaboration

### Branching Strategy

- **Main branch**: `main` (protected)
- **Branch naming convention**: `v<version>/<type>/<description>`

**Format**: `v{major-version}/{type}/{kebab-case-description}`

**Version**: Read from `version.json` in the repository root. Use the major version number (e.g., `v17` for version 17.x.x).

**Types**:
| Type | Use Case |
|------|----------|
| `feature` | New feature being introduced to the product |
| `bugfix` | Fix to an existing issue with the product |
| `qa` | Adding or updating unit, integration, or end-to-end tests |
| `improvement` | Update to something that already exists but isn't broken (UI finessing, refactoring) |
| `task` | Update that doesn't directly impact product behavior (dependency updates, build pipeline) |

**Description**: A short, kebab-case description (a few words). This should be prefixed with the GitHub issue number if the update is related to resolving a tracked issue.

**Examples**:
```
v17/bugfix/12345-correct-display-of-pending-migrations
v17/feature/add-webhook-support
v17/improvement/optimize-content-cache
v17/qa/add-media-service-tests
v17/task/update-ef-core-dependency
```

See `.github/CONTRIBUTING.md` for full guidelines.

### Pull Request Process

- **PR Template**: `.github/pull_request_template.md`
- **Required CI Checks**:
  - All tests pass
  - Code formatting (dotnet format)
  - No build warnings
- **Merge Strategy**: Squash and merge (via GitHub UI)
- **Reviews**: Required from code owners

#### PR Naming Convention

Use the format: `Area: Description (closes #IssueID)`

**Examples**:
| Area | Description | Issue |
|------|-------------|-------|
| Relations: | Move persistence of relations from repository into notification handlers | (closes #00000) |
| Management API: | Correct the population of the parent for sibling items when retrieved under a folder | |
| Docs: | Updated contributing guidelines to welcome contributions on bugfixes | |

**Area**: The feature or aspect affected (e.g., UFM, TipTap, Docs, Segmentation, Migrations). Helps readers quickly understand what is being changed.

**Description Best Practices**:
- Include the area of change (Relations, Management API, etc.)
- Describe the change and its impact
- Be specific, not vague (describe "a golden retriever" not just "a dog")

**Issue Linking**: Add `(closes #IssueID)` to auto-close linked issues on merge.

### Commit Messages

Follow Conventional Commits format:
```
<type>(<scope>): <description>

Types: feat, fix, docs, style, refactor, test, chore
Scope: project name (core, web, api, etc.)

Examples:
feat(core): add IContentService.GetByIds method
fix(api): resolve null reference in schema handler
docs(web): update routing documentation
```

### Code Owners

Project ownership is distributed across teams. Check individual project directories for ownership.

---

## 4. Architecture Patterns

### Core Architectural Decisions

1. **Layered Architecture with Dependency Inversion**
   - Core defines contracts (interfaces)
   - Infrastructure implements contracts
   - Web/APIs consume implementations via DI

2. **Interface-First Design**
   - All services defined as interfaces in Core
   - Enables testing, polymorphism, extensibility

3. **Notification Pattern** (not C# events)
   - See `/src/Umbraco.Core/CLAUDE.md` → "2. Notification System (Event Handling)"

4. **Composer Pattern** (DI registration)
   - See `/src/Umbraco.Core/CLAUDE.md` → "3. Composer Pattern (DI Registration)"

5. **Scoping Pattern** (Unit of Work)
   - See `/src/Umbraco.Core/CLAUDE.md` → "5. Scoping Pattern (Unit of Work)"

6. **Attempt Pattern** (operation results)
   - `Attempt<TResult, TStatus>` instead of exceptions
   - Strongly-typed operation status enums

### Key Design Patterns Used

- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Scoping for transactions
- **Builder Pattern** - `ProblemDetailsBuilder` for API errors
- **Strategy Pattern** - OpenAPI handlers (schema ID, operation ID)
- **Options Pattern** - All configuration via `IOptions<T>`
- **Factory Pattern** - Content type factories
- **Mediator Pattern** - Notification aggregator

---

## 5. Project-Specific Notes

### Centralized Package Management

**All NuGet package versions** are centralized in `Directory.Packages.props`. Individual projects do NOT specify versions.

```xml
<!-- Individual projects reference WITHOUT version -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" />

<!-- Versions defined in Directory.Packages.props -->
<PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
```

### Build Configuration

- `Directory.Build.props` - Shared properties (target framework, company, copyright)
- `.editorconfig` - Code style rules
- `.globalconfig` - Roslyn analyzer rules

### Persistence Layer - NPoco and EF Core

The repository contains BOTH (actively supported):
- **Current**: NPoco-based persistence (`Umbraco.Cms.Persistence.Sqlite`, `Umbraco.Cms.Persistence.SqlServer`) - widely used and fully supported
- **Future**: EF Core-based persistence (`Umbraco.Cms.Persistence.EFCore.*`) - migration in progress

**Note**: The codebase is actively migrating to EF Core, but NPoco remains the primary persistence layer and is not deprecated. Both are fully supported.

### Authentication: OpenIddict

All APIs use **OpenIddict** (OAuth 2.0/OpenID Connect):
- Reference tokens (not JWT) for better security
- **Secure cookie-based token storage** (v17+) - tokens stored in HTTP-only cookies with `__Host-` prefix
- Tokens are redacted from client-side responses and passed via secure cookies only
- ASP.NET Core Data Protection for token encryption
- Configured in `Umbraco.Cms.Api.Common`
- API requests must include credentials (`credentials: include` for fetch)

**Load Balancing Requirement**: All servers must share the same Data Protection key ring.

### Content Caching Strategy

**HybridCache** (`Umbraco.PublishedCache.HybridCache`):
- In-memory cache + distributed cache support
- Published content only (not draft)
- Invalidated via notifications and cache refreshers

### API Versioning

APIs use `Asp.Versioning.Mvc`:
- Management API: `/umbraco/management/api/v{version}/*`
- Delivery API: `/umbraco/delivery/api/v{version}/*`
- OpenAPI docs: `/umbraco/openapi/management.json`, `/umbraco/openapi/delivery.json`
- Swagger UI: `/umbraco/openapi/`

### Backoffice npm Package Structure

The backoffice (`Umbraco.Web.UI.Client`) is published to npm as **`@umbraco-cms/backoffice`** with a plugin architecture:

#### Architecture Overview

- **Multi-workspace structure**: Subprojects in `src/libs/*`, `src/packages/*`, `src/external/*`
- **Export model**: All exports defined in root `package.json` → `./exports` field
- **Importmap-driven runtime**: Dependencies provided at runtime via importmap (single source of truth)
- **Build-time types**: TypeScript types come from npm peerDependencies
- **Plugin model**: Developers create plugins that import from `@umbraco-cms/backoffice/*` exports

#### Dependency Hoisting Strategy

When building for npm (`npm pack`), the `cleanse-pkg.js` script hoists subproject dependencies to root `peerDependencies` with intelligent version range conversion:

**Version Range Logic** (uses `semver` package):

1. **Pre-release (0.x.y)**: Convert to explicit range
   - Input: `^0.85.0` or `0.85.0`
   - Output: `>=0.85.0 <1.0.0`
   - Rationale: Pre-release caret only allows patch updates, explicit range allows minor upgrades within 0.x.x
   - Example: Plugin can use `@hey-api/openapi-ts@0.91.1` while backoffice uses `0.85.0`

2. **Stable with caret (^X.Y.Z where X ≥ 1)**: Keep as-is
   - Input: `^3.3.1`
   - Output: `^3.3.1` (unchanged)
   - Rationale: Caret already implements correct semantics for stable versions

3. **Stable exact versions (X.Y.Z where X ≥ 1)**: Add caret
   - Input: `3.16.0`
   - Output: `^3.16.0`
   - Rationale: Normalizes to conventional semver format

#### Key Dependencies

**Runtime via importmap** (types available from peerDependencies):
- `lit`, `rxjs`, `@umbraco-ui/uui` - Core framework
- `monaco-editor`, `@tiptap/*` - Feature-specific editors
- `@hey-api/openapi-ts` - HTTP client type generation

**Build-time only** (not hoisted):
- `vite`, `typescript`, `eslint` - Dev tooling

#### Plugin Development Implications

Plugin developers should:
- **Declare explicit dependencies** in their own `package.json` (avoid relying on transitive deps)
- **Understand the version ranges**: `>=0.85.0 <1.0.0` means they can use newer pre-release versions
- **Know that types match npm ranges**, but runtime comes from importmap (managed by backoffice)
- **When `@hey-api` hits 1.0.0**: Published constraint will automatically become `^1.0.0`

#### Implementation Details

- Script location: `src/Umbraco.Web.UI.Client/devops/publish/cleanse-pkg.js`
- Runs as `prepack` hook before npm pack
- Uses `semver.minVersion()` for robust version range parsing
- Generates single source of truth for importmap versions

### Known Limitations

1. **Circular Dependencies**: Avoided via `Lazy<T>` or event notifications
2. **Multi-Server**: Requires shared Data Protection key ring and synchronized clocks (NTP)
3. **Database Support**: SQL Server, SQLite

---

## Quick Reference

### Essential Commands

```bash
# Build solution
dotnet build

# Run all tests
dotnet test

# Run specific test category
dotnet test --filter "Category=Integration"

# Format code
dotnet format

# Pack all projects
dotnet pack -c Release
```

### Key Projects

| Project | Type | Description |
|---------|------|-------------|
| **Umbraco.Core** | Library | Interface contracts and domain models |
| **Umbraco.Infrastructure** | Library | Service implementations and data access |
| **Umbraco.Web.UI** | Application | Main web application (Razor/MVC) |
| **Umbraco.Cms.Api.Management** | Library | Management API (backoffice) |
| **Umbraco.Cms.Api.Delivery** | Library | Delivery API (headless CMS) |
| **Umbraco.Cms.Api.Common** | Library | Shared API infrastructure |
| **Umbraco.PublishedCache.HybridCache** | Library | Published content caching |
| **Umbraco.Examine.Lucene** | Library | Full-text search indexing |

### Important Files

- **Solution**: `umbraco.sln`
- **Build Config**: `Directory.Build.props`, `Directory.Packages.props`
- **Code Style**: `.editorconfig`, `.globalconfig`
- **Documentation**: `/CLAUDE.md`, `/src/Umbraco.Core/CLAUDE.md`, `/src/Umbraco.Cms.Api.Common/CLAUDE.md`

### Project-Specific Documentation

For detailed information about individual projects, see their CLAUDE.md files:
- **Core Architecture**: `/src/Umbraco.Core/CLAUDE.md` - Service contracts, notification patterns
- **API Infrastructure**: `/src/Umbraco.Cms.Api.Common/CLAUDE.md` - OpenAPI, authentication, serialization

### Getting Help

- **Official Docs**: https://docs.umbraco.com/
- **Contributing Guide**: `.github/CONTRIBUTING.md`
- **Issues**: https://github.com/umbraco/Umbraco-CMS/issues
- **Community**: https://forum.umbraco.com/
- **Releases**: https://releases.umbraco.com/

---

**This repository follows a layered architecture with strict dependency rules. The Core defines contracts, Infrastructure implements them, and Web/APIs consume them. Each layer can be understood independently, but dependencies always flow inward toward Core.**
