# Umbraco CMS - Management API

RESTful API for Umbraco backoffice operations. Manages content, media, users, and system configuration through OpenAPI-documented endpoints.

**Project**: `Umbraco.Cms.Api.Management`
**Type**: ASP.NET Core Web API Library
**Files**: 1,317 C# files across 54+ controller domains

---

## 1. Architecture

### Target Framework
- **.NET 10.0** (`net10.0`)
- **C# 12** with nullable reference types enabled
- **ASP.NET Core** Web API

### Application Type
**REST API Library** - Plugged into Umbraco.Web.UI, provides the Management API surface for backoffice operations.

### Key Technologies
- **Web Framework**: ASP.NET Core MVC with `Asp.Versioning.Mvc` (v1.0 currently)
- **OpenAPI**: Microsoft.AspNetCore.OpenApi with custom transformers, Swagger UI via Swashbuckle
- **Authentication**: OpenIddict via `Umbraco.Cms.Api.Common` (reference tokens, not JWT)
- **Authorization**: Policy-based with `IAuthorizationService`
- **Validation**: FluentValidation via base controllers
- **Serialization**: System.Text.Json with custom converters
- **Mapping**: Manual presentation factories (no AutoMapper)
- **Patching**: JsonPatch.Net for PATCH operations
- **Real-time**: SignalR hubs (`BackofficeHub`, `ServerEventHub`)
- **DI**: Microsoft.Extensions.DependencyInjection via `ManagementApiComposer`

### Project Structure
```
src/Umbraco.Cms.Api.Management/
├── Controllers/                   # 54+ domain-specific controller folders
│   ├── Document/                  # Document (content) CRUD + publish/unpublish
│   ├── Media/                     # Media CRUD + upload
│   ├── Member/                    # Member management
│   ├── User/                      # User management
│   ├── DataType/                  # Data type configuration
│   ├── DocumentType/              # Content type schemas
│   ├── Template/                  # Razor template management
│   ├── Dictionary/                # Localization dictionary
│   ├── Language/                  # Language/culture config
│   ├── Security/                  # Auth, login, external logins
│   ├── Install/                   # Installation wizard
│   ├── Upgrade/                   # Upgrade operations
│   ├── LogViewer/                 # Log browsing
│   ├── HealthCheck/               # Health check dashboard
│   ├── Webhook/                   # Webhook management
│   └── [48 more domains...]
│
├── ViewModels/                    # Request/response DTOs (one folder per domain)
├── Factories/                     # Domain model → ViewModel converters
├── Services/                      # Business logic (thin layer over Core.Services)
├── Mapping/                       # ViewModel → domain model mappers
├── Security/                      # Auth providers, sign-in manager, external logins
├── OpenApi/                       # OpenAPI transformers (schema, operation, security)
├── Routing/                       # Route configuration, SignalR hubs
├── DependencyInjection/           # Service registration (55+ files)
├── Middleware/                    # Preview, server events
├── Configuration/                 # IOptions configurators
├── Filters/                       # Action filters
├── Serialization/                 # JSON converters
└── OpenApi.json                   # Embedded OpenAPI spec (1.3MB)
```

### Dependencies
- **Umbraco.Cms.Api.Common** - Shared API infrastructure (base controllers, OpenAPI config)
- **Umbraco.Infrastructure** - Service implementations, data access
- **Umbraco.PublishedCache.HybridCache** - Published content queries
- **JsonPatch.Net** - JSON Patch (RFC 6902) support
- **Microsoft.AspNetCore.OpenApi** - OpenAPI document generation
- **Swashbuckle.AspNetCore.SwaggerUI** - Swagger UI

### Design Patterns
1. **Controller-per-Operation** - Each endpoint is a separate controller class
   - Example: `CreateDocumentController`, `UpdateDocumentController`, `DeleteDocumentController`
   - Enables fine-grained authorization and operation-specific logic
   - **Responsibilities**: entrypoint/routing, authorization and mapping
   - **avoid**: business logic directly in controllers (there are a few known violations)

2. **Presentation Factory Pattern** - Factories convert domain models to ViewModels
   - Example: `IDocumentEditingPresentationFactory` (src/Umbraco.Cms.Api.Management/Factories/)
   - Separation: Controllers → Factories → ViewModels

3. **Attempt Pattern** - Operations return `Attempt<TResult, TStatus>` for status-based error handling
   - Controllers map status enums to HTTP status codes via helper methods

4. **Authorization Service Pattern** - All authorization via `IAuthorizationService`, not attributes
   - Checked in base controller methods (see `ManagementApiControllerBase`)

5. **Options Pattern** - All configuration via `IOptions<T>` (security, routing, OpenAPI)

6. **SignalR Event Broadcasting** - Real-time notifications via `BackofficeHub` and `ServerEventHub`

---

## 2. Commands

### Build & Run
```bash
# Build
dotnet build src/Umbraco.Cms.Api.Management

# Test (tests in ../../tests/Umbraco.Tests.Integration and Umbraco.Tests.UnitTests)
dotnet test --filter "FullyQualifiedName~Management"

# Pack (for NuGet distribution)
dotnet pack src/Umbraco.Cms.Api.Management -c Release
```

### Code Quality
```bash
# Format code
dotnet format src/Umbraco.Cms.Api.Management

# Build with warnings (note: some warnings suppressed, see .csproj line 23)
dotnet build src/Umbraco.Cms.Api.Management /p:TreatWarningsAsErrors=true
```

### OpenAPI Documentation
The project embeds a pre-generated `OpenApi.json` (1.3MB). To regenerate:
```bash
# Run Umbraco.Web.UI, access /umbraco/openapi/
# Download JSON from /umbraco/openapi/management.json
```

### Package Management
```bash
# Add package (versions centralized in Directory.Packages.props)
dotnet add src/Umbraco.Cms.Api.Management package [PackageName]

# Check for vulnerable packages
dotnet list src/Umbraco.Cms.Api.Management package --vulnerable
```

### Environment Setup
1. **Prerequisites**: .NET 10 SDK
2. **IDE**: Visual Studio 2022 or Rider (with .editorconfig support)
3. **Configuration**: Inherits from `Umbraco.Web.UI` appsettings (no app settings in this library)

---

## 3. Style Guide

### Project-Specific Patterns

**Controller Naming** (line examples from CreateDocumentController.cs:16):
```csharp
[ApiVersion("1.0")]
public class CreateDocumentController : CreateDocumentControllerBase
```
- Pattern: `{Verb}{Entity}Controller` (e.g., `CreateDocumentController`, `UpdateMediaController`)
- Base class: `{Verb}{Entity}ControllerBase` for shared logic
- **Critical**: One operation per controller (not one controller per resource)

**Async Naming** - All async methods use `Async` suffix consistently:
```csharp
await _contentEditingService.CreateAsync(model, CurrentUserKey(_backOfficeSecurityAccessor));
```

**Factory Pattern Usage** (line 44):
```csharp
ContentCreateModel model = _documentEditingPresentationFactory.MapCreateModel(requestModel);
```
- ViewModels → Domain: `Map{Operation}Model(requestModel)`
- Domain → ViewModels: Factory classes in `Factories/` folder

### Key Patterns from Codebase

**ControllerBase Helper Methods** (inherited from `ManagementApiControllerBase` in Api.Common):
- `CreatedAtId<TController>(expression, id)` - Returns 201 with Location header
- `ContentEditingOperationStatusResult(status)` - Maps status enum to ProblemDetails
- `CurrentUserKey(accessor)` - Gets current user from security context

**Authorization Pattern** (all controllers):
```csharp
private readonly IAuthorizationService _authorizationService;
// Check permissions in action, not via [Authorize] attribute
```

---

## 4. Test Bench

### Test Location
- **Unit Tests**: `tests/Umbraco.Tests.UnitTests/Umbraco.Cms.Api.Management/`
- **Integration Tests**: `tests/Umbraco.Tests.Integration/Umbraco.Cms.Api.Management/`

### Running Tests
```bash
# All Management API tests
dotnet test --filter "FullyQualifiedName~Management"

# Specific domain (e.g., Document controllers)
dotnet test --filter "FullyQualifiedName~Management.Controllers.Document"
```

### Testing Focus
1. **Controller logic** - Request validation, authorization checks, status code mapping
2. **Factories** - ViewModel ↔ Domain model conversion accuracy
3. **Authorization** - Policy enforcement for each operation
4. **OpenAPI schema** - Ensure OpenAPI document generation doesn't break

### InternalsVisibleTo
Tests have access to internal types (see .csproj:44-52):
- `Umbraco.Tests.UnitTests`
- `Umbraco.Tests.Integration`
- `DynamicProxyGenAssembly2` (for Moq)

---

## 5. Error Handling

### Operation Status Pattern
Controllers use `Attempt<TResult, TStatus>` with strongly-typed status enums:
```csharp
Attempt<ContentCreateResult, ContentEditingOperationStatus> result =
    await _contentEditingService.CreateAsync(model, userKey);

return result.Success
    ? CreatedAtId<ByKeyDocumentController>(controller => nameof(controller.ByKey), result.Result.Content!.Key)
    : ContentEditingOperationStatusResult(result.Status); // Maps to ProblemDetails
```

**Status Enums** (from Core):
- `ContentEditingOperationStatus` - InvalidParent, NotFound, NotAllowed, etc.
- `UserOperationStatus` - UserNameIsNotEmail, DuplicateUserName, etc.
- Each enum value maps to specific HTTP status + ProblemDetails type

### ProblemDetails
All errors return RFC 7807 ProblemDetails via helper methods in base controllers:
- 400 Bad Request: Validation failures, invalid operations
- 403 Forbidden: Authorization failures
- 404 Not Found: Resource not found
- 409 Conflict: Duplicate operations

### Critical Logging Points
1. **Authorization failures** - Logged by AuthorizationService
2. **Service operation failures** - Logged in Infrastructure layer services
3. **External login errors** - BackOfficeSignInManager (Security/)

---

## 6. Clean Code

### Key Design Decisions

**Why Controller-per-Operation?** (not RESTful resource-based controllers)
- Fine-grained authorization per operation
- Operation-specific request/response models
- Clearer OpenAPI documentation
- Example: 20+ controllers in `Controllers/Document/` for different operations

**Why Manual Factories instead of AutoMapper?**
- Explicit control over mapping logic
- Easier debugging (no magic)
- Better performance (no reflection overhead)
- See `Factories/` directory (92 factory classes)

### Project-Specific Architectural Decisions

**Embedded OpenAPI Spec** (OpenApi.json - 1.3MB):
- Pre-generated, embedded as resource
- Served for client SDK generation
- **Why?** Deterministic output, faster startup (no runtime generation)

**SignalR for Real-time** (Routing/BackofficeHub.cs:33):
- `BackofficeHub` - User notifications, cache refreshes
- `ServerEventHub` - Background job updates, health checks
- Routes: `/umbraco/backoffice-signalr`, `/umbraco/serverevent-signalr`

### Code Smells to Watch For

1. **Large factory classes** - Some factories have 1000+ lines (e.g., `UserGroupPresentationFactory.cs`)
   - Consider splitting by operation

2. **Repeated authorization checks** - Each controller duplicates auth logic
   - Already abstracted to base classes, but still verbose

3. **ViewModel explosion** - 1000+ ViewModel classes across ViewModels/ folders
   - Consider shared base models or composition

---

## 7. Security

### Authentication & Authorization
**Method**: OpenIddict (OAuth 2.0) via Umbraco.Cms.Api.Common
- Reference tokens (not JWT) stored in database
- Token validation via OpenIddict middleware
- ASP.NET Core Data Protection for token encryption

**Authorization**:
- Basic authorization is done trough policies and the `AuthorizeAttribute`
```csharp
// Example from DocumentTreeControllerBase.cs, the user needs at least access to a section that uses trees
[Authorize(Policy = AuthorizationPolicies.SectionAccessForContentTree)]
```
- Authorization that needs (parts of) the payload are done manually trough the IAuthorizationService
```csharp
// Example from CreateDocumentController.cs:22
private readonly IAuthorizationService _authorizationService;

// Authorization checked in base controller methods, not attributes
protected async Task<IActionResult> HandleRequest(request, Func<Task<IActionResult>> handler)
{
    var authResult = await _authorizationService.AuthorizeAsync(User, request, policy);
    // ...
}
```

**Policies** (defined in Security/Authorization/):
- `ContentPermissionHandler` - Document/Media CRUD permissions
- `SectionAccessHandler` - Backoffice section access
- `UserGroupPermissionHandler` - Admin operations

**Password Requirements** (Security/ConfigureBackOfficeIdentityOptions.cs:18):
- See Identity options configuration

### External Login Providers
**Location**: `Security/BackOfficeExternalLoginProviders.cs`
- Google, Microsoft, OpenID Connect providers
- Auto-linking with `ExternalSignInAutoLinkOptions`
- **Critical**: Validate external claims before auto-linking users

### Input Validation
**FluentValidation** used throughout:
- Request models validated automatically via MVC integration
- Custom validators in each domain folder (e.g., `ViewModels/Document/Validators/`)

**Parameter Validation**:
```csharp
// Controllers validate IDs, keys before service calls
if (requestModel.Parent == null)
    return BadRequest(new ProblemDetailsBuilder()...);
```

### API Security
**CORS** - Configured in Umbraco.Web.UI (not this project)

**HTTPS Enforcement** - Configured in Umbraco.Web.UI

**Request Size Limits** - Configured for file uploads in Umbraco.Web.UI

**Security Headers** - Handled by Umbraco.Web.UI middleware

### Secrets Management
**No secrets in this project** - Configuration injected from parent application (Umbraco.Web.UI)

### Dependency Security
```bash
# Check vulnerable packages
dotnet list src/Umbraco.Cms.Api.Management package --vulnerable
```

### Security Anti-Patterns to Avoid
1. **Never bypass authorization checks** - All operations must authorize
2. **Never trust client validation** - Always validate on server
3. **Never expose stack traces** - ProblemDetails abstracts errors
4. **Never log sensitive data** - User passwords, tokens, API keys

---

## 8. Teamwork and Workflow

**⚠️ SKIPPED** - This is a sub-project. See root `/CLAUDE.md` for repository-wide teamwork protocols.

---

## 9. Edge Cases

### Domain-Specific Edge Cases

**Document Operations**:
1. **Publishing with descendants** - Can timeout on large trees
   - Use `PublishDocumentWithDescendantsController` with result polling
   - See `Controllers/Document/PublishDocumentWithDescendantsResultController.cs`

2. **Recycle bin operations** - Items in recycle bin can't be published
   - Check `IsTrashed` before publish operations

3. **Public access rules** - Affects authorization and routing
   - See `Controllers/Document/CreatePublicAccessDocumentController.cs`

**Media Upload**:
1. **Large file uploads** - Request size limits in parent app
   - Controllers accept multipart/form-data
   - Temporary files cleaned by background job

2. **Media picker** - Can reference deleted media
   - Validation in `Factories/` checks for orphaned references

**User Management**:
1. **External logins** - Auto-linking can create duplicate users if email mismatches
   - See `Security/ExternalSignInAutoLinkOptions.cs`

2. **User groups** - Deleting user group doesn't delete users
   - Users reassigned to default group

**Webhooks**:
1. **Webhook failures** - Failed webhooks retry with exponential backoff
   - See `Controllers/Webhook/` for configuration

### Known Gotchas (from TODO comments)

**StyleSheet/Script/PartialView Tree Controllers** - All have identical TODO comment:
```
TODO: [NL] This must return path segments for a query to work
// src/Umbraco.Cms.Api.Management/Controllers/Stylesheet/Tree/StylesheetTreeControllerBase.cs
// src/Umbraco.Cms.Api.Management/Controllers/Script/Tree/ScriptTreeControllerBase.cs
// src/Umbraco.Cms.Api.Management/Controllers/PartialView/Tree/PartialViewTreeControllerBase.cs
```

**BackOfficeController** - External login TODO:
```
// src/Umbraco.Cms.Api.Management/Controllers/Security/BackOfficeController.cs
// TODO: Handle external logins properly
```

---

## 10. Agentic Workflow

### When to Add a New Endpoint

**Decision Points**:
1. Does the operation fit an existing controller domain? (Document, Media, User, etc.)
2. Is this a new CRUD operation or a custom action?
3. Does this require new authorization policies?

**Workflow**:
1. **Create Controller** in appropriate `Controllers/{Domain}/` folder
   - Follow naming: `{Verb}{Entity}Controller`
   - Inherit from domain-specific base controller or `ManagementApiControllerBase`

2. **Define ViewModels** in `ViewModels/{Domain}/`
   - Request model (e.g., `CreateDocumentRequestModel`)
   - Response model (if not reusing existing)

3. **Create or Update Factory** in `Factories/`
   - Map request ViewModel → domain model
   - Map domain result → response ViewModel

4. **Authorization** - Check required permissions in controller action
   - Use `IAuthorizationService.AuthorizeAsync()`

5. **Service Layer** - Call Core services (from `Umbraco.Core.Services`)
   - Handle `Attempt<>` results
   - Map status to HTTP status codes

6. **OpenAPI Annotations**:
   - `[ApiVersion("1.0")]`
   - `[MapToApiVersion("1.0")]`
   - `[ProducesResponseType(...)]` for all status codes

7. **Testing**:
   - Unit test controller logic
   - Integration test end-to-end flow

8. **Update OpenApi.json** (if needed for client generation)

### Quality Gates Before PR
1. All tests pass
2. Code formatted (`dotnet format`)
3. No new warnings (check suppressed warnings list in .csproj:23)
4. OpenAPI schema valid (check at `/umbraco/openapi/`)
5. Authorization tested (unit + integration tests)

### Common Pitfalls
1. **Forgetting authorization checks** - Every operation must authorize
2. **Inconsistent status code mapping** - Use base controller helpers
3. **Large factory classes** - Split by operation if >500 lines
4. **Missing ProducesResponseType** - Breaks OpenAPI client generation
5. **Not handling Attempt failures** - Always check `result.Success`

---

## 11. Project-Specific Notes

### Key Design Decisions

**Why 1,317 files for one API?**
- Controller-per-operation pattern = many controllers
- 54 domains × average 10-20 operations per domain
- Tradeoff: Verbose but explicit, easier to navigate than megacontrollers

**Why manual factories instead of AutoMapper?**
- Performance: No reflection overhead
- Debuggability: Step through mapping logic
- Control: Complex mappings (e.g., security trimming) require custom logic

**Why embedded OpenApi.json?**
- Deterministic OpenAPI spec for client generation
- Faster startup (no runtime generation)
- Easier versioning (commit changes to spec)
- Added `OpenAPIContractTest` to test that all operations are exported

### External Integrations
None directly in this project. All integrations handled by:
- **Umbraco.Infrastructure** - External search, media, email providers
- **Umbraco.Core** - External data sources, webhooks

### Known Limitations

1. **API Versioning**: Currently v1.0 and v1.1
   - Future versions will require new controller classes or action methods

2. **Batch Operations**: Limited batch endpoint support
   - Most operations are single-entity (create one document at a time)

3. **Real-time Limits**: SignalR hubs don't scale beyond single-server without Redis backplane
   - Configure Redis for multi-server setups

4. **File Upload Size**: Controlled by parent app (Umbraco.Web.UI)
   - This project doesn't set limits

### Performance Considerations

**Caching**:
- Published content cached via `Umbraco.PublishedCache.HybridCache`
- Controllers query cache, not database (for published content)

**Background Jobs**:
- Long-running operations (publish with descendants, export) return job ID
- Poll result endpoint for completion
- See `Controllers/Document/PublishDocumentWithDescendantsResultController.cs`

**OpenAPI Generation**:
- Pre-generated (OpenApi.json embedded)
- Runtime generation disabled for performance

### Technical Debt (Top Issues from TODO comments)

1. **Warnings Suppressed** (Umbraco.Cms.Api.Management.csproj:9-22):
   ```
   TODO: Fix and remove overrides:
   - SA1117: params all on same line
   - SA1401: make fields private
   - SA1134: own line attributes
   - CS0108: hidden inherited member
   - CS0618/CS9042: update obsolete references
   - CS1998: remove async or make method synchronous
   - CS8524: switch statement exhaustiveness
   - IDE0060: removed unused parameter
   - SA1649: file name match type
   - CS0419: ambiguous reference
   - CS1573: param tag for all parameters
   - CS1574: unresolveable cref
   ```

2. **Tree Controller Path Segments** (multiple files):
   - Stylesheet/Script/PartialView tree controllers need path segment support for queries
   - Files: `Controllers/Stylesheet/Tree/StylesheetTreeControllerBase.cs`
   - Files: `Controllers/Script/Tree/ScriptTreeControllerBase.cs`
   - Files: `Controllers/PartialView/Tree/PartialViewTreeControllerBase.cs`

3. **External Login Handling** (Controllers/Security/BackOfficeController.cs):
   - TODO: Handle external logins properly

4. **Large Factory Classes** (Factories/UserGroupPresentationFactory.cs):
   - Some factories exceed 1000 lines - consider splitting

5. **ViewModel Explosion** - 1000+ ViewModel classes
   - Consider shared base models or composition to reduce duplication

---

## Quick Reference

### Essential Commands
```bash
# Build
dotnet build src/Umbraco.Cms.Api.Management

# Test Management API
dotnet test --filter "FullyQualifiedName~Management"

# Format code
dotnet format src/Umbraco.Cms.Api.Management

# Pack for NuGet
dotnet pack src/Umbraco.Cms.Api.Management -c Release
```

### Key Projects
- **Umbraco.Cms.Api.Management** (this) - Management API controllers and models
- **Umbraco.Cms.Api.Common** - Shared API infrastructure, base controllers
- **Umbraco.Infrastructure** - Service implementations, data access
- **Umbraco.Core** - Domain models, service interfaces

### Important Files
- **Project file**: `src/Umbraco.Cms.Api.Management/Umbraco.Cms.Api.Management.csproj`
- **Composer**: `src/Umbraco.Cms.Api.Management/ManagementApiComposer.cs` (DI entry point)
- **Routing**: `src/Umbraco.Cms.Api.Management/Routing/BackOfficeAreaRoutes.cs`
- **OpenAPI Spec**: `src/Umbraco.Cms.Api.Management/OpenApi.json` (embedded, 1.3MB)
- **Base Controllers**: See `Umbraco.Cms.Api.Common` project

### Configuration
No appsettings in this library - all configuration from parent app (Umbraco.Web.UI):
- OpenIddict settings
- CORS settings
- File upload limits

### API Endpoints
Base path: `/umbraco/management/api/v1/`

Examples:
- `POST /umbraco/management/api/v1/document` - Create document
- `GET /umbraco/management/api/v1/document/{id}` - Get document by key
- `PUT /umbraco/management/api/v1/document/{id}` - Update document
- `DELETE /umbraco/management/api/v1/document/{id}` - Delete document

Full spec: See OpenApi.json or Swagger UI at `/umbraco/openapi/`

### Getting Help
- **Root Documentation**: `/CLAUDE.md` (repository overview)
- **API Common Docs**: `../Umbraco.Cms.Api.Common/CLAUDE.md` (shared API patterns)
- **Core Docs**: `../Umbraco.Core/CLAUDE.md` (domain architecture)
- **Official Docs**: https://docs.umbraco.com/umbraco-cms/reference/management-api
