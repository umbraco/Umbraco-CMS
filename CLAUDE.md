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

**Issue Linking**: Add `(closes #IssueID)` to the title for readability, AND include a closing keyword on its own line in the PR body (e.g., `Fixes #IssueID`) so GitHub actually auto-links and auto-closes the issue on merge. GitHub only parses closing keywords (`closes`, `fixes`, `resolves`) from the PR body or commit messages — the title suffix is cosmetic and does **not** trigger auto-close on its own.

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
   - Infrastructure implements contracts that need Infrastructure-owned machinery
   - Web/APIs consume implementations via DI

   **Where service implementations live**: Services whose dependencies are satisfiable from Core interfaces alone (repositories, scope, config, other Core services) live in `Umbraco.Core/Services/` — this covers the majority of domain services (`MemberService`, `ContentService`, `MediaService`, `ContentTypeService`, `EntityService`, `AuditService`, `ExternalMemberService`, etc.). Service implementations only live in `Umbraco.Infrastructure/Services/Implement/` when they genuinely need Infrastructure concerns — Examine indexes (`ContentSearchService`, `MediaSearchService`, `IndexedEntitySearchService`), log files (`LogViewerRepository`), packaging internals (`PackagingService`), webhook firing (`WebhookFiringService`), distributed-job coordination (`DistributedJobService`). When adding a new service, default to Core and only move to Infrastructure if a concrete dependency forces it.

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

## 5. Avoiding Breaking Changes

No binary breaking changes are allowed within a major version. Three patterns are used:

### 5.1 Obsolete Constructor + StaticServiceProvider

When a public class needs new dependencies, obsolete the existing constructor and add a new one. The old constructor delegates to the new one, resolving missing deps via `StaticServiceProvider`.

```csharp
[Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
public MyService(IDependencyA depA)
    : this(
        depA,
        StaticServiceProvider.Instance.GetRequiredService<IDependencyB>())
{
}

public MyService(IDependencyA depA, IDependencyB depB)
{
    _depA = depA;
    _depB = depB;
}
```

**Examples**:
- `ContentCollectionPresentationFactory` - added `FlagProviderCollection`
- `CacheInstructionService` - added `ILastSyncedManager`, `IRepositoryCacheVersionService`
- `DocumentPresentationFactory` - added `FlagProviderCollection`

**Rules**:
- Old constructor marked `[Obsolete("... Scheduled for removal in Umbraco {current-major+2}.")]`
- Old constructor calls new constructor via `: this(...)`
- Uses `StaticServiceProvider.Instance.GetRequiredService<T>()` for new params only
- DI registration must use the NEW constructor (old is for external consumers only)

### 5.2 Obsolete Method + New Overload

When a public method signature needs to change, add the new method/overload and obsolete the old. The obsolete method should call the new one with suitable defaults.

```csharp
[Obsolete("Use the overload taking all parameters. Scheduled for removal in Umbraco 19.")]
public void DoThing(string name)
    => DoThing(name, extraParam: null);

public void DoThing(string name, string? extraParam)
{
    // Real implementation here
}
```

**Rules**:
- Old method marked `[Obsolete]` with removal schedule
- DRY: old method calls new method, providing defaults for new parameters
- All internal callers must be updated to use the new method
- No callers should remain on the obsolete method within the codebase

### 5.3 Default Interface Implementation

When adding methods to a public interface, provide a default implementation so existing external implementations don't break.

```csharp
public interface IMyService
{
    // Existing method
    void ExistingMethod();

    // New method with default implementation
    void NewMethod(string param)
        => ExistingMethod(); // delegate to existing if possible
}
```

**Strategies for the default** (in order of preference):
1. **Use existing interface methods** to satisfy the contract (even if not optimal)
2. **Return a sensible default** like empty collection, null, etc.
3. **Throw `NotImplementedException`** if no reasonable default exists

**Example**: `IContentService.SaveBlueprint` - new overload with `IContent? createdFromContent` has a default impl that calls the old method (ignoring the new param).

**Example**: `IDocumentPresentationFactory.CreateCulturePublishScheduleModels` - full default implementation with logic, uses `StaticServiceProvider` for dependency resolution within the interface.

**Rules**:
- Add `// TODO (V{next-major}): Remove the default implementation when {obsolete method} is removed.` comment
- Default impl should be functionally correct even if not optimal
- If using `StaticServiceProvider` in a default impl, note this is temporary

### 5.4 General Rules

- **Removal policy**: Obsoleted members must remain for at least one full major version before removal. If obsoleted in version N, the earliest removal is version N+2. For example, something obsoleted in v17 is scheduled for removal in v19 (giving the whole of v18 as a deprecation period).
- All `[Obsolete]` attributes must include **"Scheduled for removal in Umbraco {current+2}"**
- Read `version.json` to determine the current major version
- Suppress `CS0618` warnings where obsolete members must call each other:
  ```csharp
  #pragma warning disable CS0618 // Type or member is obsolete
      => OldMethod(param);
  #pragma warning restore CS0618 // Type or member is obsolete
  ```
- Update ALL internal callers to use the new API - no internal code should use obsolete members

---

## 6. Project-Specific Notes

### Centralized Package Management

**NuGet package versions** are centralized in `Directory.Packages.props`. There are two `Directory.Packages.props` files in the source tree, with multi-level merging enabled so the test file inherits from the root:

| File | Scope |
|------|-------|
| `Directory.Packages.props` (root) | Production source code packages — referenced by all `src/**` projects |
| `tests/Directory.Packages.props` | Test-only packages (NUnit, Moq, Bogus, BenchmarkDotNet, etc.) — adds entries on top of the inherited root file |

When updating dependencies, decide which file the package belongs in:
- A package used only by test projects → `tests/Directory.Packages.props`
- A package used by any production project (or by both production and tests) → root `Directory.Packages.props`

```xml
<!-- Individual projects reference WITHOUT version -->
<PackageReference Include="Microsoft.AspNetCore.OpenApi" />

<!-- Versions defined in Directory.Packages.props -->
<PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.0" />
```

**Opt-out**: `src/Umbraco.Web.UI/Umbraco.Web.UI.csproj` sets `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>` and specifies versions inline (for `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Build.Tasks.Core`, `Microsoft.ICU.ICU4C.Runtime`, etc.). Update those versions directly in that csproj when bumping. Two further `Directory.Packages.props` files exist under `templates/` for the project/extension templates and have their own version sets — keep `Microsoft.AspNetCore.OpenApi` aligned between the root file and `templates/UmbracoExtension/`.

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
- Tokens are redacted from client-side responses and passed via secure cookies only (`[redacted]` placeholder)
- ASP.NET Core Data Protection for token encryption
- Configured in `Umbraco.Cms.Api.Common`
- API requests must include credentials (`credentials: include` for fetch)

**Load Balancing Requirement**: All servers must share the same Data Protection key ring.

**Frontend auth pitfalls** — see `src/Umbraco.Web.UI.Client/docs/edge-cases.md` (Auth & Cross-tab section) and `docs/security.md`. Key points:
- Never call `validateToken()` per API request — it revokes the previous reference token (ID2019 errors)
- `window.opener` is set for ANY `window.open()` target, not only OAuth popups — scope guards to the pathname too
- BroadcastChannel does not deliver messages to the sender's own tab

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

### Updating `OpenApi.json` (Management API)

When a PR changes Management API controllers or models, the `OpenApi.json` file in the Management API project must be updated:

1. Run the Umbraco instance locally
2. Open Swagger UI and navigate to the swagger.json link (e.g. `https://localhost:44339/umbraco/swagger/management/swagger.json`)
3. Copy the full JSON content and paste it into `src/Umbraco.Cms.Api.Management/OpenApi.json`

**Important**: Commit only the substantive changes — not IDE-applied formatting (whitespace, reordering, etc.). Extraneous formatting diffs make PRs harder to review and merge-ups more error-prone.

### Backoffice npm Package

The backoffice is published to npm as `@umbraco-cms/backoffice`. Runtime dependencies are provided via importmap; npm peerDependencies provide types only. For full details on dependency hoisting, version range logic, and plugin development, see `/src/Umbraco.Web.UI.Client/CLAUDE.md` → "npm Package Publishing".

### Known Limitations

1. **Circular Dependencies**: Avoided via `Lazy<T>` or event notifications
2. **Multi-Server**: Requires shared Data Protection key ring and synchronized clocks (NTP)
3. **Database Support**: SQL Server, SQLite

---

## 7. CI/CD — Claude AI Assistant

Two GitHub Actions workflows powered by `anthropics/claude-code-action@v1`. Advisory only — does not block merging.

### Workflows

| File | Trigger | Purpose |
|------|---------|---------|
| `claude-review.yml` | `pull_request: [opened, ready_for_review]` | Auto-review every non-draft PR using the `umb-review` skill |
| `claude.yml` | `@claude` comments, issue assign/label | Interactive assistant for PRs and issues |

### Auto-Review (`claude-review.yml`)

Runs the full `.claude/skills/umb-review/SKILL.md` procedure on every newly opened or un-drafted PR. Produces inline comments per finding and one summary comment with a verdict. Skips draft PRs. No turn limit.

### Interactive (`claude.yml`)

Responds to `@claude` mentions on PRs and issues. The trigger phrase is stripped before Claude sees the message, so:

- `@claude review` → light review using `gh pr diff` (not the umb-review skill)
- `@claude fix ...` → implements a fix on a new branch
- `@claude help` → answers questions about the codebase
- `@claude label` → applies labels
- `@claude` (empty) → defaults to `review` on PRs, `help` on issues

Also triggers on issue assignment to `claude` or adding the `claude` label. Gated: only runs when `@claude` appears in the comment/issue body. Max 25 turns.

**Allowed Bash tools**: `gh`, `git`, `npm`, `dotnet` (interactive only; auto-review allows `gh` and `git`).

### Labels

Both workflows apply labels based on content:

**On PRs** (based on changed files):

| Label | Condition |
|-------|-----------|
| `area/frontend` | Files under `src/Umbraco.Web.UI.Client/` |
| `area/backend` | `.cs` files outside the frontend client |
| `area/test` | Only test files changed |
| `category/api` | Management or Delivery API files |
| `category/breaking` | Breaking changes detected |
| `category/localization` | Localization/language files |
| `category/test-automation` | Only test files changed |
| `category/refactor` | Pure refactoring, no new features |
| `category/performance` | Performance-related changes |
| `category/ux` | User-facing changes |
| `category/ui` | UI layer changes |

**On Issues** (based on content): same `area/*` and `category/*` labels, plus `affected/v14` through `affected/v17` and `affected/backoffice`.

Labels are only added, never removed. Claude applies only labels it is confident about.

### Key Implementation Notes

- **Checkout required** — the action internally runs `git fetch origin main` for trusted file restoration. Without `actions/checkout`, it fails with `fatal: not a git repository`.
- **`id-token: write` permission** — required for OIDC token exchange with the Claude GitHub App.
- **Trigger phrase stripping** — the action strips `@claude` from comments before passing to Claude. Prompts must reference commands without the prefix (e.g., `review` not `@claude review`).
- **PR number injection** — the interactive workflow injects the PR/issue number into the prompt via `${{ github.event.issue.number }}` since Claude can't discover it from `gh pr view` when checked out on `main`.

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

### Integration Test Database Configuration

Integration tests are configured in `tests/Umbraco.Tests.Integration/appsettings.Tests.json`.

The `Tests:Database:DatabaseType` setting controls which database is used:
- `"SQLite"` (default) - No external dependencies
- `"LocalDb"` - Uses SQL Server LocalDB, required for SQL Server-specific tests (e.g., page-level locking, `sys.dm_tran_locks`)

SQL Server-specific tests use `BaseTestDatabase.IsSqlite()` to skip when running on SQLite.

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
- **Backoffice Frontend**: `/src/Umbraco.Web.UI.Client/CLAUDE.md` - Lit web components, extension system, auth client

**Important**: When working on backoffice client code (anything under `src/Umbraco.Web.UI.Client/`), read `/src/Umbraco.Web.UI.Client/CLAUDE.md` first. It contains action-specific checklists (deprecation, testing, security, etc.) that are not duplicated here.

### Getting Help

- **Official Docs**: https://docs.umbraco.com/
- **Contributing Guide**: `.github/CONTRIBUTING.md`
- **Issues**: https://github.com/umbraco/Umbraco-CMS/issues
- **Community**: https://forum.umbraco.com/
- **Releases**: https://releases.umbraco.com/

---

**This repository follows a layered architecture with strict dependency rules. The Core defines contracts, Infrastructure implements them, and Web/APIs consume them. Each layer can be understood independently, but dependencies always flow inward toward Core.**
