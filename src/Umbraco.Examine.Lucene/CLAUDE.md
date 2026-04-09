# Umbraco.Examine.Lucene

Full-text search provider for Umbraco CMS using Examine and Lucene.NET. Provides content, media, member, and Delivery API indexing with configurable directory factories and index diagnostics.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Package ID**: Umbraco.Cms.Examine.Lucene
**Namespace**: Umbraco.Cms.Infrastructure.Examine
**Dependencies**: Umbraco.Infrastructure, Examine (Lucene.NET wrapper)

---

## 1. Architecture

### Project Purpose

This project provides Lucene-based full-text search capabilities for Umbraco:

1. **Index Types** - Content, Media, Member, and Delivery API indexes
2. **Directory Factories** - Configurable index storage (filesystem, temp, synced)
3. **Index Diagnostics** - Health checks and metadata for backoffice
4. **Backoffice Search** - Unified search across content tree with permissions

### Folder Structure

```
Umbraco.Examine.Lucene/
├── DependencyInjection/
│   ├── ConfigureIndexOptions.cs       # Per-index configuration (77 lines)
│   └── UmbracoBuilderExtensions.cs    # AddExamineIndexes() registration (64 lines)
├── Extensions/
│   └── ExamineExtensions.cs           # Lucene query parsing, health check
├── AddExamineComposer.cs              # Auto-registration via IComposer
├── BackOfficeExamineSearcher.cs       # Unified backoffice search (532 lines)
├── ConfigurationEnabledDirectoryFactory.cs  # Directory factory selector
├── DeliveryApiContentIndex.cs         # Headless API index with culture support
├── LuceneIndexDiagnostics.cs          # Base diagnostics implementation
├── LuceneIndexDiagnosticsFactory.cs   # Diagnostics factory
├── LuceneRAMDirectoryFactory.cs       # In-memory directory for testing
├── NoPrefixSimpleFsLockFactory.cs     # Lock factory without prefix
├── UmbracoApplicationRoot.cs          # Application root path provider
├── UmbracoContentIndex.cs             # Content/media index (158 lines)
├── UmbracoExamineIndex.cs             # Base index class (153 lines)
├── UmbracoExamineIndexDiagnostics.cs  # Extended diagnostics
├── UmbracoLockFactory.cs              # Lock factory wrapper
├── UmbracoMemberIndex.cs              # Member index
└── UmbracoTempEnvFileSystemDirectoryFactory.cs  # Temp directory factory
```

### Index Hierarchy

```
LuceneIndex (Examine)
    └── UmbracoExamineIndex (base for all Umbraco indexes)
            ├── UmbracoContentIndex (content + media)
            ├── UmbracoMemberIndex (members)
            └── DeliveryApiContentIndex (headless API)
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### Built-in Indexes (Constants.UmbracoIndexes)

| Index Name | Type | Purpose |
|------------|------|---------|
| `InternalIndex` | UmbracoContentIndex | Backoffice search (all content) |
| `ExternalIndex` | UmbracoContentIndex | Frontend published content |
| `MembersIndex` | UmbracoMemberIndex | Member search |
| `DeliveryApiContentIndex` | DeliveryApiContentIndex | Headless API content |

**Note:** See [ConfigureIndexOptions](#configureindexoptions-dependencyinjectionconfigureindexoptionscs) for per-index analyzer and validator configuration.

### UmbracoExamineIndex (UmbracoExamineIndex.cs)

Base class for all Umbraco indexes. Key features:

**Runtime State Check** (lines 84-95):
```csharp
protected bool CanInitialize()
{
    var canInit = _runtimeState.Level == RuntimeLevel.Run;
    // Logs warning once if runtime not ready
}
```
Prevents indexing during install/upgrade.

**Special Field Transformations** (lines 130-152):
- `__Path` field - Enables descendant queries via path matching
- `__Icon` field - Preserves icon for display

**Raw Field Storage** (lines 101-118):
Fields prefixed with `__Raw_` are stored as `StoredField` (not analyzed), used for returning exact values.

### UmbracoContentIndex (UmbracoContentIndex.cs)

Handles content and media indexing with validation and cascade deletes.

**Cascade Delete** (lines 128-157):
When content deleted, automatically finds and removes all descendants:
```csharp
var descendantPath = $@"\-1\,*{nodeId}\,*";
var rawQuery = $"{UmbracoExamineFieldNames.IndexPathFieldName}:{descendantPath}";
```

**Validation Groups** (lines 66-116):
- `Valid` - Index normally
- `Failed` - Skip (invalid data)
- `Filtered` - Delete from index (moved to recycle bin)

### DeliveryApiContentIndex (DeliveryApiContentIndex.cs)

Specialized index for Delivery API with culture support.

**Key Differences** (lines 20-34):
- `ApplySpecialValueTransformations = false` - No path/icon transformations
- `PublishedValuesOnly = false` - Handles via populator
- `EnableDefaultEventHandler = false` - Custom event handling

**Composite IDs** (lines 118-128):
Index IDs can be composite: `"1234|da-DK"` (content ID + culture) or simple: `"1234"`.

### BackOfficeExamineSearcher (BackOfficeExamineSearcher.cs)

Unified search for backoffice tree with user permissions.

**Search Features**:
- Node name boosting (10x for exact match, line 316-327)
- Wildcard support for partial matches (line 350-376)
- User start node filtering (lines 378-456)
- Recycle bin filtering (lines 185-191)
- Multi-language variant search (all `nodeName_{lang}` fields)

**Entity Type Routing** (lines 89-155): Member→MembersIndex, Media→InternalIndex, Document→InternalIndex

---

## 4. Directory Factories

### Configuration Options (IndexCreatorSettings.LuceneDirectoryFactory)

- `Default` - FileSystemDirectoryFactory (standard filesystem storage)
- `TempFileSystemDirectoryFactory` - UmbracoTempEnvFileSystemDirectoryFactory (store in `%TEMP%/ExamineIndexes`)
- `SyncedTempFileSystemDirectoryFactory` - SyncedFileSystemDirectoryFactory (temp with sync to persistent)

### UmbracoTempEnvFileSystemDirectoryFactory (lines 20-34)

Creates unique temp path using site name + application identifier hash:
```csharp
var hashString = hostingEnvironment.SiteName + "::" + applicationIdentifier.GetApplicationUniqueIdentifier();
var cachePath = Path.Combine(Path.GetTempPath(), "ExamineIndexes", appDomainHash);
```

**Purpose**: Prevents index collisions when same app moves between workers in load-balanced scenarios.

### ConfigurationEnabledDirectoryFactory (lines 34-62)

Selector that creates appropriate factory based on `IndexCreatorSettings.LuceneDirectoryFactory` config value.

---

## 5. Index Configuration

### ConfigureIndexOptions (DependencyInjection/ConfigureIndexOptions.cs)

Configures per-index options via `IConfigureNamedOptions<LuceneDirectoryIndexOptions>`.

**Per-Index Settings** (lines 39-61):

| Index | Analyzer | Validator |
|-------|----------|-----------|
| InternalIndex | CultureInvariantWhitespaceAnalyzer | ContentValueSetValidator |
| ExternalIndex | StandardAnalyzer | PublishedContentValueSetValidator |
| MembersIndex | CultureInvariantWhitespaceAnalyzer | MemberValueSetValidator |
| DeliveryApiContentIndex | StandardAnalyzer | None (populator handles) |

**Global Settings** (lines 63-70):
- `UnlockIndex = true` - Always unlock on startup
- Snapshot deletion policy when using SyncedTempFileSystemDirectoryFactory

---

## 6. Index Diagnostics

### LuceneIndexDiagnostics (LuceneIndexDiagnostics.cs)

Provides health checks and metadata for backoffice examine dashboard.

**Health Check** (lines 41-45):
```csharp
public Attempt<string?> IsHealthy()
{
    var isHealthy = Index.IsHealthy(out Exception? indexError);
    return isHealthy ? Attempt<string?>.Succeed() : Attempt.Fail(indexError?.Message);
}
```

**Metadata** (lines 51-92):
- CommitCount, DefaultAnalyzer, LuceneDirectory type
- LuceneIndexFolder (relative path)
- DirectoryFactory and IndexDeletionPolicy types

### UmbracoExamineIndexDiagnostics (UmbracoExamineIndexDiagnostics.cs)

Extends base with Umbraco-specific metadata (lines 23-48):
- EnableDefaultEventHandler
- PublishedValuesOnly
- Validator settings (IncludeItemTypes, ExcludeItemTypes, etc.)

---

## 7. Project-Specific Notes

### Auto-Registration

`AddExamineComposer` (lines 10-14) automatically registers all Examine services:
```csharp
builder
    .AddExamine()
    .AddExamineIndexes();
```

### Service Registrations (UmbracoBuilderExtensions.cs)

Key services registered (lines 18-62): `IBackOfficeExamineSearcher`, `IIndexDiagnosticsFactory`, `IApplicationRoot`, `ILockFactory`, plus all 4 indexes with `ConfigurationEnabledDirectoryFactory`.

### Known Technical Debt

1. **Warning Suppression** (`.csproj:10-14`): `CS0618` - Uses obsolete members in Examine/Lucene.NET

2. **TODO: Raw Query Support** (`BackOfficeExamineSearcher.cs:71-76`):
   ```csharp
   // TODO: WE should try to allow passing in a lucene raw query, however we will still need to do some manual string
   // manipulation for things like start paths, member types, etc...
   ```

3. **TODO: Query Parsing** (`ExamineExtensions.cs:21-22`):
   ```csharp
   // TODO: I'd assume there would be a more strict way to parse the query but not that i can find yet
   ```

### Index Field Names

Key fields from `UmbracoExamineFieldNames` (defined in Umbraco.Core):
- `__Path` - Content path for descendant queries
- `__Icon` - Content icon
- `__Raw_*` - Raw stored values (not analyzed)
- `__Key` - Content GUID
- `__NodeTypeAlias` - Content type alias
- `__IndexType` - `content`, `media`, or `member`

### Lucene Query Escaping

`BackOfficeExamineSearcher.BuildQuery()` (lines 193-311) handles:
- Special characters `*`, `-`, `_` removal/replacement
- Quoted phrase search (`"exact match"`)
- Query escaping via `QueryParserBase.Escape()`
- Path escaping (replace `-` with `\-`, `,` with `\,`)

### User Start Node Filtering

`BackOfficeExamineSearcher.AppendPath()` (lines 378-456) ensures users only see content they have access to:
1. Gets user's start nodes from `IBackOfficeSecurityAccessor`
2. Filters search results to paths under start nodes
3. Returns empty results if searchFrom outside user's access

---

## Quick Reference

**Index Names** (Constants.UmbracoIndexes):
- `InternalIndexName` = "InternalIndex"
- `ExternalIndexName` = "ExternalIndex"
- `MembersIndexName` = "MembersIndex"
- `DeliveryApiContentIndexName` = "DeliveryApiContentIndex"

**Key Interfaces**:
- `IUmbracoIndex` - Marker for Umbraco indexes
- `IUmbracoContentIndex` - Content index marker
- `IBackOfficeExamineSearcher` - Backoffice search service
- `IIndexDiagnostics` - Health/metadata for dashboard

**Dependencies**:
- `Umbraco.Infrastructure` - Core services, index populators
- `Umbraco.Core` - Field names, constants, interfaces
- `Examine` (NuGet) - Lucene.NET abstraction
