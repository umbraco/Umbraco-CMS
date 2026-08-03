# Umbraco.Cms.Persistence.EFCore.Sqlite

SQLite-specific EF Core provider for Umbraco CMS. Implements SQLite database configuration, distributed locking, migrations, and service registration for the EF Core persistence layer.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Dependencies**: Umbraco.Infrastructure

---

## 1. Architecture

### Project Purpose

This provider project implements SQLite-specific functionality for the EF Core persistence layer:

1. **Database Configurator** - Configures DbContext with SQLite-specific options (`IDatabaseConfigurator`)
2. **Service Registrar** - Registers SQLite-specific services like distributed locking (`IDbContextServiceRegistrar`)
3. **Distributed Locking** - SQLite-specific distributed locking mechanism
4. **Migration Provider** - Executes SQLite-specific EF Core migrations
5. **Migration Provider Setup** - Configures DbContext to use SQLite for migrations
6. **Migrations** - SQLite-specific migration files (OpenIddict tables, webhooks, etc.)
7. **Retrying Execution Strategy** - Retries transient SQLite lock errors on EF Core operations

### Folder Structure

```
Umbraco.Cms.Persistence.EFCore.Sqlite/
├── Migrations/
│   ├── 20230622183638_InitialCreate.cs          # No-op (NPoco creates tables)
│   ├── 20230807123456_AddOpenIddict.cs          # OpenIddict tables
│   ├── 20240403141051_UpdateOpenIddictToV5.cs   # OpenIddict v5 schema
│   ├── 20251006140958_UpdateOpenIddictToV7.cs   # OpenIddict v7 (no-op for SQLite)
│   ├── 20260209100831_AddWebhookDto.cs          # Webhook tables
│   └── UmbracoDbContextModelSnapshot.cs         # Current model state
├── EFCoreSqliteComposer.cs                       # DI registration
├── SqliteCollationModelCustomizer.cs             # Applies COLLATE NOCASE to all string properties
├── SqliteDatabaseConfigurator.cs                 # IDatabaseConfigurator impl
├── SqliteDbContextServiceRegistrar.cs            # IDbContextServiceRegistrar impl
├── SqliteEFCoreDistributedLockingMechanism.cs    # Distributed locking
├── SqliteMigrationProvider.cs                    # IMigrationProvider impl
├── SqliteMigrationProviderSetup.cs               # IMigrationProviderSetup impl
├── UmbracoBuilderExtensions.cs                   # IUmbracoBuilder extensions
└── SqliteRetryingExecutionStrategy.cs            # IExecutionStrategy for transient lock errors
```

### Relationship with Infrastructure

This project extends `Umbraco.Infrastructure`:

- Implements `IDatabaseConfigurator` interface defined in Infrastructure
- Implements `IDbContextServiceRegistrar` interface defined in Infrastructure
- Implements `IMigrationProvider` interface defined in Infrastructure
- Implements `IMigrationProviderSetup` interface defined in Infrastructure
- Uses `UmbracoDbContext` from Infrastructure
- Provider name: `Microsoft.Data.Sqlite` (from `Constants.ProviderNames.SQLLite`)

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

```bash
# Build this project
dotnet build src/Umbraco.Cms.Persistence.EFCore.Sqlite

# Run related tests
dotnet test --filter "FullyQualifiedName~EFCore"
```

---

## 3. Key Components

### EFCoreSqliteComposer

Registers SQLite implementations of `IMigrationProvider`, `IMigrationProviderSetup`, and provider registration services.

### SqliteDatabaseConfigurator

Implements `IDatabaseConfigurator` to configure `DbContextOptionsBuilder` with `UseSqlite`. Called by `DbContextRegistration` when registering DbContext types.

### SqliteDbContextServiceRegistrar

Implements `IDbContextServiceRegistrar` to register SQLite-specific services (e.g., distributed locking) for each registered DbContext type.

### SqliteEFCoreDistributedLockingMechanism

SQLite-specific distributed locking implementation. Moved from Infrastructure to this provider package as it is provider-specific.

### SqliteMigrationProvider

- `MigrateAsync(EFCoreMigration)` - Runs specific named migration
- `MigrateAllAsync()` - Runs all pending migrations
- **Critical**: Cannot run `MigrateAllAsync` when transaction is active

### SqliteMigrationProviderSetup

Configures `DbContextOptionsBuilder` with `UseSqlite`, the migrations assembly, and the
`SqliteRetryingExecutionStrategy` (see below). Invoked from
`UmbracoDbContext.ConfigureOptions` for every `UmbracoDbContext` instance, so all EF Core
access to the Umbraco database (including OpenIddict's token store) inherits the retry.

### SqliteRetryingExecutionStrategy

Custom `Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy` that retries on transient
SQLite errors (`SQLITE_BUSY`, `SQLITE_LOCKED`) using `SqliteExceptionExtensions.IsBusyOrLocked`
from the parent project. Defaults inherit `ExecutionStrategy.DefaultMaxRetryCount` (6) and
`ExecutionStrategy.DefaultMaxDelay` (30s), giving a ~56-second retry budget — see the class's
XML doc for the rationale and the unattended-upgrade escape hatch for very long migrations.
Added to resolve issue #22939 (OpenIddict token reads failing during long migrations).

### UmbracoBuilderExtensions

Extension methods on `IUmbracoBuilder` for adding SQLite EF Core support to the application.

---

## 4. Migrations

### Migration History

| Migration | Date | Purpose |
|-----------|------|---------|
| `InitialCreate` | 2023-06-22 | No-op - NPoco creates base tables |
| `AddOpenIddict` | 2023-08-07 | Creates OpenIddict tables (Applications, Tokens, Authorizations, Scopes) |
| `UpdateOpenIddictToV5` | 2024-04-03 | Schema updates for OpenIddict v5 |
| `UpdateOpenIddictToV7` | 2025-10-06 | No-op for SQLite (no schema changes needed) |
| `AddWebhookDto` | 2026-02-09 | Webhook tables |

### OpenIddict Tables Created

All tables prefixed with `umbraco`:
- `umbracoOpenIddictApplications` - OAuth client applications
- `umbracoOpenIddictAuthorizations` - User authorizations
- `umbracoOpenIddictScopes` - OAuth scopes
- `umbracoOpenIddictTokens` - Access/refresh tokens

### SQLite-Specific Notes

- **TEXT column type** - SQLite uses TEXT for all strings (no varchar)
- **InitialCreate is no-op** - NPoco creates base tables, EF Core manages OpenIddict only
- **v7 migration is no-op** - SQLite didn't require schema changes that SQL Server needed

### Adding New Migrations

1. Configure SQLite connection string in `src/Umbraco.Web.UI/appsettings.json`
2. Run from repository root:
   ```bash
   dotnet ef migrations add %Name% -s src/Umbraco.Web.UI -p src/Umbraco.Cms.Persistence.EFCore.Sqlite -c UmbracoDbContext
   ```
3. Empty the `Up()` and `Down()` methods (these are no-ops — NPoco creates the tables)
4. **Critical**: Also add equivalent migration to `Umbraco.Cms.Persistence.EFCore.SqlServer`
5. Update `SqliteMigrationProvider.GetMigrationType()` switch if adding named migrations

### Model Customizers

SQLite does **NOT** need per-DTO customizers for index features like `.IncludeProperties()` — those are SQL Server-specific.

However, SQLite **does** require a global collation customizer. NPoco's `SqliteSyntaxProvider` creates ALL string columns as `TEXT COLLATE NOCASE` (case-insensitive), matching SQL Server's typical `CI_AS` database-level collation. EF Core's SQLite provider creates plain `TEXT` columns, which default to `BINARY` (case-sensitive). Without a customizer, string comparisons (lookups by alias, email, login, ISO code, dictionary keys, etc.) would silently break when EF Core takes over table creation.

**This is already handled automatically by `SqliteCollationModelCustomizer`**, registered in `UmbracoBuilderExtensions`. It iterates all entity types in the model and applies `.UseCollation("NOCASE")` to every string property. No per-DTO action is required when adding new DTOs.

**Why not per-property in shared configurations?** Adding `.UseCollation("NOCASE")` to every string property in every `IEntityTypeConfiguration` would be verbose (80+ DTOs), error-prone, and would affect SQL Server unnecessarily. A single SQLite-specific customizer is cleaner.

**Full migration guide**: See `/src/Umbraco.Infrastructure/CLAUDE.md` → "Section 12: EF Core DTO Migration Guide".

---

## 5. Project-Specific Notes

### Named Migration Mapping

`SqliteMigrationProvider.GetMigrationType()` maps `EFCoreMigration` enum to migration types. When adding named migrations, update both the enum in Infrastructure and this switch.

### Auto-Generated Files

`UmbracoDbContextModelSnapshot.cs` is regenerated by EF Core - do not edit manually.

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `SqliteCollationModelCustomizer.cs` | Applies `COLLATE NOCASE` to all string properties globally |
| `SqliteDatabaseConfigurator.cs` | DbContext configuration (IDatabaseConfigurator) |
| `SqliteDbContextServiceRegistrar.cs` | Provider service registration (IDbContextServiceRegistrar) |
| `SqliteEFCoreDistributedLockingMechanism.cs` | Distributed locking |
| `SqliteMigrationProvider.cs` | Migration execution |
| `SqliteMigrationProviderSetup.cs` | Migration DbContext configuration |
| `SqliteRetryingExecutionStrategy.cs` | Retry on transient SQLite BUSY/LOCKED errors |
| `EFCoreSqliteComposer.cs` | DI registration |
| `UmbracoBuilderExtensions.cs` | Builder extensions |
| `Migrations/*.cs` | Migration files |

### Provider Name
`Constants.ProviderNames.SQLLite` = `"Microsoft.Data.Sqlite"`

### Related Projects
- **Parent**: `Umbraco.Infrastructure` - EF Core abstractions, DbContext, provider registration contracts
- **Sibling**: `Umbraco.Cms.Persistence.EFCore.SqlServer` - SQL Server equivalent
