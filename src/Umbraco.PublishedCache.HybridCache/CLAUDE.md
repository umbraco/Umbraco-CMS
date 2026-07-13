# Umbraco.PublishedCache.HybridCache

Published content caching layer for Umbraco CMS using Microsoft's HybridCache (in-memory + optional distributed cache). Provides high-performance content delivery with cache seeding, serialization, and notification-based invalidation.

**Project Type**: Class Library (NuGet package)
**Target Framework**: net10.0
**Package ID**: Umbraco.Cms.PublishedCache.HybridCache
**Namespace**: Umbraco.Cms.Infrastructure.HybridCache
**Dependencies**: Umbraco.Core, Umbraco.Infrastructure, Microsoft.Extensions.Caching.Hybrid, MessagePack, K4os.Compression.LZ4

---

## 1. Architecture

### Project Purpose

This project implements the published content cache using Microsoft's HybridCache pattern:

1. **Multi-Level Caching** - L1 (in-memory) + L2 (optional distributed cache like Redis)
2. **Cache Seeding** - Pre-populates cache on startup with frequently accessed content
3. **MessagePack Serialization** - Fast binary serialization with LZ4 compression
4. **Notification-Based Invalidation** - Automatic cache updates on content changes
5. **Draft/Published Separation** - Separate cache entries for draft and published content

### Folder Structure

```
Umbraco.PublishedCache.HybridCache/
├── DependencyInjection/
│   └── UmbracoBuilderExtensions.cs        # AddUmbracoHybridCache() registration (90 lines)
├── Extensions/
│   └── HybridCacheExtensions.cs           # ExistsAsync extension method
├── Factories/
│   ├── CacheNodeFactory.cs                # Creates ContentCacheNode from IContent
│   ├── ICacheNodeFactory.cs
│   ├── IPublishedContentFactory.cs
│   └── PublishedContentFactory.cs         # Creates IPublishedContent from cache
├── NotificationHandlers/
│   ├── CacheRefreshingNotificationHandler.cs   # Content/media change handler (120 lines)
│   ├── HybridCacheStartupNotificationHandler.cs
│   └── SeedingNotificationHandler.cs      # Startup seeding (39 lines)
├── Persistence/
│   ├── ContentSourceDto.cs                # DTO for database queries
│   ├── DatabaseCacheRepository.cs         # NPoco database access (891 lines)
│   └── IDatabaseCacheRepository.cs
├── SeedKeyProviders/
│   ├── BreadthFirstKeyProvider.cs         # Base breadth-first traversal
│   ├── Document/
│   │   ├── ContentTypeSeedKeyProvider.cs  # Seeds by content type
│   │   └── DocumentBreadthFirstKeyProvider.cs  # Seeds top-level content (83 lines)
│   └── Media/
│       └── MediaBreadthFirstKeyProvider.cs
├── Serialization/
│   ├── ContentCacheDataModel.cs           # Serializable cache model
│   ├── HybridCacheSerializer.cs           # MessagePack serializer (40 lines)
│   ├── IContentCacheDataSerializer.cs     # Nested data serializer interface
│   ├── JsonContentNestedDataSerializer.cs # JSON serializer option
│   ├── MsgPackContentNestedDataSerializer.cs  # MessagePack serializer option
│   └── LazyCompressedString.cs            # Lazy LZ4 compression
├── Services/
│   ├── DocumentCacheService.cs            # Content caching service (372 lines)
│   ├── MediaCacheService.cs               # Media caching service
│   ├── MemberCacheService.cs              # Member caching service
│   └── DomainCacheService.cs              # Domain caching service
├── CacheManager.cs                        # ICacheManager facade (27 lines)
├── ContentCacheNode.cs                    # Cache entry model (24 lines)
├── ContentData.cs                         # Content data container
├── ContentNode.cs                         # Content node representation
├── DatabaseCacheRebuilder.cs              # Full cache rebuild
├── DocumentCache.cs                       # IPublishedContentCache impl (58 lines)
├── MediaCache.cs                          # IPublishedMediaCache impl
├── MemberCache.cs                         # IPublishedMemberCache impl
├── DomainCache.cs                         # IDomainCache impl
└── PublishedContent.cs                    # IPublishedContent impl
```

### Cache Architecture

```
Request → DocumentCache (IPublishedContentCache)
              ↓
         DocumentCacheService
              ↓
    ┌─────────────────────────┐
    │     HybridCache         │
    │  ┌───────┐  ┌────────┐  │
    │  │  L1   │  │   L2   │  │
    │  │Memory │→ │ Redis  │  │
    │  └───────┘  └────────┘  │
    └─────────────────────────┘
              ↓ (cache miss)
    DatabaseCacheRepository
              ↓
    cmsContentNu table
```

---

## 2. Commands

**For Git workflow and build commands**, see [repository root](../../CLAUDE.md).

---

## 3. Key Components

### HybridCache Configuration (UmbracoBuilderExtensions.cs)

**Registration** (lines 30-36):
```csharp
builder.Services.AddHybridCache(options =>
{
    // Default 100MB max payload (configurable)
    options.MaximumPayloadBytes = 1024 * 1024 * 100;
}).AddSerializer<ContentCacheNode, HybridCacheSerializer>();
```

**Key Services Registered** (lines 38-52):
- `IPublishedContentCache` → `DocumentCache`
- `IPublishedMediaCache` → `MediaCache`
- `IPublishedMemberCache` → `MemberCache`
- `IDomainCache` → `DomainCache`
- `IDocumentCacheService` → `DocumentCacheService`
- `IMediaCacheService` → `MediaCacheService`
- `ICacheManager` → `CacheManager`

### DocumentCacheService (Services/DocumentCacheService.cs)

Core caching logic for content.

**GetNodeAsync** (lines 110-155):
1. Check in-memory `_publishedContentCache` dictionary
2. Call `HybridCache.GetOrCreateAsync()` with database fallback
3. Verify published ancestor path exists
4. Convert `ContentCacheNode` to `IPublishedContent`

**Cache Key Format** (line 326):
```csharp
private static string GetCacheKey(Guid key, bool preview)
    => preview ? $"{key}+draft" : $"{key}";
```

**Cache Entry Options** (lines 275-287):
- Seed entries use `SeedCacheDuration` from `CacheSettings`
- Regular entries use `LocalCacheDuration` and `RemoteCacheDuration`
- Preview/draft entries always use regular (non-seed) durations

### ContentCacheNode (ContentCacheNode.cs)

Immutable cache entry model (marked `[ImmutableObject(true)]` for HybridCache performance):
- `Id`, `Key`, `SortOrder`, `CreateDate`, `CreatorId`
- `ContentTypeId`, `IsDraft`
- `ContentData? Data` - Property values, culture data, URL segments

### HybridCacheSerializer (Serialization/HybridCacheSerializer.cs)

MessagePack serialization with LZ4 compression:
```csharp
_options = defaultOptions
    .WithCompression(MessagePackCompression.Lz4BlockArray)
    .WithSecurity(MessagePackSecurity.UntrustedData);
```

---

## 4. Cache Seeding

### Seed Key Providers

| Provider | Purpose | Config Setting |
|----------|---------|----------------|
| `ContentTypeSeedKeyProvider` | Seeds specific content types | N/A |
| `DocumentBreadthFirstKeyProvider` | Seeds top-level content first | `DocumentBreadthFirstSeedCount` |
| `MediaBreadthFirstKeyProvider` | Seeds top-level media first | `MediaBreadthFirstSeedCount` |

### DocumentBreadthFirstKeyProvider (lines 28-82)

Breadth-first traversal that:
1. Gets root content keys
2. Filters to only published content (any culture)
3. Traverses children breadth-first until seed count reached
4. Only includes content with published ancestor path

### SeedingNotificationHandler (lines 27-38)

Runs on `UmbracoApplicationStartingNotification`:
- Skips during Install or Upgrade (when maintenance page shown)
- Calls `DocumentCacheService.SeedAsync()` then `MediaCacheService.SeedAsync()`

### Seed Process (DocumentCacheService.SeedAsync lines 205-264)

1. Collect seed keys from all `IDocumentSeedKeyProvider` instances
2. Process in batches of `DocumentSeedBatchSize`
3. Check if key already in cache via `ExistsAsync()`
4. Batch fetch uncached keys from database
5. Set in cache with seed entry options

---

## 5. Cache Invalidation

### CacheRefreshingNotificationHandler (120 lines)

Handles 8 notification types:

| Notification | Action |
|--------------|--------|
| `ContentRefreshNotification` | `RefreshContentAsync()` |
| `ContentDeletedNotification` | `DeleteItemAsync()` for each |
| `MediaRefreshNotification` | `RefreshMediaAsync()` |
| `MediaDeletedNotification` | `DeleteItemAsync()` for each |
| `ContentTypeRefreshedNotification` | Clear type cache, `Rebuild()` |
| `ContentTypeDeletedNotification` | Clear type cache |
| `MediaTypeRefreshedNotification` | Clear type cache, `Rebuild()` |
| `MediaTypeDeletedNotification` | Clear type cache |

### RefreshContentAsync (DocumentCacheService.cs lines 300-324)

1. Creates draft cache node from `IContent`
2. Persists to `cmsContentNu` table
3. If publishing: creates published node and persists
4. If unpublishing: clears published cache entry

### Cache Tags (line 331)

All content uses tag `Constants.Cache.Tags.Content` for bulk invalidation:
```csharp
await _hybridCache.RemoveByTagAsync(Constants.Cache.Tags.Content, cancellationToken);
```

---

## 6. Database Persistence

### DatabaseCacheRepository (891 lines)

NPoco-based repository for `cmsContentNu` table.

**Key Methods**:
- `GetContentSourceAsync(Guid key, bool preview)` - Single content fetch
- `GetContentSourcesAsync(IEnumerable<Guid> keys)` - Batch fetch
- `RefreshContentAsync(ContentCacheNode, PublishedState)` - Insert/update cache
- `Rebuild()` - Full cache rebuild by content type

**SQL Templates** (lines 575-748):
Uses `SqlContext.Templates` for cached SQL generation with optimized joins across:
- `umbracoNode`, `umbracoContent`, `cmsDocument`, `umbracoContentVersion`
- `cmsDocumentVersion`, `cmsContentNu`

### Serialization Options (NuCacheSettings.NuCacheSerializerType)

| Type | Serializer |
|------|------------|
| `JSON` | JsonContentNestedDataSerializer |
| `MessagePack` | MsgPackContentNestedDataSerializer |

---

## 7. Project-Specific Notes

### Experimental API Warning

HybridCache API is experimental (suppressed with `#pragma warning disable EXTEXP0018`).

### Draft vs Published Caching

- **Draft cache key**: `"{key}+draft"`
- **Published cache key**: `"{key}"`
- Each stored separately to avoid cross-contamination
- Draft entries never use seed durations

### Published Ancestor Path Check (DocumentCacheService.cs lines 131-135)

Before returning cached content, verifies ancestor path is published via `_publishStatusQueryService.HasPublishedAncestorPath()`. Returns null if parent unpublished.

### In-Memory Content Cache (the L0 converted-content cache)

The converted `IPublishedContent` objects are cached behind `IConvertedPublishedContentCache<TKey>`
(`Services/IConvertedPublishedContentCache.cs`), used by `DocumentCacheService` (`<string>`) and
`MediaCacheService` (`<Guid>`), since `ContentCacheNode` → `IPublishedContent` conversion is expensive.
This is the single insert/remove/clear path for the L0 cache, and each implementation tracks both the entry
count and an approximate retained byte total. The implementation is chosen per service by
`IConvertedPublishedContentCacheFactory` (injected into both services) from the configured maximum.

The cache is **unbounded by default** — `UnboundedConvertedPublishedContentCache<TKey>`, a plain
`ConcurrentDictionary` only evicted on content change / explicit clear, so walking the whole published tree
(Delivery API crawl, sitemap, warm-up) retains the whole tree's converted form.

A **bounded, scan-resistant** mode is available as an **opt-in package**,
`Umbraco.Cms.PublishedCache.HybridCache.Bounded` (`src/Umbraco.PublishedCache.HybridCache.Bounded`). It is
**not** part of the default install — that keeps its `BitFaster.Caching` dependency (the W-TinyLFU
`ConcurrentLfu` backing) out of every site's dependency graph. Installing the package registers
`IBoundedConvertedPublishedContentCacheFactory` (via an auto-discovered `IComposer`); then setting
`CacheSettings.Entry.Document.MaximumLocalCacheItems` / `...Media.MaximumLocalCacheItems` makes the
corresponding L0 cache bounded, so frequently requested content is retained while rarely accessed content is
evicted and a one-off full-tree walk cannot grow it without bound. If a maximum is configured but the
package is absent, the default factory logs a `Warning` and falls back to unbounded (it never fails to boot).
Default `null` preserves the unbounded behaviour; the observability below quantifies it either way.

### Memory observability

The in-memory structures whose footprint scales with the size of the content tree implement
`IMemoryCacheSizeReporter` (`Umbraco.Cms.Core.Cache`), exposing an approximate retained **entry count** and
(where cheaply derivable) an approximate **byte** estimate:

| Reporter (`CacheName`) | Structure | Byte estimate |
|------------------------|-----------|---------------|
| `Published content (converted, L0)` | `DocumentCacheService` L0 cache | running total of per-entry node-size estimates |
| `Published media (converted, L0)` | `MediaCacheService` L0 cache | running total of per-entry node-size estimates |
| `Document URL segments` | `DocumentUrlService._documentUrlCache` (≈ documents × cultures × draft/published) | sampled structural estimate |
| `Document navigation` / `Media navigation` | the in-memory navigation trees (active + recycle bin) | sampled structural estimate |

`MemoryCacheSizeReportingJob` (a recurring job, all server roles, 1-minute period) logs each count and byte
estimate plus `GC.GetTotalMemory` and `Environment.WorkingSet` **at `Debug` level** — enable `Debug` for
`Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.MemoryCacheSizeReportingJob` to capture, e.g. during a
reindex or crawl. Counts/bytes are a **trend/attribution** signal (a value that climbs and never falls
indicates unbounded retention). The byte figures are coarse approximations, **not** a heap measurement: the
L0 estimate is an *underlying-content lower bound* (`ContentCacheNodeSizeEstimator` sums the source node's
stored content without decompressing or walking the converted graph, so it omits the property-editor-driven
conversion blow-up); true per-object bytes come from a GC dump. Note the tiers: **L0** is the
converted-`IPublishedContent` cache reported above; **L1** is Microsoft HybridCache's in-process tier of
`ContentCacheNode` entries (behind L0); **L2** is the optional distributed tier. The HybridCache **L1** has
no exposed count/size — measure it from the GC dump until a sized backing cache is wired up (PR 3).

### Known Technical Debt

1. **Warnings Disabled** (`.csproj:10-12`): `TreatWarningsAsErrors=false`
2. **Property Value Null Handling** (`PropertyData.cs:25, 37`): Cannot be null, needs fallback decision
3. **Routing Cache TODO** (`ContentCacheDataModel.cs:26`): Remove when routing cache implemented
4. **SQL Template Limitations** (`DatabaseCacheRepository.cs:612, 688, 737`): Can't use templates with certain joins
5. **Serializer Refactor** (`DatabaseCacheRepository.cs:481`): Standardize to ContentTypeId only
6. **String Interning** (`MsgPackContentNestedDataSerializer.cs:29`): Intern alias strings during deserialization
7. **V18 Interface Cleanup** (`IDatabaseCacheRepository.cs:24, 48`): Remove default implementations

### Cache Settings (CacheSettings)

Key configuration options:
- `DocumentBreadthFirstSeedCount` - Number of documents to seed
- `MediaBreadthFirstSeedCount` - Number of media items to seed
- `DocumentSeedBatchSize` - Batch size for seeding
- `Entry.Document.SeedCacheDuration` - Duration for seeded entries
- `Entry.Document.LocalCacheDuration` - L1 cache duration
- `Entry.Document.RemoteCacheDuration` - L2 cache duration

### InternalsVisibleTo

Test projects with access to internal types:
- `Umbraco.Tests`
- `Umbraco.Tests.Common`
- `Umbraco.Tests.UnitTests`
- `Umbraco.Tests.Integration`
- `DynamicProxyGenAssembly2` (for mocking)

---

## Quick Reference

### Cache Flow

```
GetByIdAsync(id) → GetByKeyAsync(key) → GetNodeAsync(key, preview)
    → HybridCache.GetOrCreateAsync()
        → DatabaseCacheRepository.GetContentSourceAsync() [on miss]
    → PublishedContentFactory.ToIPublishedContent()
    → CreateModel(IPublishedModelFactory)
```

### Key Interfaces

| Interface | Implementation | Purpose |
|-----------|----------------|---------|
| `IPublishedContentCache` | DocumentCache | Content retrieval |
| `IPublishedMediaCache` | MediaCache | Media retrieval |
| `IDocumentCacheService` | DocumentCacheService | Cache operations |
| `IDatabaseCacheRepository` | DatabaseCacheRepository | Database access |
| `ICacheNodeFactory` | CacheNodeFactory | Cache node creation |

### Configuration Keys

```json
{
  "Umbraco": {
    "CMS": {
      "Cache": {
        "DocumentBreadthFirstSeedCount": 100,
        "MediaBreadthFirstSeedCount": 50,
        "DocumentSeedBatchSize": 100,
        "Entry": {
          "Document": {
            "SeedCacheDuration": "04:00:00",
            "LocalCacheDuration": "00:05:00",
            "RemoteCacheDuration": "01:00:00"
          }
        }
      }
    }
  }
}
```

### Related Projects

| Project | Relationship |
|---------|--------------|
| `Umbraco.Core` | Interfaces (IPublishedContentCache, etc.) |
| `Umbraco.Infrastructure` | Services, repositories, NPoco |
| Microsoft.Extensions.Caching.Hybrid | HybridCache implementation |
