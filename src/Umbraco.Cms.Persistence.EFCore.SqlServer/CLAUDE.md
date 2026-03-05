# Umbraco.Cms.Persistence.EFCore.SqlServer

SQL Server-specific EF Core provider for Umbraco CMS. Implements SQL Server database configuration, distributed locking, migrations, and service registration for the EF Core persistence layer.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Dependencies**: Umbraco.Infrastructure

---

## 1. Architecture

### Project Purpose

This provider project implements SQL Server-specific functionality for the EF Core persistence layer:

1. **Database Configurator** - Configures DbContext with SQL Server-specific options (`IDatabaseConfigurator`)
2. **Service Registrar** - Registers SQL Server-specific services like distributed locking (`IDbContextServiceRegistrar`)
3. **Distributed Locking** - SQL Server-specific distributed locking mechanism
4. **Migration Provider** - Executes SQL Server-specific EF Core migrations
5. **Migration Provider Setup** - Configures DbContext to use SQL Server for migrations
6. **Migrations** - SQL Server-specific migration files (OpenIddict tables, webhooks, etc.)

### Folder Structure

```
Umbraco.Cms.Persistence.EFCore.SqlServer/
├── Migrations/
│   ├── 20230622184303_InitialCreate.cs          # No-op (NPoco creates tables)
│   ├── 20230807654321_AddOpenIddict.cs          # OpenIddict tables
│   ├── 20240403140654_UpdateOpenIddictToV5.cs   # OpenIddict v5 schema changes
│   ├── 20251006140751_UpdateOpenIddictToV7.cs   # Token Type column expansion
│   ├── 20260209102250_AddWebhookDto.cs          # Webhook tables
│   └── UmbracoDbContextModelSnapshot.cs         # Current model state
├── EFCoreSqlServerComposer.cs                   # DI registration
├── SqlServerDatabaseConfigurator.cs             # IDatabaseConfigurator impl
├── SqlServerDbContextServiceRegistrar.cs        # IDbContextServiceRegistrar impl
├── SqlServerEFCoreDistributedLockingMechanism.cs # Distributed locking
├── SqlServerMigrationProvider.cs                # IMigrationProvider impl
├── SqlServerMigrationProviderSetup.cs           # IMigrationProviderSetup impl
└── UmbracoBuilderExtensions.cs                  # IUmbracoBuilder extensions
```

### Relationship with Infrastructure

This project extends `Umbraco.Infrastructure`:

- Implements `IDatabaseConfigurator` interface defined in Infrastructure
- Implements `IDbContextServiceRegistrar` interface defined in Infrastructure
- Implements `IMigrationProvider` interface defined in Infrastructure
- Implements `IMigrationProviderSetup` interface defined in Infrastructure
- Uses `UmbracoDbContext` from Infrastructure
- Provider name: `Microsoft.Data.SqlClient` (from `Constants.ProviderNames.SQLServer`)

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

```bash
# Build this project
dotnet build src/Umbraco.Cms.Persistence.EFCore.SqlServer

# Run related tests
dotnet test --filter "FullyQualifiedName~EFCore"
```

---

## 3. Key Components

### EFCoreSqlServerComposer

Registers SQL Server implementations of `IMigrationProvider`, `IMigrationProviderSetup`, and provider registration services.

### SqlServerDatabaseConfigurator

Implements `IDatabaseConfigurator` to configure `DbContextOptionsBuilder` with `UseSqlServer`. Called by `DbContextRegistration` when registering DbContext types.

### SqlServerDbContextServiceRegistrar

Implements `IDbContextServiceRegistrar` to register SQL Server-specific services (e.g., distributed locking) for each registered DbContext type.

### SqlServerEFCoreDistributedLockingMechanism

SQL Server-specific distributed locking implementation using database-level locks. Moved from Infrastructure to this provider package as it is provider-specific.

### SqlServerMigrationProvider

Executes migrations via `MigrateAsync(EFCoreMigration)` or `MigrateAllAsync()`. Unlike SQLite sibling, no transaction check needed (SQL Server handles concurrent migrations natively).

### SqlServerMigrationProviderSetup

Configures `DbContextOptionsBuilder` with `UseSqlServer` and migrations assembly.

### UmbracoBuilderExtensions

Extension methods on `IUmbracoBuilder` for adding SQL Server EF Core support to the application.

---

## 4. Migrations

### Migration History

| Migration | Date | Purpose |
|-----------|------|---------|
| `InitialCreate` | 2023-06-22 | No-op - NPoco creates base tables |
| `AddOpenIddict` | 2023-08-07 | Creates OpenIddict tables |
| `UpdateOpenIddictToV5` | 2024-04-03 | Renames Type->ClientType, adds ApplicationType/JsonWebKeySet/Settings |
| `UpdateOpenIddictToV7` | 2025-10-06 | Expands Token.Type from nvarchar(50) to nvarchar(150) |
| `AddWebhookDto` | 2026-02-09 | Webhook tables |

### OpenIddict Tables Created

Prefixed with `umbraco`: Applications, Authorizations, Scopes, Tokens (see SQLite sibling for details).

### SQL Server-Specific Differences from SQLite

1. **nvarchar types** - Uses `nvarchar(n)` and `nvarchar(max)` instead of TEXT
2. **v5 migration has actual changes** - Column rename and additions (SQLite handled differently)
3. **v7 migration has schema change** - Token.Type column expanded (SQLite didn't need this)

### Adding New Migrations

1. Configure SQL Server connection string in `src/Umbraco.Web.UI/appsettings.json`
2. Run migration command from repository root
3. **Critical**: Also add equivalent migration to `Umbraco.Cms.Persistence.EFCore.Sqlite`
4. Update `SqlServerMigrationProvider.GetMigrationType()` switch if adding named migrations

### Model Customizers (`DtoCustomization/`)

SQL Server-specific EF Core model customizations (e.g., indexes with included columns via `.IncludeProperties()`) live in `DtoCustomization/`. These extend the shared configurations in `Umbraco.Infrastructure/Persistence/Dtos/EFCore/Configurations/`.

**Pattern**: Implement `IEFCoreModelCustomizer<TDto>` and register via `builder.AddEFCoreModelCustomizer<T>()` in `UmbracoBuilderExtensions.AddCustomizers()`.

**Current customizers**:
- `SqlServerNodeDtoModelCustomizer` — adds `.IncludeProperties()` to `NodeDto` indexes
- `SqlServerDocumentVersionDtoModelCustomizer` — adds `.IncludeProperties()` to `DocumentVersionDto` indexes

**When to add a new customizer**: Only when the NPoco DTO has SQL Server-specific index features (included columns via `[IncludeColumns]`). If the DTO has no included columns, no customizer is needed.

**Full migration guide**: See `/src/Umbraco.Infrastructure/CLAUDE.md` → "Section 12: EF Core DTO Migration Guide".

---

## 5. Project-Specific Notes

### Named Migration Mapping

`SqlServerMigrationProvider.GetMigrationType()` maps `EFCoreMigration` enum to migration types. Update both the enum in Infrastructure and this switch when adding named migrations.

### Transaction Handling

SQL Server handles concurrent migrations natively - no transaction check needed (unlike SQLite).

### Auto-Generated Files

`UmbracoDbContextModelSnapshot.cs` is regenerated by EF Core - do not edit manually.

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `SqlServerDatabaseConfigurator.cs` | DbContext configuration (IDatabaseConfigurator) |
| `SqlServerDbContextServiceRegistrar.cs` | Provider service registration (IDbContextServiceRegistrar) |
| `SqlServerEFCoreDistributedLockingMechanism.cs` | Distributed locking |
| `SqlServerMigrationProvider.cs` | Migration execution |
| `SqlServerMigrationProviderSetup.cs` | Migration DbContext configuration |
| `EFCoreSqlServerComposer.cs` | DI registration |
| `UmbracoBuilderExtensions.cs` | Builder extensions |
| `Migrations/*.cs` | Migration files |

### Provider Name
`Constants.ProviderNames.SQLServer` = `"Microsoft.Data.SqlClient"`

### Related Projects
- **Parent**: `Umbraco.Infrastructure` - EF Core abstractions, DbContext, provider registration contracts
- **Sibling**: `Umbraco.Cms.Persistence.EFCore.Sqlite` - SQLite equivalent
