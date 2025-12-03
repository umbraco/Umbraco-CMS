# Umbraco.Cms.Persistence.Sqlite

SQLite database provider for Umbraco CMS using NPoco ORM. Provides SQLite-specific SQL syntax, type mappers, distributed locking, and connection interceptors.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Dependencies**: Umbraco.Infrastructure, Microsoft.Data.Sqlite

**Note**: This is the **legacy NPoco-based** SQLite provider. For EF Core SQLite support, see `Umbraco.Cms.Persistence.EFCore.Sqlite`.

---

## 1. Architecture

### Project Purpose

This project provides complete SQLite support for Umbraco's NPoco-based persistence layer:

1. **SQL Syntax Provider** - SQLite-specific SQL generation and schema operations
2. **Type Mappers** - GUID, decimal, date/time mapping for SQLite's type system
3. **Distributed Locking** - SQLite-specific locking using WAL mode
4. **Connection Interceptors** - Deferred transactions, MiniProfiler, retry policies
5. **Bulk Insert** - SQLite-optimized record insertion

### Folder Structure

```
Umbraco.Cms.Persistence.Sqlite/
├── Interceptors/
│   ├── SqliteAddMiniProfilerInterceptor.cs     # MiniProfiler integration
│   ├── SqliteAddPreferDeferredInterceptor.cs   # Deferred transaction wrapper
│   ├── SqliteAddRetryPolicyInterceptor.cs      # Transient error retry
│   └── SqliteConnectionInterceptor.cs          # Base interceptor
├── Mappers/
│   ├── SqliteGuidScalarMapper.cs               # GUID as TEXT mapping
│   ├── SqlitePocoDateAndTimeOnlyMapper.cs      # Date/Time as TEXT
│   ├── SqlitePocoDecimalMapper.cs              # Decimal as TEXT (lossless)
│   └── SqlitePocoGuidMapper.cs                 # GUID for NPoco
├── Services/
│   ├── SqliteBulkSqlInsertProvider.cs          # Bulk insert via transactions
│   ├── SqliteDatabaseCreator.cs                # Database initialization
│   ├── SqliteDatabaseProviderMetadata.cs       # Provider info
│   ├── SqliteDistributedLockingMechanism.cs    # WAL-based locking
│   ├── SqliteExceptionExtensions.cs            # Error code helpers
│   ├── SqlitePreferDeferredTransactionsConnection.cs  # Deferred tx wrapper
│   ├── SqliteSpecificMapperFactory.cs          # Type mapper factory
│   ├── SqliteSyntaxProvider.cs                 # SQL syntax (481 lines)
│   └── SqliteTransientErrorDetectionStrategy.cs # BUSY/LOCKED detection
├── Constants.cs                                 # Provider name constant
├── SqliteComposer.cs                           # DI auto-registration
└── UmbracoBuilderExtensions.cs                 # AddUmbracoSqliteSupport()
```

### Auto-Registration

When this assembly is referenced, `SqliteComposer` automatically registers all SQLite services via the `AddUmbracoSqliteSupport()` extension method.

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### SqliteSyntaxProvider (Services/SqliteSyntaxProvider.cs)

The core SQL syntax provider implementing `ISqlSyntaxProvider`. Key characteristics:

- **All strings stored as TEXT** with `COLLATE NOCASE` (case-insensitive)
- **GUIDs stored as TEXT** (no native GUID support)
- **Decimals stored as TEXT** to avoid REAL precision loss (line 51)
- **Default isolation level**: `IsolationLevel.Serializable` (line 66-67)
- **No identity insert support** (line 73)
- **No clustered index support** (line 76)
- **LIMIT instead of TOP** for pagination (line 273-278)
- **AUTOINCREMENT required** to prevent magic ID issues with negative IDs (line 224-228)

### SqliteDistributedLockingMechanism (Services/SqliteDistributedLockingMechanism.cs)

Database-level locking using SQLite WAL mode:

- **Read locks**: Snapshot isolation via WAL (mostly no-op, just validates transaction exists)
- **Write locks**: Only one writer at a time (entire database locked)
- **Timeout handling**: Uses `CommandTimeout` for busy-wait (line 163)
- **Error handling**: Catches `SQLITE_BUSY` and `SQLITE_LOCKED` errors

### SqlitePreferDeferredTransactionsConnection (Services/SqlitePreferDeferredTransactionsConnection.cs)

Wraps `SqliteConnection` to force deferred transactions:

```csharp
// Line 33-34: The critical behavior
protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    => _inner.BeginTransaction(isolationLevel, true); // <-- The important bit
```

This prevents immediate write locks when beginning transactions, allowing multiple readers.

### Connection Interceptors

- **SqliteAddPreferDeferredInterceptor** - Wraps connections with deferred transaction support
- **SqliteAddMiniProfilerInterceptor** - Adds profiling for development
- **SqliteAddRetryPolicyInterceptor** - Retries on transient SQLite errors

---

## 4. SQLite Type Mapping

SQLite has limited types. Umbraco maps .NET types as follows:

| .NET Type | SQLite Type | Notes |
|-----------|-------------|-------|
| `int`, `long`, `bool` | INTEGER | Native support |
| `string` | TEXT COLLATE NOCASE | Case-insensitive |
| `Guid` | TEXT | Stored as string representation |
| `DateTime`, `DateTimeOffset` | TEXT | ISO format string |
| `decimal` | TEXT | Prevents REAL precision loss |
| `double`, `float` | REAL | Native support |
| `byte[]` | BLOB | Native support |

---

## 5. Project-Specific Notes

### Database Creation Prevention

`UmbracoBuilderExtensions.cs` (line 49-64) prevents accidental database file creation:

```csharp
// Changes ReadWriteCreate mode to ReadWrite only
if (connectionStringBuilder.Mode == SqliteOpenMode.ReadWriteCreate)
{
    connectionStringBuilder.Mode = SqliteOpenMode.ReadWrite;
}
```

This ensures the database must exist before Umbraco connects.

### WAL Mode Locking

SQLite with WAL (Write-Ahead Logging) journal mode:
- Multiple readers can access snapshots concurrently
- Only one writer at a time (database-level lock)
- Write lock uses `umbracoLock` table with UPDATE to acquire

### Bulk Insert Implementation

`SqliteBulkSqlInsertProvider` (line 38-60) doesn't use true bulk copy (SQLite doesn't support it). Instead:
1. Wraps inserts in a transaction if not already in one
2. Inserts records one-by-one with `database.Insert()`
3. Completes transaction

This is slower than SQL Server's `SqlBulkCopy` but safe for SQLite.

### Known Technical Debt

1. **Warning Suppression** (`.csproj:8-12`): `CS0114` - hiding inherited members needs fixing
2. **TODO in SqliteSyntaxProvider** (line 178): `TryGetDefaultConstraint` not implemented for SQLite

### Differences from SQL Server Provider

| Feature | SQLite | SQL Server |
|---------|--------|------------|
| Bulk Insert | Transaction + individual inserts | SqlBulkCopy |
| Locking | Database-level (WAL) | Row-level |
| Identity Insert | Not supported | Supported |
| Clustered Indexes | Not supported | Supported |
| TOP clause | LIMIT | TOP |
| Decimal | TEXT (lossless) | DECIMAL |
| GUID | TEXT | UNIQUEIDENTIFIER |

---

## Quick Reference

### Essential Files

| File | Purpose |
|------|---------|
| `SqliteSyntaxProvider.cs` | Core SQL syntax provider |
| `SqliteDistributedLockingMechanism.cs` | Database locking |
| `UmbracoBuilderExtensions.cs` | DI registration |
| `SqlitePreferDeferredTransactionsConnection.cs` | Deferred tx support |

### Provider Name
`Constants.ProviderName` = `"Microsoft.Data.Sqlite"`

### Registered Services

All registered via `TryAddEnumerable` (allowing multiple providers):
- `ISqlSyntaxProvider` → `SqliteSyntaxProvider`
- `IBulkSqlInsertProvider` → `SqliteBulkSqlInsertProvider`
- `IDatabaseCreator` → `SqliteDatabaseCreator`
- `IProviderSpecificMapperFactory` → `SqliteSpecificMapperFactory`
- `IDatabaseProviderMetadata` → `SqliteDatabaseProviderMetadata`
- `IDistributedLockingMechanism` → `SqliteDistributedLockingMechanism`
- `IProviderSpecificInterceptor` → Three interceptors

### Related Projects
- **Sibling**: `Umbraco.Cms.Persistence.SqlServer` - SQL Server NPoco provider
- **EF Core**: `Umbraco.Cms.Persistence.EFCore.Sqlite` - EF Core SQLite provider
