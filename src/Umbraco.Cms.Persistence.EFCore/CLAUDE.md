# Umbraco.Cms.Persistence.EFCore

Entity Framework Core persistence layer for Umbraco CMS. This project provides EF Core integration including scoping, distributed locking, and migration infrastructure.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Dependencies**: Umbraco.Core, Umbraco.Infrastructure

---

## 1. Architecture

### Project Purpose

This project bridges EF Core with Umbraco's existing persistence infrastructure. It provides:

1. **UmbracoDbContext** - Base DbContext with automatic table prefix (`umbraco`) and provider-based configuration
2. **EF Core Scoping** - Integration with Umbraco's Unit of Work pattern (scopes)
3. **Distributed Locking** - SQL Server and SQLite locking mechanisms for EF Core contexts
4. **Migration Infrastructure** - Provider-agnostic migration execution

### Folder Structure

```
Umbraco.Cms.Persistence.EFCore/
├── Composition/
│   └── UmbracoEFCoreComposer.cs      # DI registration, OpenIddict setup
├── Extensions/
│   ├── DbContextExtensions.cs         # ExecuteScalarAsync, MigrateDatabaseAsync
│   └── UmbracoEFCoreServiceCollectionExtensions.cs  # AddUmbracoDbContext<T>
├── Locking/
│   ├── SqlServerEFCoreDistributedLockingMechanism.cs
│   └── SqliteEFCoreDistributedLockingMechanism.cs
├── Migrations/
│   ├── IMigrationProvider.cs          # Provider-specific migration execution
│   └── IMigrationProviderSetup.cs     # DbContext options setup per provider
├── Scoping/
│   ├── AmbientEFCoreScopeStack.cs     # AsyncLocal scope stack
│   ├── EFCoreScope.cs                 # Main scope implementation
│   ├── EFCoreDetachableScope.cs       # Detachable scope for Deploy
│   ├── EFCoreScopeProvider.cs         # Scope factory
│   ├── EFCoreScopeAccessor.cs         # Ambient scope accessor
│   └── I*.cs                          # Interfaces
├── Constants-ProviderNames.cs         # SQLite/SQLServer provider constants
├── EfCoreMigrationExecutor.cs         # Migration orchestrator
├── StringExtensions.cs                # Provider name comparison
└── UmbracoDbContext.cs                # Base DbContext
```

### Key Design Decisions

1. **Generic DbContext Support** - All scoping/locking is generic (`<TDbContext>`) allowing custom contexts
2. **Parallel NPoco Coexistence** - Designed to work alongside existing NPoco persistence layer
3. **Provider Abstraction** - `IMigrationProvider` and `IMigrationProviderSetup` enable database-agnostic migrations
4. **OpenIddict Integration** - UmbracoDbContext automatically registers OpenIddict entity sets

### Database Providers

Provider-specific implementations live in separate projects:
- `Umbraco.Cms.Persistence.EFCore.SqlServer` - SQL Server provider and migrations
- `Umbraco.Cms.Persistence.EFCore.Sqlite` - SQLite provider and migrations

Provider names (from `Constants.ProviderNames`):
- `Microsoft.Data.SqlClient` - SQL Server
- `Microsoft.Data.Sqlite` - SQLite

---

## 2. Commands

**For Git workflow and standard build commands**, see [repository root](../../CLAUDE.md).

### EF Core Migrations

**Important**: Run from repository root with valid connection string in `src/Umbraco.Web.UI/appsettings.json`.

```bash
# Add migration (SQL Server)
dotnet ef migrations add %MigrationName% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer -c UmbracoDbContext

# Add migration (SQLite)
dotnet ef migrations add %MigrationName% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext

# Remove last migration (SQL Server)
dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer

# Remove last migration (SQLite)
dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite

# Generate migration script
dotnet ef migrations script -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.SqlServer
```

---

## 3. Key Patterns

### Adding a Custom DbContext

Use `AddUmbracoDbContext<T>` to register with full scope integration:

```csharp
services.AddUmbracoDbContext<MyCustomDbContext>((provider, options, connectionString, providerName) =>
{
    options.UseOpenIddict(); // Optional
});
```

Registers: `IDbContextFactory<T>`, scope providers, accessors, and distributed locking

### Using EF Core Scopes

```csharp
using IEfCoreScope<UmbracoDbContext> scope = _scopeProvider.CreateScope();

await scope.ExecuteWithContextAsync<MyEntity>(async dbContext =>
{
    return await dbContext.MyEntities.FirstOrDefaultAsync(x => x.Id == id);
});

scope.Complete(); // Required to commit
```

### UmbracoDbContext Table Naming

All tables auto-prefixed with `umbraco` (see `UmbracoDbContext.cs:85-88`).

---

## 4. Scoping System

### Scope Hierarchy

```
EFCoreScopeProvider<T>
    ├── Creates EFCoreScope<T> (normal scope)
    └── Creates EFCoreDetachableScope<T> (for Deploy scenarios)

EFCoreScope<T>
    ├── Manages DbContext lifecycle
    ├── Handles transactions (BeginTransaction/Commit/Rollback)
    ├── Integrates with parent NPoco IScope when nested
    └── Manages distributed locks via Locks property
```

### Ambient Scope Stack

Uses `AsyncLocal<ConcurrentStack<T>>` for thread-safe tracking. Scopes must be disposed in LIFO order (child before parent).

### Transaction Integration

When an EF Core scope is created inside an NPoco scope:
1. The EF Core scope reuses the parent's `DbTransaction`
2. Transaction commit/rollback is delegated to the parent scope
3. DbContext connects to the same connection as the parent

See `EFCoreScope.cs:158-180` for transaction initialization logic.

---

## 5. Distributed Locking

### SQL Server Locking

Table-level locks with `REPEATABLEREAD` hint. Timeout via `SET LOCK_TIMEOUT`. Requires `ReadCommitted` or higher.

### SQLite Locking

Database-level locking (SQLite limitation). Read locks use snapshot isolation (WAL mode). Write locks are exclusive. Handles `SQLITE_BUSY/LOCKED` errors.

### Lock Requirements

Requires active scope, transaction, and minimum `ReadCommitted` isolation.

---

## 6. Migration System

### How Migrations Execute

1. `UmbracoEFCoreComposer` registers `EfCoreMigrationExecutor`
2. Notification handler triggers on database creation
3. `EfCoreMigrationExecutor` finds correct `IMigrationProvider` by provider name
4. Provider executes via `dbContext.Database.Migrate()`

### Adding New Migrations

Create equivalent migrations in both SqlServer and Sqlite provider projects using commands from section 2.

---

## 7. Project-Specific Notes

### Known Technical Debt

**Warning Suppressions** (`.csproj:9-14`): `IDE0270` (null checks), `CS0108` (hiding members), `CS1998` (async/await).

**Detachable Scope** (`EFCoreDetachableScope.cs:93-94`): TODO for Deploy integration, limited test coverage.

### Circular Dependency Handling

`SqlServerEFCoreDistributedLockingMechanism` uses `Lazy<IEFCoreScopeAccessor<T>>` to break circular dependency.

### StaticServiceProvider Usage

Fallback for design-time EF tooling (migrations) and startup edge cases when DI unavailable.

### OpenIddict Integration

`UmbracoEFCoreComposer` configures OpenIddict via `options.UseOpenIddict()` (see `Composition/UmbracoEFCoreComposer.cs:22-36`).

### Vulnerable Dependency Override

Top-level dependency on `Microsoft.Extensions.Caching.Memory` overrides vulnerable EF Core transitive dependency (`.csproj:18-19`).

### InternalsVisibleTo

`Umbraco.Tests.Integration` has access to internal types.

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `UmbracoDbContext.cs` | Base DbContext with table prefixing |
| `EFCoreScope.cs` | Main scope implementation |
| `EFCoreScopeProvider.cs` | Scope factory |
| `UmbracoEFCoreServiceCollectionExtensions.cs` | `AddUmbracoDbContext<T>` extension |
| `UmbracoEFCoreComposer.cs` | DI registration |

### Key Interfaces

| Interface | Purpose |
|-----------|---------|
| `IEfCoreScope<TDbContext>` | Scope contract with `ExecuteWithContextAsync` |
| `IEFCoreScopeProvider<TDbContext>` | Creates/manages scopes |
| `IEFCoreScopeAccessor<TDbContext>` | Access ambient scope |
| `IMigrationProvider` | Provider-specific migration execution |
| `IMigrationProviderSetup` | DbContext options setup per provider |

### Provider Constants

```csharp
Constants.ProviderNames.SQLServer = "Microsoft.Data.SqlClient"
Constants.ProviderNames.SQLLite = "Microsoft.Data.Sqlite"
```

### Related Projects

- `Umbraco.Cms.Persistence.EFCore.SqlServer` - SQL Server implementation
- `Umbraco.Cms.Persistence.EFCore.Sqlite` - SQLite implementation
- `Umbraco.Infrastructure` - Contains NPoco scoping that this integrates with
