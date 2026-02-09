# Umbraco CMS - Infrastructure

Implementation layer for Umbraco CMS, providing concrete implementations of all Core interfaces. Handles database access (NPoco), caching, background jobs, migrations, search indexing (Examine), email (MailKit), and logging (Serilog).

**Project**: `Umbraco.Infrastructure`
**Type**: .NET Library
**Files**: 1,006 C# files implementing Core contracts

---

## 1. Architecture

### Target Framework
- **.NET 10.0** (`net10.0`)
- **C# 12** with nullable reference types enabled
- **Library** (no executable)

### Application Type
**Infrastructure Layer** - Implements all interfaces defined in `Umbraco.Core`. This is where contracts meet concrete implementations.

### Key Technologies
- **Database Access**: NPoco (micro-ORM) with SQL Server & SQLite support
- **Caching**: In-memory + distributed cache via `IAppCache`
- **Background Jobs**: Recurring jobs via `IRecurringBackgroundJob` and `IDistributedBackgroundJob`
- **Search**: Examine (Lucene.NET wrapper) for full-text search
- **Email**: MailKit for SMTP email
- **Logging**: Serilog with structured logging
- **Migrations**: Custom migration framework for database schema + data
- **DI**: Microsoft.Extensions.DependencyInjection
- **Serialization**: System.Text.Json
- **Identity**: Microsoft.Extensions.Identity.Stores for user/member management
- **Authentication**: OpenIddict.Abstractions

### Project Structure
```
src/Umbraco.Infrastructure/
├── Persistence/                   # Database access (NPoco)
│   ├── Repositories/              # Repository implementations (47 repos)
│   │   └── Implement/             # Concrete repository classes
│   ├── Dtos/                      # Database DTOs (80+ files)
│   ├── Mappers/                   # Entity ↔ DTO mappers (43 mappers)
│   ├── Factories/                 # Entity factories (28 factories)
│   ├── Querying/                  # Query builders and translators
│   ├── SqlSyntax/                 # SQL dialect handlers (SQL Server, SQLite)
│   ├── DatabaseModelDefinitions/  # Table/column definitions
│   └── UmbracoDatabase.cs         # Main database wrapper (NPoco)
│
├── Services/                      # Service implementations
│   └── Implement/                 # Concrete service classes (16 services)
│       ├── ContentService.cs      # Content CRUD operations
│       ├── MediaService.cs        # Media operations
│       ├── UserService.cs         # User management
│       └── [13 more services...]
│
├── Scoping/                       # Unit of Work implementation
│   ├── ScopeProvider.cs           # Transaction/scope management
│   ├── Scope.cs                   # Unit of work implementation
│   └── AmbientScopeContextStack.cs # Async-safe scope context
│
├── Migrations/                    # Database migration system
│   ├── Install/                   # Initial database schema
│   ├── Upgrade/                   # Version upgrade migrations (21 versions)
│   ├── PostMigrations/            # Post-upgrade data fixes
│   ├── MigrationPlan.cs           # Migration orchestration
│   └── MigrationPlanExecutor.cs   # Migration execution
│
├── BackgroundJobs/                # Background job infrastructure
│   ├── Jobs/                      # Concrete job implementations
│   │   ├── ReportSiteJob.cs       # Telemetry reporting
│   │   ├── TempFileCleanupJob.cs  # Cleanup temp files
│   │   └── ServerRegistration/    # Multi-server coordination
│   └── RecurringBackgroundJobHostedService.cs # Job scheduler
│
├── Examine/                       # Search indexing (Lucene)
│   ├── ContentValueSetBuilder.cs  # Index document content
│   ├── MediaValueSetBuilder.cs    # Index media
│   ├── MemberValueSetBuilder.cs   # Index members
│   ├── DeliveryApiContentIndexPopulator.cs # Delivery API indexing
│   └── Deferred/                  # Deferred index updates
│
├── Security/                      # Identity & authentication
│   ├── BackOfficeUserStore.cs     # User store (Identity)
│   ├── MemberUserStore.cs         # Member store (Identity)
│   ├── BackOfficeIdentity*.cs     # Identity configuration
│   └── Passwords/                 # Password hashing
│
├── PropertyEditors/               # Property editor implementations (75 files)
│   ├── ValueConverters/           # Convert stored → typed values
│   ├── Validators/                # Property validation
│   ├── Configuration/             # Editor configuration
│   └── DeliveryApi/               # Delivery API converters
│
├── Cache/                         # Cache implementation & invalidation
│   ├── DatabaseServerMessengerNotificationHandler.cs # Multi-server cache sync
│   └── PropertyEditors/           # Property editor caching
│
├── Serialization/                 # JSON serialization (20 files)
│   ├── SystemTextJsonSerializer.cs # Main serializer
│   └── Converters/                # Custom JSON converters
│
├── Mail/                          # Email (MailKit)
│   ├── EmailSender.cs             # SMTP email sender
│   └── EmailMessageExtensions.cs
│
├── Logging/                       # Serilog setup
│   ├── Serilog/                   # Serilog enrichers
│   └── MessageTemplates.cs        # Structured logging templates
│
├── Mapping/                       # Object mapping (IUmbracoMapper)
│   └── UmbracoMapper.cs           # AutoMapper-like functionality
│
├── Notifications/                 # Notification handlers
├── Packaging/                     # Package import/export
├── ModelsBuilder/                 # Strongly-typed models generation
├── Routing/                       # URL routing implementation
├── Runtime/                       # Application lifecycle
├── Search/                        # Search services
├── Sync/                          # Multi-server synchronization
├── Telemetry/                     # Analytics/telemetry
└── Templates/                     # Template parsing
```

### Dependencies
- **Umbraco.Core** - All interface contracts (only dependency)
- **NPoco** - Micro-ORM for database access
- **Examine.Core** - Search indexing abstraction
- **MailKit** - Email sending
- **HtmlAgilityPack** - HTML parsing
- **Serilog** - Structured logging
- **ncrontab** - Cron expression parsing for background jobs
- **OpenIddict.Abstractions** - OAuth/OIDC abstractions
- **Microsoft.Extensions.Identity.Stores** - Identity storage

### Design Patterns
1. **Repository Pattern** - All database access via repositories
   - Example: `UserRepository`, `ContentRepository`, `MediaRepository`
   - Implements interfaces from Umbraco.Core

2. **Unit of Work** - Scoping pattern for transactions
   - `IScopeProvider` / `Scope` - transaction management
   - Must call `scope.Complete()` to commit

3. **Factory Pattern** - Entity factories convert DTOs → domain models
   - Example: `ContentFactory`, `MediaFactory`, `UserFactory`
   - Located in `Persistence/Factories/`

4. **Mapper Pattern** - Bidirectional DTO ↔ Entity mapping
   - Example: `ContentMapper`, `MediaMapper`, `UserMapper`
   - Located in `Persistence/Mappers/`

5. **Strategy Pattern** - SQL syntax providers for different databases
   - `SqlServerSyntaxProvider`, `SqliteSyntaxProvider`
   - Abstracted via `ISqlSyntaxProvider`

6. **Migration Pattern** - Version-based database migrations
   - `MigrationPlan` defines migration graph
   - `MigrationPlanExecutor` runs migrations in order

7. **Builder Pattern** - `ValueSetBuilder` for search indexing

---

## 2. Commands

### Build & Test
```bash
# Build
dotnet build src/Umbraco.Infrastructure

# Test (tests in ../../tests/)
dotnet test --filter "FullyQualifiedName~Infrastructure"

# Pack
dotnet pack src/Umbraco.Infrastructure -c Release
```

### Code Quality
```bash
# Format code
dotnet format src/Umbraco.Infrastructure

# Build with all warnings (note: many suppressed, see .csproj:31)
dotnet build src/Umbraco.Infrastructure /p:TreatWarningsAsErrors=true
```

### Database Migrations (Developer Context)
This project contains the migration framework but **migrations are NOT run via EF Core**. Migrations run at application startup via `MigrationPlanExecutor`.

To create a new migration:
1. Create class inheriting `MigrationBase` in `Migrations/Upgrade/`
2. Add to `UmbracoPlan` migration plan
3. Restart application - migration runs automatically

### Package Management
```bash
# Check for vulnerable packages
dotnet list src/Umbraco.Infrastructure package --vulnerable

# Check outdated packages (versions in Directory.Packages.props)
dotnet list src/Umbraco.Infrastructure package --outdated
```

### Environment Setup
1. **Prerequisites**: .NET 10 SDK, SQL Server or SQLite
2. **IDE**: Visual Studio 2022, Rider, or VS Code
3. **Database**: Automatically created on first run (see Install/DatabaseSchemaCreator.cs)

---

## 3. Style Guide

### Project-Specific Patterns

**Repository Naming** (from UserRepository.cs:30):
```csharp
internal sealed class UserRepository : EntityRepositoryBase<Guid, IUser>, IUserRepository
```
- Pattern: `{Entity}Repository : EntityRepositoryBase<TId, TEntity>, I{Entity}Repository`
- Always `internal sealed` (not exposed outside assembly)
- Inherit from `EntityRepositoryBase` for common CRUD

**Factory Naming** (consistent across Factories/ directory):
```csharp
internal class ContentFactory : IEntityFactory<IContent, ContentDto>
```
- Pattern: `{Entity}Factory : IEntityFactory<TEntity, TDto>`
- Converts DTO → Domain entity
- Always `internal` (implementation detail)

**Service Naming** (from Services/Implement/):
```csharp
internal sealed class ContentService : RepositoryService, IContentService
```
- Pattern: `{Domain}Service : RepositoryService, I{Domain}Service`
- Inherits `RepositoryService` for scope/repository access
- Always `internal sealed`

### Key Code Patterns

**Scope Usage** (required for all database operations):
```csharp
using (ICoreScope scope = ScopeProvider.CreateCoreScope())
{
    // Database operations here
    scope.Complete(); // MUST call to commit
}
```

**NPoco Query Building** (from repositories):
```csharp
Sql<ISqlContext> sql = Sql()
    .Select<ContentDto>()
    .From<ContentDto>()
    .Where<ContentDto>(x => x.NodeId == id);
```

---

## 4. Test Bench

### Test Location
- **Unit Tests**: `tests/Umbraco.Tests.UnitTests/Umbraco.Infrastructure/`
- **Integration Tests**: `tests/Umbraco.Tests.Integration/Umbraco.Infrastructure/`
- **Benchmarks**: `tests/Umbraco.Tests.Benchmarks/`

### Running Tests
```bash
# All Infrastructure tests
dotnet test --filter "FullyQualifiedName~Infrastructure"

# Specific area (e.g., Persistence)
dotnet test --filter "FullyQualifiedName~Infrastructure.Persistence"

# Integration tests only
dotnet test --filter "Category=Integration&FullyQualifiedName~Infrastructure"
```

### Testing Focus
1. **Repository tests** - CRUD operations, query translation
2. **Scope tests** - Transaction behavior, nested scopes
3. **Migration tests** - Schema creation, upgrade paths
4. **Service tests** - Business logic, notification firing
5. **Mapper tests** - DTO ↔ Entity conversion accuracy

### InternalsVisibleTo
Tests have access to internal types (see .csproj:73-93):
- `Umbraco.Tests`, `Umbraco.Tests.UnitTests`, `Umbraco.Tests.Integration`, `Umbraco.Tests.Common`, `Umbraco.Tests.Benchmarks`
- `DynamicProxyGenAssembly2` (for Moq)

---

## 5. Error Handling

### Attempt Pattern (from Services)
Services return `Attempt<TResult, TStatus>` from Core:
```csharp
Attempt<IContent?, ContentEditingOperationStatus> result =
    await _contentService.CreateAsync(model, userKey);

if (!result.Success)
{
    // result.Status contains typed error (e.g., ContentEditingOperationStatus.NotFound)
}
```

### Database Error Handling
**NPoco Exception Wrapping**:
- `NPocoSqlException` wraps database errors
- Check `InnerException` for underlying DB error
- SQL syntax errors logged via `ILogger<T>`

### Scope Error Handling
**Critical**: If scope not completed, transaction rolls back:
```csharp
using (ICoreScope scope = ScopeProvider.CreateCoreScope())
{
    try
    {
        // Operations
        scope.Complete(); // MUST be called
    }
    catch
    {
        // Transaction automatically rolled back if not completed
        throw;
    }
}
```

### Critical Logging Points
1. **Migration failures** - Logged in `MigrationPlanExecutor.cs`
2. **Database connection errors** - Logged in `UmbracoDatabaseFactory.cs`
3. **Repository exceptions** - Logged in `EntityRepositoryBase`
4. **Background job failures** - Logged in `RecurringBackgroundJobHostedService.cs`

---

## 6. Clean Code

### Key Design Decisions

**Why NPoco instead of EF Core?**
- Performance: NPoco is faster for Umbraco's read-heavy workload
- Control: Fine-grained control over SQL generation
- Flexibility: Easier to optimize complex queries
- History: Legacy decision, but still valid (EF Core support added in parallel)

**Why Custom Migration Framework?**
- Pre-dates EF Core Migrations
- Supports data migrations + schema migrations
- Graph-based dependencies (not linear like EF Core)
- Can target multiple database providers with same migration
- Located in `Migrations/` directory

**Why EntityRepositoryBase?**
- DRY: Common CRUD operations (Get, GetMany, Save, Delete)
- Consistency: All repos follow same patterns
- Caching: Integrated cache invalidation
- Scoping: Automatic transaction management

**Why Separate DTOs?**
- Database schema != domain model
- DTOs are flat, entities have relationships
- Allows independent evolution of schema vs domain
- Located in `Persistence/Dtos/`

### Architectural Decisions

**Repository Layer** (Persistence/Repositories/Implement/):
- 47 repository implementations
- All inherit from `EntityRepositoryBase<TId, TEntity>`
- Repositories do NOT fire notifications (services do)

**Service Layer** (Services/Implement/):
- 16 service implementations
- Services fire notifications before/after operations
- Services manage scopes (not repositories)
- Example: `ContentService`, `MediaService`, `UserService`

### Code Smells to Watch For

1. **Forgetting `scope.Complete()`** - Transaction silently rolls back
2. **Nested scopes without awareness** - Only innermost scope controls commit
3. **Lazy loading outside scope** - NPoco relationships must load within scope
4. **Large migrations** - Split into multiple steps if > 1000 lines
5. **Repository logic in services** - Keep repos thin, logic in services

---

## 7. Security

### Input Validation
**At Service Layer**:
- Services validate before calling repositories
- FluentValidation NOT used (manual validation)
- Example: `ContentService` validates content type exists before creating content

### Data Access Security
**SQL Injection Prevention**:
- NPoco uses parameterized queries automatically
- Never concatenate SQL strings
- All queries via `Sql<ISqlContext>` builder:
  ```csharp
  Sql().Select<ContentDto>().Where<ContentDto>(x => x.NodeId == id) // Safe
  Database.Query<ContentDto>($"SELECT * FROM Content WHERE id = {id}") // NEVER do this
  ```

### Authentication & Authorization
**Identity Stores** (Security/):
- `BackOfficeUserStore.cs` - User store for ASP.NET Core Identity
- `MemberUserStore.cs` - Member store for ASP.NET Core Identity
- Password hashing via `IPasswordHasher<T>` (PBKDF2)

**Password Security** (Security/Passwords/):
- PBKDF2 with 10,000 iterations (configurable)
- Salted hashes stored in database
- Legacy hash formats supported for migration

### Secrets Management
**No secrets in this library** - Configuration from parent application (Umbraco.Web.UI):
- Connection strings from `appsettings.json`
- SMTP credentials from `appsettings.json`
- Email sender uses `IOptions<EmailSenderSettings>`

### Dependency Security
```bash
# Check vulnerable dependencies
dotnet list src/Umbraco.Infrastructure package --vulnerable
```

### Security Anti-Patterns to Avoid
1. **Raw SQL queries** - Always use NPoco `Sql<ISqlContext>` builder
2. **Storing plain text passwords** - Use Identity's password hasher
3. **Exposing internal types** - Keep repos/services `internal`
4. **Logging sensitive data** - Never log passwords, connection strings, API keys

---

## 8. Teamwork and Workflow

**⚠️ SKIPPED** - This is a sub-project. See root `/CLAUDE.md` for repository-wide teamwork protocols.

---

## 9. Edge Cases

### Scope Edge Cases

**Nested Scopes** - Only innermost scope commits:
```csharp
using (var outer = ScopeProvider.CreateCoreScope())
{
    using (var inner = ScopeProvider.CreateCoreScope())
    {
        // Work
        inner.Complete(); // This does nothing!
    }
    outer.Complete(); // This commits
}
```

**Async Scopes** - Scopes are NOT thread-safe:
- Don't pass scopes across threads
- Don't use scopes in Parallel.ForEach
- Create new scope on each async operation

**SQLite Lock Contention**:
- SQLite has database-level locking
- Multiple concurrent writes = lock errors
- Use `[SuppressMessage]` for known SQLite lock issues
- See `UserRepository.cs:39` - `_sqliteValidateSessionLock`

### Migration Edge Cases

**Migration Rollback** - NOT SUPPORTED:
- Migrations are one-way only
- Test migrations thoroughly before release
- Use database backups for rollback

**Migration Dependencies** - Graph-based:
- Migrations can have multiple dependencies
- Dependencies resolved via `MigrationPlan`
- Circular dependencies throw `InvalidOperationException`

**Data Migrations** - Can be slow:
- Migrations run at startup (blocking)
- Large data migrations (> 100k rows) should be chunked
- Use `AsyncMigrationBase` for long-running operations

### Repository Edge Cases

**Cache Invalidation**:
- Repository CRUD operations invalidate cache automatically
- Bulk operations may not invalidate correctly
- Repositories fire cache refreshers via `DistributedCache`

**NPoco Lazy Loading**:
- Relationships must be loaded within scope
- Accessing lazy-loaded properties outside scope throws
- Use `.FetchOneToMany()` to eager load

### Background Job Edge Cases

**Multi-Server Coordination**:
- Background jobs use server registration to coordinate
- Only "main" server runs jobs (determined by DB lock)
- If main server dies, another takes over within 5 minutes

---

## 10. Agentic Workflow

### When to Add a New Repository

**Decision Points**:
1. Does the entity have a Core interface in `Umbraco.Core/Persistence/Repositories`?
2. Is this entity persisted to the database?
3. Does it require custom queries beyond basic CRUD?

**Workflow**:
1. **Create DTO** in `Persistence/Dtos/` (matches database table)
2. **Create Mapper** in `Persistence/Mappers/` (DTO ↔ Entity)
3. **Create Factory** in `Persistence/Factories/` (DTO → Entity)
4. **Create Repository** in `Persistence/Repositories/Implement/`
   - Inherit from `EntityRepositoryBase<TId, TEntity>`
   - Implement interface from Core
5. **Register in Composer** (DependencyInjection/)
6. **Write Tests** (unit + integration)

### When to Add a New Service

**Decision Points**:
1. Does the service have a Core interface in `Umbraco.Core/Services`?
2. Does it coordinate multiple repositories?
3. Does it need to fire notifications?

**Workflow**:
1. **Implement Interface** from Core in `Services/Implement/`
   - Inherit from `RepositoryService`
   - Inject repositories via constructor
2. **Add Notification Firing**:
   - Fire `*SavingNotification` before operation (cancellable)
   - Fire `*SavedNotification` after operation
3. **Manage Scopes** - Services create scopes, not repositories
4. **Register in Composer**
5. **Write Tests**

### When to Add a Migration

**Decision Points**:
1. Is this a schema change (tables, columns, indexes)?
2. Is this a data migration (update existing data)?
3. Which version does this target?

**Workflow**:
1. **Create Migration Class** in `Migrations/Upgrade/V{Version}/`
   - Inherit from `MigrationBase` (schema) or `AsyncMigrationBase` (data)
   - Implement `Migrate()` method
2. **Add to UmbracoPlan** in `Migrations/Upgrade/UmbracoPlan.cs`
   - Specify dependencies (runs after which migrations?)
3. **Test Migration**:
   - Integration test with database
   - Test upgrade from previous version
4. **Document Breaking Changes** (if any)

### Quality Gates Before PR
1. All tests pass
2. Code formatted (`dotnet format`)
3. No new warnings (check suppressed warnings list in .csproj:31)
4. Database migrations tested (upgrade from previous version)
5. Scope usage correct (all scopes completed)

### Common Pitfalls
1. **Forgetting `scope.Complete()`** - Transaction rolls back silently
2. **Repository logic in services** - Keep repos focused on data access
3. **Missing cache invalidation** - Repositories auto-invalidate, but custom queries may not
4. **Missing notifications** - Services must fire notifications
5. **Eager loading outside scope** - NPoco relationships must load within scope
6. **Large migrations** - Chunk data migrations for performance

---

## 11. Project-Specific Notes

### Key Design Decisions

**Why 1,006 files for "just" implementation?**
- 47 repositories × ~3 files each (repo, mapper, factory) = ~141 files
- 75 property editors × ~2 files each = ~150 files
- 80 DTOs for database tables = 80 files
- 21 versions × ~5 migrations each = ~105 files
- Remaining: services, background jobs, search, email, logging, etc.

**Why NPoco + Custom Migrations?**
- Historical: Predates EF Core
- Performance: Faster than EF Core for Umbraco's workload
- Control: Fine-grained SQL control
- **Note**: EF Core support added in parallel (`Umbraco.Cms.Persistence.EFCore` project)

**Why Separate Factories and Mappers?**
- **Factories**: DTO → Entity (one direction, for reading from DB)
- **Mappers**: DTO ↔ Entity (bidirectional, includes column mapping metadata)
- Factories use Mappers under the hood

### External Integrations

**Email (MailKit)**:
- SMTP email via `MailKit` library (version in `Directory.Packages.props`)
- Configured via `IOptions<EmailSenderSettings>`
- Supports TLS, SSL, authentication

**Search (Examine)**:
- Lucene.NET wrapper
- Indexes content, media, members
- `ValueSetBuilder` classes convert entities → search documents
- Located in `Examine/` directory

**Logging (Serilog)**:
- Structured logging throughout
- Enrichers: Process ID, Thread ID
- Sinks: File, Async
- Configuration in `appsettings.json` of parent app

**Background Jobs (Recurring)**:
- Cron-based scheduling via `ncrontab`
- `IRecurringBackgroundJob` interface
- Jobs: Telemetry, temp file cleanup, server registration
- Located in `BackgroundJobs/Jobs/`

### Known Limitations

1. **NPoco Lazy Loading** - Must load relationships within scope
2. **Migration Rollback** - One-way only, no rollback support
3. **SQLite Locking** - Database-level locks cause contention
4. **Single Database** - No multi-database support (e.g., read replicas)
5. **Background Jobs** - Single-server only (distributed jobs require additional setup)

### Performance Considerations

**Caching**:
- Repository results cached automatically
- Cache invalidation via `DistributedCache`
- Multi-server cache sync via database messenger

**Database Connection Pooling**:
- ADO.NET connection pooling enabled by default
- Configured in connection string

**N+1 Query Problem**:
- NPoco supports eager loading via `.FetchOneToMany()`
- Always profile queries in development

**Background Jobs**:
- Run on background threads (don't block web requests)
- Use `IRecurringBackgroundJob` for scheduled tasks
- Use `IDistributedBackgroundJob` for multi-server coordination

### Technical Debt (from TODO comments in .csproj and code)

1. **Warnings Suppressed** (Umbraco.Infrastructure.csproj:10-30):
   ```
   TODO: Fix and remove overrides:
   - CS0618: handle member obsolete appropriately
   - CA1416: validate platform compatibility
   - SA1117: params all on same line
   - SA1401: make fields private
   - SA1134: own line attributes
   - CA2017: match parameters number
   - CS0108: hidden inherited member
   - SYSLIB0051: formatter-based serialization
   - SA1649: filename match type name
   - CS1998: remove async or make method synchronous
   - CS0169: unused field
   - CS0114: hidden inherited member
   - IDE0060: remove unused parameter
   - SA1130: use lambda syntax
   - IDE1006: naming violation
   - CS1066: default value
   - CS0612: obsolete
   - CS1574: resolve cref
   ```

2. **CacheInstructionService.cs** - TODO comments (multiple locations)

3. **MemberUserStore.cs** - TODO: Handle external logins

4. **BackOfficeUserStore.cs** - TODO: Optimize user queries

5. **UserRepository.cs** - TODO: SQLite session validation lock (line 39)

6. **Repository Base Classes** - Some repos have large inheritance chains (tech debt)

### TRACE_SCOPES Feature
**Debug-only scope tracing** (Umbraco.Infrastructure.csproj:34-36):
```xml
<PropertyGroup Condition="'$(Configuration)' == 'Debug'">
  <DefineConstants>$(DefineConstants);TRACE_SCOPES</DefineConstants>
</PropertyGroup>
```
- Enables detailed scope logging in Debug builds
- Helps debug nested scope issues
- Performance impact - only use in development

---

## Quick Reference

### Essential Commands
```bash
# Build
dotnet build src/Umbraco.Infrastructure

# Test Infrastructure
dotnet test --filter "FullyQualifiedName~Infrastructure"

# Format code
dotnet format src/Umbraco.Infrastructure

# Pack for NuGet
dotnet pack src/Umbraco.Infrastructure -c Release
```

### Key Dependencies
- **Umbraco.Core** - Interface contracts (only project dependency)
- **NPoco** - Database access
- **Examine.Core** - Search indexing
- **MailKit** - Email sending
- **Serilog** - Logging

### Important Files
- **Project file**: `src/Umbraco.Infrastructure/Umbraco.Infrastructure.csproj`
- **Scope Provider**: `src/Umbraco.Infrastructure/Scoping/ScopeProvider.cs`
- **Database Factory**: `src/Umbraco.Infrastructure/Persistence/UmbracoDatabaseFactory.cs`
- **Migration Executor**: `src/Umbraco.Infrastructure/Migrations/MigrationPlanExecutor.cs`
- **Content Service**: `src/Umbraco.Infrastructure/Services/Implement/ContentService.cs`
- **User Repository**: `src/Umbraco.Infrastructure/Persistence/Repositories/Implement/UserRepository.cs`

### Critical Patterns
```csharp
// 1. Always use scopes for database operations
using (ICoreScope scope = ScopeProvider.CreateCoreScope())
{
    // Work
    scope.Complete(); // MUST call to commit
}

// 2. NPoco query building
Sql<ISqlContext> sql = Sql()
    .Select<ContentDto>()
    .From<ContentDto>()
    .Where<ContentDto>(x => x.NodeId == id);

// 3. Repository pattern
IContent? content = _contentRepository.Get(id);

// 4. Service pattern with notifications
var saving = new ContentSavingNotification(content, eventMessages);
if (_eventAggregator.PublishCancelable(saving))
    return Attempt.Fail(...); // Cancelled

_contentRepository.Save(content);

var saved = new ContentSavedNotification(content, eventMessages);
_eventAggregator.Publish(saved);
```

### Configuration
No appsettings in this library - all configuration from parent application (Umbraco.Web.UI):
- Connection strings
- Email settings
- Serilog configuration
- Background job schedules

### Getting Help
- **Core Docs**: `../Umbraco.Core/CLAUDE.md` (interface contracts)
- **Root Docs**: `/CLAUDE.md` (repository overview)
- **Official Docs**: https://docs.umbraco.com/umbraco-cms/reference/
