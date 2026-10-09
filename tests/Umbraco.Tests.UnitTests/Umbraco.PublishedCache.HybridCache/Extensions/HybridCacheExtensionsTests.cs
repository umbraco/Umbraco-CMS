using Microsoft.Extensions.Caching.Hybrid;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache.Extensions;

/// <summary>
/// Provides tests to cover the <see cref="HybridCacheExtensions"/> class.
/// </summary>
/// <remarks>
/// Hat-tip: https://github.com/dotnet/aspnetcore/discussions/57191.
/// </remarks>
[TestFixture]
public class HybridCacheExtensionsTests
{
    private Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> _cacheMock;

    [SetUp]
    public void TestInitialize() => _cacheMock = new Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache>();

    [Test]
    public async Task ExistsAsync_WhenKeyExists_ShouldReturnTrue()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = new ContentCacheNode { Id = 1234 };

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<ContentCacheNode?>>, CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var exists = await HybridCacheExtensions.ExistsAsync<ContentCacheNode?>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsTrue(exists);
    }

    [Test]
    public async Task ExistsAsync_WhenKeyDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        string key = "test-key";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<ContentCacheNode?>>, CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .Returns((
                string key,
                Func<CancellationToken, ValueTask<ContentCacheNode?>> state,
                Func<Func<CancellationToken, ValueTask<ContentCacheNode?>>, CancellationToken, ValueTask<ContentCacheNode?>> factory,
                HybridCacheEntryOptions? options,
                IEnumerable<string>? tags,
                CancellationToken token) =>
            {
                return factory(state, token);
            });

        // Act
        var exists = await HybridCacheExtensions.ExistsAsync<ContentCacheNode?>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsFalse(exists);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExists_ShouldReturnTrueAndValueAsString()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = "test-value";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<string>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<string>>, CancellationToken, ValueTask<string>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<string>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsTrue(exists);
        Assert.AreEqual(expectedValue, value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExists_ShouldReturnTrueAndValueAsInteger()
    {
        // Arrange
        string key = "test-key";
        var expectedValue = 5;

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<int>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<int>>, CancellationToken, ValueTask<int>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .ReturnsAsync(expectedValue);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<int>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsTrue(exists);
        Assert.AreEqual(expectedValue, value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyExistsButValueIsNull_ShouldReturnTrueAndNullValue()
    {
        // Arrange
        string key = "test-key";

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<object>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<object>>, CancellationToken, ValueTask<object>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .ReturnsAsync(null!);

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<int?>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsTrue(exists);
        Assert.IsNull(value);
    }

    [Test]
    public async Task TryGetValueAsync_WhenKeyDoesNotExist_ShouldReturnFalseAndNull()
    {
        // Arrange
        string key = "test-key";

        _cacheMock.Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<object>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<object>>, CancellationToken, ValueTask<object>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .Returns((
                string key,
                Func<CancellationToken, ValueTask<object>> state,
                Func<Func<CancellationToken, ValueTask<object>>, CancellationToken, ValueTask<object>> factory,
                HybridCacheEntryOptions? options,
                IEnumerable<string>? tags,
                CancellationToken token) =>
            {
                return factory(state, token);
            });

        // Act
        var (exists, value) = await HybridCacheExtensions.TryGetValueAsync<object>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsFalse(exists);
        Assert.IsNull(value);
    }

    /// <summary>
    /// Verifies that the existence probe excludes the distributed (L2) cache from the temporary entry it creates on a miss.
    /// </summary>
    /// <remarks>
    /// HybridCache has no "try get" operation, so the probe calls <c>GetOrCreateAsync</c> with a factory that records the
    /// miss and returns null. HybridCache stores whatever that factory returns, which is why a miss leaves a temporary
    /// null entry that the probe then has to remove.
    /// HybridCache writes a factory result to L2 in the background, after <c>GetOrCreateAsync</c> has returned to the caller.
    /// If the probe's temporary null entry were written to L2, that write could complete after the probe's own
    /// <c>RemoveAsync</c>. Other servers, or this server once its local entry expired, would then read a cached null
    /// for a key that exists, and skip the database. That would make published content resolve to nothing (see
    /// https://github.com/umbraco/Umbraco-CMS/issues/23405 and https://github.com/umbraco/Umbraco-CMS/issues/24066).
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Test]
    public async Task TryGetValueAsync_ShouldNotWriteProbeEntryToDistributedCache()
    {
        // Arrange
        string key = "test-key";
        HybridCacheEntryOptions? capturedOptions = null;

        _cacheMock
            .Setup(cache => cache.GetOrCreateAsync(
                key,
                It.IsAny<Func<CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<Func<Func<CancellationToken, ValueTask<ContentCacheNode?>>, CancellationToken, ValueTask<ContentCacheNode?>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                null,
                CancellationToken.None))
            .Callback((
                string key,
                Func<CancellationToken, ValueTask<ContentCacheNode?>> state,
                Func<Func<CancellationToken, ValueTask<ContentCacheNode?>>, CancellationToken, ValueTask<ContentCacheNode?>> factory,
                HybridCacheEntryOptions? options,
                IEnumerable<string>? tags,
                CancellationToken token) => capturedOptions = options)
            .ReturnsAsync((ContentCacheNode?)null);

        // Act
        await HybridCacheExtensions.TryGetValueAsync<ContentCacheNode?>(_cacheMock.Object, key, CancellationToken.None);

        // Assert
        Assert.IsNotNull(capturedOptions);
        Assert.IsTrue(capturedOptions!.Flags?.HasFlag(HybridCacheEntryFlags.DisableDistributedCacheWrite));
    }

    /// <summary>
    /// Verifies that concurrent existence probes for the same key are serialized, including a probe that arrives
    /// while another caller is still waiting on the key's lock.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The per-key lock was previously removed when its holder released it, even if another caller was still waiting.
    /// A third caller then created a second lock for the same key, so two probes could be inside
    /// <c>GetOrCreateAsync</c> at once. HybridCache joins concurrent calls for a key into a single factory invocation,
    /// so the joining probe's own factory never ran. It reported the key as present with a null value, which callers
    /// treat as a cached "resolves to nothing", so they skipped the database read.
    /// </para>
    /// <para>
    /// Under concurrent cold-start traffic this dropped published children from <c>Children()</c> and left media pickers
    /// empty (see https://github.com/umbraco/Umbraco-CMS/issues/24066).
    /// </para>
    /// <para>
    /// The test drives three probes through an always-missing cache that holds each call open, so the interleaving is
    /// deterministic: A holds the lock, B waits, A completes, and C arrives while B is in flight.
    /// </para>
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Test]
    public async Task TryGetValueAsync_WhenProbedConcurrently_ShouldNeverOverlapProbesForTheSameKey()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var cache = new GatedHybridCache();

        // Act
        // - A enters the cache while B queues behind it on the key lock.
        Task<(bool Exists, ContentCacheNode? Value)> probeA = cache.TryGetValueAsync<ContentCacheNode?>(key, CancellationToken.None);
        await cache.WaitForEntryAsync();
        Task<(bool Exists, ContentCacheNode? Value)> probeB = cache.TryGetValueAsync<ContentCacheNode?>(key, CancellationToken.None);

        // - Once A completes, B takes over the lock. C arrives while B is still in the cache, so must queue behind it.
        cache.ReleaseNext();
        await cache.WaitForEntryAsync();
        await probeA;
        Task<(bool Exists, ContentCacheNode? Value)> probeC = cache.TryGetValueAsync<ContentCacheNode?>(key, CancellationToken.None);
        var maxInFlightWhileBHeldTheLock = cache.MaxInFlight;

        cache.ReleaseNext();
        await cache.WaitForEntryAsync();
        cache.ReleaseNext();
        (bool Exists, ContentCacheNode? Value)[] results = await Task.WhenAll(probeA, probeB, probeC);

        // Assert
        Assert.AreEqual(1, maxInFlightWhileBHeldTheLock);
        Assert.AreEqual(1, cache.MaxInFlight);
        Assert.IsTrue(results.All(result => result.Exists is false));
        Assert.IsFalse(HybridCacheExtensions.HasKeyLock(key));
    }

    /// <summary>
    /// Verifies that a probe cancelled while waiting on the key's lock gives up its reference to that lock. The lock
    /// should be discarded once the holder completes, and later probes should proceed unblocked.
    /// </summary>
    /// <remarks>
    /// A lock is only removed once no caller holds or waits on it. A cancelled wait that kept its reference would leave
    /// the lock in place for the lifetime of the process, so one lock would leak for each such key.
    /// </remarks>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    [Test]
    public async Task TryGetValueAsync_WhenCancelledWhileWaitingForLock_ShouldReleaseTheLock()
    {
        // Arrange
        string key = Guid.NewGuid().ToString();
        var cache = new GatedHybridCache();
        using var cancellationTokenSource = new CancellationTokenSource();

        Task<(bool Exists, ContentCacheNode? Value)> holdingProbe = cache.TryGetValueAsync<ContentCacheNode?>(key, CancellationToken.None);
        await cache.WaitForEntryAsync();
        Task<(bool Exists, ContentCacheNode? Value)> cancelledProbe = cache.TryGetValueAsync<ContentCacheNode?>(key, cancellationTokenSource.Token);

        // Act
        cancellationTokenSource.Cancel();

        // Assert
        Assert.ThrowsAsync<OperationCanceledException>(async () => await cancelledProbe);

        cache.ReleaseNext();
        await holdingProbe;
        Assert.IsFalse(HybridCacheExtensions.HasKeyLock(key));

        Task<(bool Exists, ContentCacheNode? Value)> laterProbe = cache.TryGetValueAsync<ContentCacheNode?>(key, CancellationToken.None);
        await cache.WaitForEntryAsync();
        cache.ReleaseNext();
        Assert.IsFalse((await laterProbe).Exists);
        Assert.AreEqual(1, cache.MaxInFlight);
    }

    /// <summary>
    /// A cache that always misses, holding each <see cref="GetOrCreateAsync{TState, T}"/> call until the test releases it,
    /// and recording how many calls were in flight at once.
    /// </summary>
    private sealed class GatedHybridCache : Microsoft.Extensions.Caching.Hybrid.HybridCache
    {
        private static readonly TimeSpan _timeout = TimeSpan.FromSeconds(10);

        private readonly object _sync = new();
        private readonly Queue<TaskCompletionSource> _gates = new();
        private readonly SemaphoreSlim _entered = new(0);
        private int _inFlight;

        public int MaxInFlight { get; private set; }

        public async Task WaitForEntryAsync()
        {
            if (await _entered.WaitAsync(_timeout) is false)
            {
                Assert.Fail("Timed out waiting for a probe to enter the cache.");
            }
        }

        public void ReleaseNext()
        {
            lock (_sync)
            {
                _gates.Dequeue().SetResult();
            }
        }

        public override async ValueTask<T> GetOrCreateAsync<TState, T>(
            string key,
            TState state,
            Func<TState, CancellationToken, ValueTask<T>> factory,
            HybridCacheEntryOptions? options = null,
            IEnumerable<string>? tags = null,
            CancellationToken cancellationToken = default)
        {
            var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (_sync)
            {
                _inFlight++;
                MaxInFlight = Math.Max(MaxInFlight, _inFlight);
                _gates.Enqueue(gate);
            }

            _entered.Release();
            try
            {
                await gate.Task.WaitAsync(_timeout, cancellationToken);
            }
            finally
            {
                lock (_sync)
                {
                    _inFlight--;
                }
            }

            return await factory(state, cancellationToken);
        }

        public override ValueTask SetAsync<T>(string key, T value, HybridCacheEntryOptions? options = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public override ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;

        public override ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
            => ValueTask.CompletedTask;
    }
}
