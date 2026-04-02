# Umbraco.Cms.Persistence.SqlServer

SQL Server database provider for Umbraco CMS using NPoco ORM. Provides SQL Server-specific SQL syntax, bulk copy operations, distributed locking, and Azure SQL transient fault handling.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Dependencies**: Umbraco.Infrastructure, NPoco.SqlServer, Microsoft.Data.SqlClient

**Note**: This is the **legacy NPoco-based** SQL Server provider. For EF Core SQL Server support, see `Umbraco.Cms.Persistence.EFCore.SqlServer`.

---

## 1. Architecture

### Project Purpose

This project provides complete SQL Server support for Umbraco's NPoco-based persistence layer:

1. **SQL Syntax Provider** - SQL Server-specific SQL generation and schema operations
2. **Bulk Insert** - True `SqlBulkCopy` for high-performance batch inserts
3. **Distributed Locking** - Row-level locking with `REPEATABLEREAD` hints
4. **Fault Handling** - Azure SQL and network transient error retry policies
5. **LocalDB Support** - SQL Server LocalDB for development

### Folder Structure

```
Umbraco.Cms.Persistence.SqlServer/
├── Dtos/
│   ├── ColumnInSchemaDto.cs              # Schema query results
│   ├── ConstraintPerColumnDto.cs
│   ├── ConstraintPerTableDto.cs
│   ├── DefaultConstraintPerColumnDto.cs
│   └── DefinedIndexDto.cs
├── FaultHandling/
│   ├── RetryPolicyFactory.cs             # Creates retry policies
│   └── Strategies/
│       ├── NetworkConnectivityErrorDetectionStrategy.cs
│       ├── SqlAzureTransientErrorDetectionStrategy.cs  # Azure SQL error codes
│       └── ThrottlingCondition.cs        # Azure throttling decode
├── Interceptors/
│   ├── SqlServerAddMiniProfilerInterceptor.cs
│   ├── SqlServerAddRetryPolicyInterceptor.cs
│   └── SqlServerConnectionInterceptor.cs  # Base interceptor
├── Services/
│   ├── BulkDataReader.cs                 # Base bulk reader
│   ├── MicrosoftSqlSyntaxProviderBase.cs # Shared SQL syntax
│   ├── PocoDataDataReader.cs             # NPoco bulk reader
│   ├── SqlServerBulkSqlInsertProvider.cs # SqlBulkCopy wrapper
│   ├── SqlServerDatabaseCreator.cs
│   ├── SqlServerDistributedLockingMechanism.cs
│   ├── SqlServerSpecificMapperFactory.cs
│   ├── SqlServerSyntaxProvider.cs        # Main syntax provider
│   ├── SqlAzureDatabaseProviderMetadata.cs
│   ├── SqlLocalDbDatabaseProviderMetadata.cs
│   └── SqlServerDatabaseProviderMetadata.cs
├── Constants.cs                          # Provider name
├── LocalDb.cs                            # LocalDB management (~1,100 lines)
├── NPocoSqlServerDatabaseExtensions.cs   # NPoco bulk extensions config
├── SqlServerComposer.cs                  # DI auto-registration
└── UmbracoBuilderExtensions.cs           # AddUmbracoSqlServerSupport()
```

### Auto-Registration

When this assembly is referenced, `SqlServerComposer` automatically registers all SQL Server services via `AddUmbracoSqlServerSupport()`.

### Provider Name Migration

Auto-migrates legacy `System.Data.SqlClient` to `Microsoft.Data.SqlClient`:

**Line 53-59**: `UmbracoBuilderExtensions.cs` updates connection string provider name
```csharp
if (options.ProviderName == "System.Data.SqlClient")
{
    options.ProviderName = Constants.ProviderName; // Microsoft.Data.SqlClient
}
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### SqlServerDistributedLockingMechanism (Services/SqlServerDistributedLockingMechanism.cs)

Row-level locking using `REPEATABLEREAD` table hints:

- **Read locks** (line 147): `SELECT value FROM umbracoLock WITH (REPEATABLEREAD) WHERE id=@id`
- **Write locks** (line 182-183): `UPDATE umbracoLock WITH (REPEATABLEREAD) SET value = ... WHERE id=@id`
- **Timeout**: Uses `SET LOCK_TIMEOUT {milliseconds}` (lines 149, 185)
- **Error handling**: Catches SQL error 1222 (lock timeout) → throws `DistributedReadLockTimeoutException` or `DistributedWriteLockTimeoutException`
- **Minimum isolation**: Requires `ReadCommitted` or higher (lines 141-145, 176-180)

### SqlServerBulkSqlInsertProvider (Services/SqlServerBulkSqlInsertProvider.cs)

True bulk insert using `SqlBulkCopy`:

```csharp
// Line 61-68: SqlBulkCopy configuration
new SqlBulkCopy(tConnection, SqlBulkCopyOptions.Default, tTransaction)
{
    BulkCopyTimeout = 0,           // No timeout (uses connection timeout)
    DestinationTableName = tableName,
    BatchSize = 4096,              // Consistent with NPoco
}
```

**Key features**:
- Streams data via `PocoDataDataReader<T>` (line 69) - avoids in-memory DataTable
- Explicit column mappings by name (lines 74-77) - prevents column order mismatches
- Returns actual count inserted (line 80) - NPoco's `InsertBulk` doesn't provide this

### SqlAzureTransientErrorDetectionStrategy (FaultHandling/Strategies/)

Detects transient Azure SQL errors for retry:

| Error Code | Description |
|------------|-------------|
| 40501 | Service busy (throttling) |
| 10928/10929 | Resource limits reached |
| 10053/10054 | Transport-level errors |
| 10060 | Connection timeout |
| 40197/40540/40143 | Service processing errors |
| 40613 | Database not available |
| 233 | Connection initialization error |
| 64 | Login process error |

### SqlServerSyntaxProvider (Services/SqlServerSyntaxProvider.cs)

SQL Server version detection and syntax generation:

- **Engine editions**: Desktop, Standard, Enterprise, Express, Azure (lines 23-31)
- **SQL Server versions**: V7 through V2019, plus Azure (lines 33-47)
- **Default isolation**: `ReadCommitted` (line 69)
- **Azure detection**: `ServerVersion?.IsAzure` determines `DbProvider` (line 67)
- **Version detection**: Populated via `GetDbProviderManifest()` by querying SQL Server metadata

---

## 4. LocalDB Support

`LocalDb.cs` (~1,100 lines) provides comprehensive SQL Server LocalDB management:

- Database creation/deletion
- Instance management
- File path handling
- Connection string generation

**Known issues** (from TODO comments):
- Line 358: Stale database handling not implemented
- Line 359: File name assumptions may not always be correct

---

## 5. Differences from SQLite Provider

| Feature | SQL Server | SQLite |
|---------|-----------|--------|
| **Bulk Insert** | `SqlBulkCopy` (true bulk) | Transaction + individual inserts |
| **Locking** | Row-level with `REPEATABLEREAD` hints | Database-level (WAL mode) |
| **Types** | Native (NVARCHAR, DECIMAL, UNIQUEIDENTIFIER) | TEXT for most types |
| **Transient Retry** | Azure SQL error codes + network errors | BUSY/LOCKED retry only |
| **LocalDB** | Full support (~1,100 lines) | N/A |

---

## 6. Project-Specific Notes

### InternalsVisibleTo

Test projects have access to internal types:
```xml
<InternalsVisibleTo>Umbraco.Tests.Integration</InternalsVisibleTo>
<InternalsVisibleTo>Umbraco.Tests.UnitTests</InternalsVisibleTo>
```

### Known Technical Debt

1. **Warning Suppressions** (`.csproj:8-22`): Multiple analyzer warnings suppressed
   - StyleCop: SA1405, SA1121, SA1117
   - IDE: 1006 (naming), 0270/0057/0054/0048 (simplification)
   - CS0618 (obsolete usage), CS1574 (XML comments)

2. **BulkInsert TODO** (`SqlServerBulkSqlInsertProvider.cs:44-46`): Custom `SqlBulkCopy` implementation used because NPoco's `InsertBulk` doesn't return record count. Performance comparison vs NPoco's DataTable approach pending.

3. **LocalDB TODOs** (`LocalDb.cs:358-359`): Stale database cleanup and file name assumption handling incomplete.

### NPoco Bulk Extensions

`NPocoSqlServerDatabaseExtensions.ConfigureNPocoBulkExtensions()` is called during registration (line 50) to configure NPoco's SQL Server bulk operations.

### Three Provider Metadata Types

Different metadata for different SQL Server variants:
- `SqlServerDatabaseProviderMetadata` - Standard SQL Server
- `SqlLocalDbDatabaseProviderMetadata` - LocalDB
- `SqlAzureDatabaseProviderMetadata` - Azure SQL

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `SqlServerSyntaxProvider.cs` | Core SQL syntax provider |
| `SqlServerDistributedLockingMechanism.cs` | Row-level locking |
| `SqlServerBulkSqlInsertProvider.cs` | SqlBulkCopy wrapper |
| `SqlAzureTransientErrorDetectionStrategy.cs` | Azure retry logic |
| `LocalDb.cs` | LocalDB management |

### Provider Name
`Constants.ProviderName` = `"Microsoft.Data.SqlClient"`

### Registered Services

All registered via `TryAddEnumerable`:
- `ISqlSyntaxProvider` → `SqlServerSyntaxProvider`
- `IBulkSqlInsertProvider` → `SqlServerBulkSqlInsertProvider`
- `IDatabaseCreator` → `SqlServerDatabaseCreator`
- `IProviderSpecificMapperFactory` → `SqlServerSpecificMapperFactory`
- `IDatabaseProviderMetadata` → Three variants (SqlServer, LocalDb, Azure)
- `IDistributedLockingMechanism` → `SqlServerDistributedLockingMechanism`
- `IProviderSpecificInterceptor` → MiniProfiler and RetryPolicy interceptors

### Related Projects
- **Sibling**: `Umbraco.Cms.Persistence.Sqlite` - SQLite NPoco provider
- **EF Core**: `Umbraco.Cms.Persistence.EFCore.SqlServer` - EF Core SQL Server provider
