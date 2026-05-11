# Memory Leak Analysis — Umbraco CMS v17

**Date**: 2026-03-03
**Branch**: `main`
**Scope**: All production projects under `src/`
**Methodology**: Static analysis — grep-based pattern matching across ~1,000 C# source files

---

## Executive Summary

Seven potential memory management issues were identified. None represent an unbounded memory growth path that would cause noticeable degradation or an `OutOfMemoryException` on a typical site running for days or weeks. The most accurate characterisation of the meaningful findings is **reduced `ArrayPool` efficiency** rather than classical memory leaks — the GC reclaims all affected memory eventually, but pooled buffers are not returned promptly.

The single highest-value fix is a one-line addition to `DatabaseServerMessenger.Dispose()`. Two findings around `JsonDocument` disposal are worth addressing for correctness, particularly on multi-server deployments. The remaining findings have negligible practical impact.

---

## Findings

### Finding 1 — `CancellationTokenSource` Not Disposed

| | |
|---|---|
| **File** | `src/Umbraco.Infrastructure/Sync/DatabaseServerMessenger.cs` |
| **Lines** | 24 (creation), 339–349 (Dispose) |
| **Confidence** | High |
| **Practical Impact** | Negligible |

`DatabaseServerMessenger` implements `IDisposable`, but its `Dispose(bool)` method omits disposal of `_cancellationTokenSource`:

```csharp
// Line 24 — created
private readonly CancellationTokenSource _cancellationTokenSource = new();

// Lines 339–349 — _syncIdle is disposed; _cancellationTokenSource is not
protected virtual void Dispose(bool disposing)
{
    if (!_disposedValue)
    {
        if (disposing)
        {
            _syncIdle.Dispose();
            // ← _cancellationTokenSource.Dispose() is missing
        }
        _disposedValue = true;
    }
}
```

`CancellationTokenSource` internally holds a native `SafeWaitHandle` (a Win32 event object) that should be released via `Dispose()`. Because this class is a singleton, exactly **one** handle is leaked for the lifetime of the process — the GC finaliser will never reclaim it. The practical memory cost is a few hundred bytes and one OS handle, which is immeasurable in a normal server process.

**Real-world impact over several days**: None observable. This is a correctness issue rather than a practical one.

**Recommended fix**: Add `_cancellationTokenSource.Dispose();` inside the `if (disposing)` block at line 345. This is a single-line change.

---

### Finding 2 — `JsonDocument` Not Disposed in Cache Sync Loop

| | |
|---|---|
| **File** | `src/Umbraco.Infrastructure/Services/CacheInstructionService.cs` |
| **Lines** | 287, 293, 315–334 |
| **Confidence** | High |
| **Practical Impact** | Low (single server) / Low–Medium (multi-server) |

`TryDeserializeInstructions` allocates a `JsonDocument` — which rents a buffer from `ArrayPool<byte>` — and returns it via an `out` parameter. The caller uses the document's `RootElement` once, then allows the variable to go out of scope without calling `Dispose()`:

```csharp
// Line 287 — JsonDocument created inside TryDeserializeInstructions
if (TryDeserializeInstructions(instruction, out JsonDocument? jsonInstructions) is false
    && jsonInstructions is null)
{
    lastId = instruction.Id;
    continue;
}

// Line 293 — last use; jsonInstructions goes out of scope without Dispose()
List<RefreshInstruction> instructionBatch = GetAllInstructions(jsonInstructions?.RootElement);
```

`JsonDocument` has no finaliser. When the GC collects an un-disposed instance, the rented `ArrayPool` buffer is collected as ordinary heap memory rather than being returned to the pool. This reduces pool hit rates and increases allocation pressure.

This codepath runs inside the multi-server cache instruction sync loop. On a **single-server** deployment the loop processes only local (skipped) instructions and almost never reaches `TryDeserializeInstructions`. On a **multi-server load-balanced** deployment with active content publishing, this can fire many times per minute.

**Real-world impact over several days**: Negligible on single-server. On a busy multi-server site, slightly elevated Gen 0 GC frequency from reduced `ArrayPool` reuse. Memory does not grow unboundedly.

**Recommended fix**: Wrap the `JsonDocument` in a `using` declaration at the call site:
```csharp
using JsonDocument? jsonInstructions = TryDeserializeInstructions(instruction);
if (jsonInstructions is null) { lastId = instruction.Id; continue; }
```

---

### Finding 3 — `JsonDocument` Cached Without Disposal on Eviction

| | |
|---|---|
| **File** | `src/Umbraco.Infrastructure/PropertyEditors/ValueConverters/JsonValueConverter.cs` |
| **Lines** | 52–68 |
| **Confidence** | Medium |
| **Practical Impact** | Low |

`ConvertSourceToIntermediate` returns a `JsonDocument` that the published content cache stores at `PropertyCacheLevel.Element` (cached per content element, per variant):

```csharp
public override PropertyCacheLevel GetPropertyCacheLevel(IPublishedPropertyType propertyType)
    => PropertyCacheLevel.Element;

public override object? ConvertSourceToIntermediate(...)
{
    // ...
    return JsonDocument.Parse(sourceString); // rented ArrayPool buffer not returned on eviction
}
```

The cache holds values as `object?` and evicts them by releasing references. Because there is no eviction callback that calls `Dispose()`, the rented buffer for each `JsonDocument` is abandoned rather than returned to the pool.

This affects every content node with a JSON property type (block lists, media pickers, nested content, etc.). On a site with mostly-static content the cached `JsonDocument` population is bounded and stable. On a site with frequent content changes causing cache churn, pool hit rates are lower and allocation pressure is higher.

**Real-world impact over several days**: Low. Memory does not grow unboundedly — the GC collects evicted documents. The observable effect, if any, would be marginally higher Gen 0 collection frequency on high-churn sites. This is unlikely to be measurable on a typical site.

**Recommended fix**: This requires a non-trivial design change — either wrapping returned values in a disposable owner type with cache eviction callbacks, or switching the internal representation away from the pooled `JsonDocument` type.

---

### Finding 4 — `CryptoStream` and `ICryptoTransform` Not Disposed

| | |
|---|---|
| **File** | `src/Umbraco.Infrastructure/Security/MemberPasswordHasher.cs` |
| **Lines** | 161–171 |
| **Confidence** | Medium |
| **Practical Impact** | Negligible |

In a legacy password decryption helper, `MemoryStream` is correctly wrapped in `using`, but `CryptoStream` and `ICryptoTransform` are not:

```csharp
private static string DecryptLegacyPassword(string encryptedPassword, SymmetricAlgorithm algorithm)
{
    using var memoryStream = new MemoryStream();
    ICryptoTransform cryptoTransform = algorithm.CreateDecryptor(); // not disposed
    var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write); // not disposed
    var buf = Convert.FromBase64String(encryptedPassword);
    cryptoStream.Write(buf, 0, 32);
    cryptoStream.FlushFinalBlock();
    return Encoding.Unicode.GetString(memoryStream.ToArray());
}
```

Both types implement `IDisposable` and hold internal transform state buffers. However, this method is only invoked for accounts with Umbraco ≤ 8 encrypted password hashes — a codepath that is exercised only during migrations from legacy installations and is effectively never called on a v17 site.

**Real-world impact over several days**: None observable. The objects are small and collected promptly by the GC.

**Recommended fix**: Add `using` declarations for both `cryptoTransform` and `cryptoStream` for correctness.

---

### Finding 5 — Static Event Subscription Without Unsubscription (Development Mode Only)

| | |
|---|---|
| **File** | `src/Umbraco.Cms.DevelopmentMode.Backoffice/InMemoryAuto/InMemoryAssemblyLoadContextManager.cs` |
| **Lines** | 10–11 |
| **Confidence** | High (pattern) |
| **Practical Impact** | None in production |

The class subscribes to a static event in its constructor but implements no `IDisposable` to unsubscribe:

```csharp
public InMemoryAssemblyLoadContextManager() =>
    AssemblyLoadContext.Default.Resolving += OnResolvingDefaultAssemblyLoadContext;
// No corresponding -= and no IDisposable
```

The class is registered as a singleton (`AddSingleton<InMemoryAssemblyLoadContextManager>()`), so its lifetime matches the process and the omission is benign in normal operation. The static event would prevent GC if the DI container released its reference (e.g. during repeated host rebuilding in integration tests). This component is only active when `ModelsMode` is `InMemoryAuto` and `RuntimeMode` is `BackofficeDevelopment` — it is never loaded in production.

**Real-world impact over several days**: None in production. Negligible in development.

**Recommended fix**: Implement `IDisposable` and unsubscribe in `Dispose()` for correctness and test isolation.

---

### Finding 6 — Static `HttpClient` Bypasses `IHttpClientFactory`

| | |
|---|---|
| **File** | `src/Umbraco.Core/Media/EmbedProviders/OEmbedProviderBase.cs` |
| **Lines** | 13, 88–92 |
| **Confidence** | Low (not a true memory leak) |
| **Practical Impact** | Negligible (memory); Low (DNS staleness) |

A static `HttpClient?` field is lazily initialised without using `IHttpClientFactory`:

```csharp
private static HttpClient? _httpClient;

if (_httpClient == null)
{
    _httpClient = new HttpClient();
    _httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd(...);
}
```

`HttpClient` is designed to be long-lived and reused, so the static pattern does not cause a memory leak. The practical concern is that DNS changes are not respected (no `PooledConnectionLifetime` on the underlying handler), which could cause stale connections on sites where OEmbed providers change their infrastructure. This is not a memory concern.

**Real-world impact over several days**: No memory impact. Potential for stale DNS on OEmbed requests after several days if a provider changes their IP.

**Recommended fix**: Inject `IHttpClientFactory` and use a named or typed client.

---

### Finding 7 — Unbounded Static Regex Cache

| | |
|---|---|
| **File** | `src/Umbraco.Core/Services/OEmbedService.cs` |
| **Lines** | 15, 68–69 |
| **Confidence** | Low |
| **Practical Impact** | Negligible |

Compiled `Regex` objects are cached in a static `ConcurrentDictionary` with no eviction:

```csharp
private static readonly ConcurrentDictionary<string, Regex> RegexCache = new();

private static Regex GetOrCreateRegex(string pattern)
    => RegexCache.GetOrAdd(pattern, p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled));
```

The dictionary is bounded by the number of unique URL scheme patterns across registered OEmbed providers, which is typically around 15–20 entries. Compiled `Regex` objects are intentionally long-lived. This is not a memory leak under normal usage; it would only become one if patterns were generated dynamically from user input at runtime (which they are not).

**Real-world impact over several days**: None observable.

**Recommended fix**: No action needed under current usage patterns. Add a size cap if the pattern set ever becomes dynamic.

---

## Items Investigated and Cleared

The following patterns were examined and found to be correctly implemented:

| Class / Area | Pattern Checked | Result |
|---|---|---|
| `DatabaseServerMessenger._syncIdle` | `ManualResetEvent` disposal | ✓ Disposed at line 345 |
| `RecurringHostedServiceBase._timer` | `System.Threading.Timer` disposal | ✓ Disposed via `_timer?.Dispose()` |
| `DistributedBackgroundJobHostedService` | `PeriodicTimer` disposal | ✓ Wrapped in `using` |
| `RetryDbConnection` | `StateChange` event handler | ✓ Unsubscribed in `Dispose(bool)` |
| `UmbracoIdentityUser` | `ObservableCollection.CollectionChanged` | ✓ Cleaned up in property setters |
| `Content` / `ContentBase` / `ContentTypeBase` | `CollectionChanged` handlers | ✓ Use `ClearCollectionChangedEvents()` before reassignment |
| `FileRepository` / `PartialViewRepository` | `MemoryStream` returned from `GetContentStream` | ✓ All call sites wrap result in `using` |
| `JsonConfigManipulator` | `FileStream` disposal | ✓ Wrapped in `await using` |
| `QueuedHostedService` | `ExecutionContext.SuppressFlow()` | ✓ Wrapped in `using` |
| Background job DI registrations | Captive dependency (scoped-in-singleton) | ✓ No violations found |

---

## Priority and Effort Summary

| Priority | Finding | Fix Effort |
|---|---|---|
| **Fix** | Finding 1: `CancellationTokenSource` not disposed | 1 line |
| **Fix** | Finding 2: `JsonDocument` not disposed in sync loop | ~3 lines |
| **Fix** | Finding 4: `CryptoStream` not disposed | 2 lines |
| **Fix** | Finding 5: Static event leak (dev-only) | `IDisposable` implementation |
| **Consider** | Finding 3: `JsonDocument` cached without disposal | Design change required |
| **Consider** | Finding 6: Static `HttpClient` | Inject `IHttpClientFactory` |
| **Monitor** | Finding 7: Static `Regex` cache | No action unless patterns become dynamic |

Findings 1, 2, and 4 are low-effort correctness fixes that follow established .NET resource management idioms. Finding 3 is a legitimate design smell that warrants a separate investigation into how the published content cache handles disposable cached values.
