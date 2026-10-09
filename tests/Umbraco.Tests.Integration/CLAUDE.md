# Umbraco.Tests.Integration

Integration tests that boot a real Umbraco container (`UmbracoBuilder`) against a real database
(SQLite in-memory by default, LocalDb/SQL Server optionally — see the root `CLAUDE.md` →
"Integration Test Database Configuration"). Most fixtures derive from `UmbracoIntegrationTest`.

## Testing caching and cache refreshers

`UmbracoIntegrationTest` is wired for isolation and speed, **not** cache fidelity. Three harness
defaults will silently make a cache-related test pass **regardless of the code under test** (a false
green — it passes with and without the fix). When the behaviour under test involves repository
caching, cache invalidation, or a `*DistributedCacheNotificationHandler`, override them in
`CustomTestSetup(IUmbracoBuilder builder)`:

1. **`AppCaches.NoCache` is registered**, so repositories never actually cache and a stale-cache
   scenario cannot be reproduced. Register a real cache (pattern: `Umbraco.Core/Cache/RuntimeCacheTests`):
   ```csharp
   builder.Services.AddUnique(_ => new AppCaches(
       new DeepCloneAppCache(new ObjectCacheAppCache()),
       NoAppCache.Instance,
       new IsolatedCaches(_ => new DeepCloneAppCache(new ObjectCacheAppCache()))));
   ```

2. **A no-op server messenger is registered** (`NoopServerMessenger`, in
   `DependencyInjection/UmbracoBuilderExtensions.cs`), so `DistributedCache.Refresh*/RefreshAll`
   never actually runs the cache refreshers. Register a synchronous local messenger so refreshers
   execute in-process:
   ```csharp
   builder.Services.AddUnique<IServerMessenger, ContentEventsTests.LocalServerMessenger>();
   ```
   `LocalServerMessenger` (in `Umbraco.Infrastructure/Services/ContentEventsTests.cs`) uses
   `distributedEnabled: false`, so `ServerMessengerBase` delivers locally and invokes the refresher
   synchronously.

3. **The `*DistributedCacheNotificationHandler` set is NOT auto-registered** by the harness — these
   are wired in `UmbracoBuilder.CoreServices` for the real app only. To test that a notification
   invalidates a cache, register the handler under test explicitly (pattern:
   `Umbraco.Core/Services/ContentTypeEditingServiceTests`):
   ```csharp
   builder.AddNotificationHandler<LanguageDeletedNotification, LanguageDeletedDistributedCacheNotificationHandler>();
   ```

Always confirm the test fails before the fix and passes after (root `CLAUDE.md` → "Tests for a bug
fix must fail before the fix"). With the defaults above left in place, a cache test cannot fail, so
it proves nothing.
