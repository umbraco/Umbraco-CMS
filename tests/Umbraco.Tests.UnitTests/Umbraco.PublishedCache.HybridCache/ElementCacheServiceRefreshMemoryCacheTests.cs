using System.Data;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Preview;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The memory refresh runs under the element tree read lock, which blocks every save. The cache writes must be
/// issued while the lock is held (so the lock orders them against a later commit) but the wait for a distributed
/// write to complete must not keep the lock: that wait needs a thread-pool hop, and on a starved pool it
/// stretches the hold from milliseconds to seconds. Because the lock no longer covers the wait, refreshes and removals
/// of one element are serialised instead, so a distributed write that is sent late cannot land on top of a newer
/// one; refreshes of different elements stay independent.
/// </summary>
[TestFixture]
public class ElementCacheServiceRefreshMemoryCacheTests
{
    [Test]
    public void RefreshMemoryCache_ReleasesTheScope_BeforeWaitingForTheDistributedWrite()
    {
        Guid key = Guid.NewGuid();
        var distributedWrite = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();
        var writeIssuedBeforeScopeDisposed = false;

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() =>
        {
            writeIssuedBeforeScopeDisposed = !scopeDisposed.IsSet;
            return new ValueTask(distributedWrite.Task);
        });
        ElementCacheService service = CreateService(CreateRepository(key).Object, CreateScopeProvider(scopeDisposed), hybridCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        try
        {
            Assert.Multiple(() =>
            {
                Assert.That(scopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True, "The scope, and with it the read lock, must be released while the distributed write is still pending.");
                Assert.That(writeIssuedBeforeScopeDisposed, Is.True, "The cache write must be issued while the lock is held so the lock orders it against later commits.");
                Assert.That(refresh.IsCompleted, Is.False, "The refresh still waits for the distributed write after releasing the lock.");
            });
        }
        finally
        {
            distributedWrite.TrySetResult();
        }

        Assert.That(refresh.Wait(TimeSpan.FromSeconds(5)), Is.True);
    }

    [Test]
    public void RefreshMemoryCache_WaitsForThePendingDistributedWriteOfTheSameElement_BeforeIssuingItsOwn()
    {
        Guid key = Guid.NewGuid();
        var firstDistributedWrite = new TaskCompletionSource();
        var secondWriteIssued = new ManualResetEventSlim();
        var firstScopeDisposed = new ManualResetEventSlim();
        var writes = 0;

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() =>
        {
            if (Interlocked.Increment(ref writes) == 1)
            {
                return new ValueTask(firstDistributedWrite.Task);
            }

            secondWriteIssued.Set();
            return ValueTask.CompletedTask;
        });
        ElementCacheService service = CreateService(CreateRepository(key).Object, CreateScopeProvider(firstScopeDisposed), hybridCache.Object);

        Task firstRefresh = Task.Run(() => service.RefreshMemoryCache(key));
        Assert.That(firstScopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True);
        Task secondRefresh = Task.Run(() => service.RefreshMemoryCache(key));
        try
        {
            Assert.That(secondWriteIssued.Wait(TimeSpan.FromMilliseconds(300)), Is.False, "The second refresh must not issue its distributed write while the first one's is still pending.");
        }
        finally
        {
            firstDistributedWrite.TrySetResult();
        }

        Assert.Multiple(() =>
        {
            Assert.That(secondWriteIssued.Wait(TimeSpan.FromSeconds(5)), Is.True, "The second refresh issues its write once the first has completed.");
            Assert.That(Task.WaitAll([firstRefresh, secondRefresh], TimeSpan.FromSeconds(5)), Is.True);
        });
    }

    [Test]
    public void RefreshMemoryCache_DoesNotWaitForThePendingDistributedWriteOfAnotherElement()
    {
        Guid key = Guid.NewGuid();
        Guid otherKey = Guid.NewGuid();
        var pendingWriteForKey = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(node =>
            node.Key == key ? new ValueTask(pendingWriteForKey.Task) : ValueTask.CompletedTask);
        ElementCacheService service = CreateService(CreateRepository(key, otherKey).Object, CreateScopeProvider(scopeDisposed), hybridCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        try
        {
            Assert.That(scopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(Task.Run(() => service.RefreshMemoryCache(otherKey)).Wait(TimeSpan.FromSeconds(5)), Is.True, "A refresh of another element completes while this one's distributed write is pending.");
        }
        finally
        {
            pendingWriteForKey.TrySetResult();
        }

        Assert.That(refresh.Wait(TimeSpan.FromSeconds(5)), Is.True);
    }

    [Test]
    public void RefreshMemoryCache_WithAPublishedRow_SetsThePublishedEntry_AndDropsTheConvertedCopyBeforeTheWriteCompletes()
    {
        Guid key = Guid.NewGuid();
        var distributedWrite = new TaskCompletionSource();
        var convertedCopyDroppedWhileWritePending = false;

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() => new ValueTask(distributedWrite.Task));
        var convertedCache = new Mock<IConvertedPublishedContentCache<string, IPublishedElement>>();
        convertedCache.Setup(c => c.Remove(key.ToString())).Callback(() => convertedCopyDroppedWhileWritePending = !distributedWrite.Task.IsCompleted).Returns(true);
        var repository = new Mock<IDatabaseCacheRepository>();
        repository.Setup(r => r.GetElementSourceForPublishStates(key)).Returns((null, new ContentCacheNode { Id = 1, Key = key, ContentTypeId = 1 }));
        ElementCacheService service = CreateService(repository.Object, CreateScopeProvider(new ManualResetEventSlim()), hybridCache.Object, convertedCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        try
        {
            Assert.That(SpinWait.SpinUntil(() => convertedCopyDroppedWhileWritePending, TimeSpan.FromSeconds(5)), Is.True, "The converted copy is dropped as soon as the in-process entry has changed, not after the distributed write completes.");
        }
        finally
        {
            distributedWrite.TrySetResult();
        }

        Assert.That(refresh.Wait(TimeSpan.FromSeconds(5)), Is.True);
        hybridCache.Verify(c => c.SetAsync(key.ToString(), It.IsAny<ContentCacheNode>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        hybridCache.Verify(c => c.RemoveAsync($"{key}+draft", It.IsAny<CancellationToken>()), Times.Once, "No draft row, so the draft entry is removed.");
        hybridCache.Verify(c => c.RemoveAsync(key.ToString(), It.IsAny<CancellationToken>()), Times.Never);
        convertedCache.Verify(c => c.Remove(key.ToString()), Times.Once);
    }

    [Test]
    public void RefreshMemoryCache_WithoutAPublishedRow_RemovesThePublishedEntry_AndDropsTheConvertedCopy()
    {
        Guid key = Guid.NewGuid();
        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() => ValueTask.CompletedTask);
        var convertedCache = new Mock<IConvertedPublishedContentCache<string, IPublishedElement>>();
        var repository = new Mock<IDatabaseCacheRepository>();
        repository.Setup(r => r.GetElementSourceForPublishStates(key)).Returns((new ContentCacheNode { Id = 1, Key = key, ContentTypeId = 1, IsDraft = true }, null));
        ElementCacheService service = CreateService(repository.Object, CreateScopeProvider(new ManualResetEventSlim()), hybridCache.Object, convertedCache.Object);

        service.RefreshMemoryCache(key);

        hybridCache.Verify(c => c.SetAsync($"{key}+draft", It.IsAny<ContentCacheNode>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Once);
        hybridCache.Verify(c => c.SetAsync(key.ToString(), It.IsAny<ContentCacheNode>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
        hybridCache.Verify(c => c.RemoveAsync(key.ToString(), It.IsAny<CancellationToken>()), Times.Once);
        convertedCache.Verify(c => c.Remove(key.ToString()), Times.Once);
    }

    [Test]
    public void RefreshMemoryCache_ReadsTheDatabaseSynchronously()
    {
        Guid key = Guid.NewGuid();
        var repository = new Mock<IDatabaseCacheRepository>();
        repository.Setup(r => r.GetElementSourceForPublishStates(key)).Returns((null, null));
        ElementCacheService service = CreateService(repository.Object, CreateScopeProvider(new ManualResetEventSlim()), CreateHybridCache(() => ValueTask.CompletedTask).Object);

        service.RefreshMemoryCache(key);

        repository.Verify(r => r.GetElementSourceForPublishStates(key), Times.Once);
        repository.Verify(r => r.GetElementSourceForPublishStatesAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public void RefreshMemoryCache_WhenOneWriteFails_WaitsForTheOtherWrite_BeforeThrowing()
    {
        Guid key = Guid.NewGuid();
        var publishedWrite = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() => new ValueTask(publishedWrite.Task));
        hybridCache
            .Setup(c => c.RemoveAsync($"{key}+draft", It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromException(new InvalidOperationException("The draft removal failed.")));
        ElementCacheService service = CreateService(CreateRepository(key).Object, CreateScopeProvider(scopeDisposed), hybridCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        try
        {
            Assert.That(scopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(Task.WhenAny(refresh, Task.Delay(300)).Result, Is.Not.SameAs(refresh), "The refresh must keep waiting for the published write after the draft write has failed, or the next refresh of the element could overtake it.");
        }
        finally
        {
            publishedWrite.TrySetResult();
        }

        Assert.That(() => refresh.GetAwaiter().GetResult(), Throws.InvalidOperationException);
    }

    [Test]
    public void RemoveFromMemoryCacheAsync_WaitsForThePendingDistributedWriteOfTheSameElement_BeforeRemoving()
    {
        Guid key = Guid.NewGuid();
        var distributedWrite = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();
        var publishedEntryRemoved = new ManualResetEventSlim();

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() => new ValueTask(distributedWrite.Task));
        hybridCache
            .Setup(c => c.RemoveAsync(key.ToString(), It.IsAny<CancellationToken>()))
            .Callback(publishedEntryRemoved.Set)
            .Returns(ValueTask.CompletedTask);
        ElementCacheService service = CreateService(CreateRepository(key).Object, CreateScopeProvider(scopeDisposed), hybridCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        Assert.That(scopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True, "The refresh has issued its write and is waiting for it.");

        Task removal = service.RemoveFromMemoryCacheAsync(key);
        try
        {
            Assert.That(publishedEntryRemoved.Wait(TimeSpan.FromMilliseconds(300)), Is.False, "The removal must wait for the pending write of the same element, or that write could bring the entry back.");
        }
        finally
        {
            distributedWrite.TrySetResult();
        }

        Assert.Multiple(() =>
        {
            Assert.That(publishedEntryRemoved.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(removal.Wait(TimeSpan.FromSeconds(5)), Is.True);
            Assert.That(refresh.Wait(TimeSpan.FromSeconds(5)), Is.True);
        });
    }

    [Test]
    public void RefreshElement_OfTrashedElement_DoesNotWaitForThePendingDistributedWriteOfTheSameElement()
    {
        // RefreshElement runs inside the saving transaction, under the element tree write lock. A refresh of the same
        // element that is waiting for that lock holds the per-element lock, so waiting for it here would deadlock.
        Guid key = Guid.NewGuid();
        var distributedWrite = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();
        IElement trashed = CreateElement(key, trashed: true);

        Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> hybridCache = CreateHybridCache(() => new ValueTask(distributedWrite.Task));
        ElementCacheService service = CreateService(CreateRepository(key).Object, CreateScopeProvider(scopeDisposed), hybridCache.Object);

        Task refresh = Task.Run(() => service.RefreshMemoryCache(key));
        Assert.That(scopeDisposed.Wait(TimeSpan.FromSeconds(5)), Is.True, "The refresh has issued its write and is waiting for it.");

        try
        {
            Task removal = Task.Run(() => service.RefreshElement(trashed));
            Assert.That(removal.Wait(TimeSpan.FromSeconds(5)), Is.True, "The removal inside the transaction must not wait for the pending write.");
        }
        finally
        {
            distributedWrite.TrySetResult();
        }

        Assert.That(refresh.Wait(TimeSpan.FromSeconds(5)), Is.True);
    }

    [Test]
    public void RefreshElement_OfTrashedElement_DeletesTheRowsSynchronously()
    {
        Guid key = Guid.NewGuid();
        var repository = new Mock<IDatabaseCacheRepository>();
        ElementCacheService service = CreateService(repository.Object, CreateScopeProvider(new ManualResetEventSlim()), CreateHybridCache(() => ValueTask.CompletedTask).Object);

        service.RefreshElement(CreateElement(key, trashed: true, id: 7));

        repository.Verify(r => r.DeleteContentItem(7), Times.Once);
        repository.Verify(r => r.DeleteContentItemAsync(It.IsAny<int>()), Times.Never);
    }

    [TestCase(PublishedState.Publishing, 2, 0)]
    [TestCase(PublishedState.Unpublishing, 1, 1)]
    [TestCase(PublishedState.Published, 1, 0)]
    public void RefreshElement_WritesTheRowsSynchronously(PublishedState publishedState, int expectedRefreshes, int expectedPublishedRemovals)
    {
        Guid key = Guid.NewGuid();
        var repository = new Mock<IDatabaseCacheRepository>();
        var cacheNodeFactory = new Mock<ICacheNodeFactory>();
        cacheNodeFactory
            .Setup(f => f.ToContentCacheNode(It.IsAny<IElement>(), It.IsAny<bool>()))
            .Returns((IElement _, bool preview) => new ContentCacheNode { Id = 7, Key = key, ContentTypeId = 1, IsDraft = preview });
        ElementCacheService service = CreateService(repository.Object, CreateScopeProvider(new ManualResetEventSlim()), CreateHybridCache(() => ValueTask.CompletedTask).Object, cacheNodeFactory: cacheNodeFactory.Object);

        service.RefreshElement(CreateElement(key, trashed: false, id: 7, publishedState));

        repository.Verify(r => r.RefreshElement(It.IsAny<ContentCacheNode>()), Times.Exactly(expectedRefreshes));
        repository.Verify(r => r.RemovePublishedElement(7), Times.Exactly(expectedPublishedRemovals));
        repository.Verify(r => r.RefreshElementAsync(It.IsAny<ContentCacheNode>()), Times.Never);
        repository.Verify(r => r.RemovePublishedElementAsync(It.IsAny<int>()), Times.Never);
    }

    private static IElement CreateElement(Guid key, bool trashed, int id = 1, PublishedState publishedState = PublishedState.Published)
    {
        var element = new Mock<IElement>();
        element.SetupGet(e => e.Key).Returns(key);
        element.SetupGet(e => e.Id).Returns(id);
        element.SetupGet(e => e.Trashed).Returns(trashed);
        element.SetupGet(e => e.PublishedState).Returns(publishedState);
        return element.Object;
    }

    private static Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> CreateHybridCache(Func<ValueTask> setAsync)
        => CreateHybridCache(_ => setAsync());

    private static Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache> CreateHybridCache(Func<ContentCacheNode, ValueTask> setAsync)
    {
        var hybridCache = new Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        hybridCache
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ContentCacheNode>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Returns((string _, ContentCacheNode node, HybridCacheEntryOptions _, IEnumerable<string> _, CancellationToken _) => setAsync(node));
        hybridCache
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);
        return hybridCache;
    }

    private static Mock<IDatabaseCacheRepository> CreateRepository(params Guid[] publishedKeys)
    {
        var repository = new Mock<IDatabaseCacheRepository>();
        foreach (Guid key in publishedKeys)
        {
            repository.Setup(r => r.GetElementSourceForPublishStates(key)).Returns((null, new ContentCacheNode { Id = 1, Key = key, ContentTypeId = 1 }));
        }

        return repository;
    }

    /// <summary>
    /// Every scope is its own mock; <paramref name="scopeDisposed"/> is set when one is disposed.
    /// </summary>
    private static ICoreScopeProvider CreateScopeProvider(ManualResetEventSlim scopeDisposed)
    {
        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(p => p.CreateCoreScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(), It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(() =>
            {
                var scope = new Mock<ICoreScope>();
                scope.Setup(s => s.Dispose()).Callback(scopeDisposed.Set);
                return scope.Object;
            });
        return scopeProvider.Object;
    }

    private static ElementCacheService CreateService(
        IDatabaseCacheRepository repository,
        ICoreScopeProvider scopeProvider,
        Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache,
        IConvertedPublishedContentCache<string, IPublishedElement>? convertedCache = null,
        ICacheNodeFactory? cacheNodeFactory = null)
    {
        var cacheFactory = new Mock<IConvertedPublishedContentCacheFactory>();
        cacheFactory
            .Setup(f => f.Create<string, IPublishedElement>(It.IsAny<int?>(), It.IsAny<string>()))
            .Returns(convertedCache ?? Mock.Of<IConvertedPublishedContentCache<string, IPublishedElement>>());

        return new ElementCacheService(
            repository,
            scopeProvider,
            hybridCache,
            Mock.Of<IPublishedContentFactory>(),
            cacheNodeFactory ?? Mock.Of<ICacheNodeFactory>(),
            [],
            Mock.Of<IPublishedModelFactory>(),
            Mock.Of<IPreviewService>(),
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            NullLogger<ElementCacheService>.Instance,
            cacheFactory.Object,
            Mock.Of<IPreviewAccessEvaluator>());
    }
}
