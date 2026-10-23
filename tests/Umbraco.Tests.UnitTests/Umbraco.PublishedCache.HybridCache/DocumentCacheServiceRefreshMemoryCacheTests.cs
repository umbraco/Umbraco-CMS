using System.Data;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.HybridCache.Factories;
using Umbraco.Cms.Infrastructure.HybridCache.Persistence;
using Umbraco.Cms.Infrastructure.HybridCache.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.PublishedCache.HybridCache;

/// <summary>
/// The post-commit memory refresh runs under the content tree read lock, which blocks every save. The cache
/// writes must be issued while the lock is held (so the lock orders them against a later commit) but the wait for
/// a distributed write to complete must not keep the lock: that wait needs a thread-pool hop, and on a starved
/// pool it stretches the hold from milliseconds to seconds.
/// </summary>
[TestFixture]
public class DocumentCacheServiceRefreshMemoryCacheTests
{
    [Test]
    public void RefreshMemoryCache_ReleasesTheScope_BeforeWaitingForTheDistributedWrite()
    {
        var distributedWrite = new TaskCompletionSource();
        var scopeDisposed = new ManualResetEventSlim();
        var writeIssuedBeforeScopeDisposed = false;

        var hybridCache = new Mock<Microsoft.Extensions.Caching.Hybrid.HybridCache>();
        hybridCache
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ContentCacheNode>(), It.IsAny<HybridCacheEntryOptions>(), It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .Callback(() => writeIssuedBeforeScopeDisposed = !scopeDisposed.IsSet)
            .Returns(new ValueTask(distributedWrite.Task));
        hybridCache
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var scope = new Mock<ICoreScope>();
        scope.Setup(s => s.Dispose()).Callback(scopeDisposed.Set);
        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(p => p.CreateCoreScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher?>(), It.IsAny<IScopedNotificationPublisher?>(), It.IsAny<bool?>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .Returns(scope.Object);

        Guid key = Guid.NewGuid();
        var publishedNode = new ContentCacheNode { Id = 1, Key = key, ContentTypeId = 1 };
        var repository = new Mock<IDatabaseCacheRepository>();
        repository.Setup(r => r.GetContentSourceForPublishStates(key)).Returns((null, publishedNode));

        DocumentCacheService service = CreateService(repository.Object, scopeProvider.Object, hybridCache.Object);

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
        scope.Verify(s => s.ReadLock(global::Umbraco.Cms.Core.Constants.Locks.ContentTree), Times.Once);
        scope.Verify(s => s.Complete(), Times.Once);
    }

    private static DocumentCacheService CreateService(IDatabaseCacheRepository repository, ICoreScopeProvider scopeProvider, Microsoft.Extensions.Caching.Hybrid.HybridCache hybridCache)
    {
        var publishStatus = new Mock<IPublishStatusQueryService>();
        publishStatus.Setup(x => x.HasPublishedAncestorPath(It.IsAny<Guid>())).Returns(true);

        return new DocumentCacheService(
            repository,
            Mock.Of<IIdKeyMap>(),
            scopeProvider,
            hybridCache,
            Mock.Of<IPublishedContentFactory>(),
            Mock.Of<ICacheNodeFactory>(),
            [],
            new OptionsWrapper<CacheSettings>(new CacheSettings()),
            Mock.Of<IPublishedModelFactory>(),
            Mock.Of<IPreviewService>(),
            publishStatus.Object,
            NullLogger<DocumentCacheService>.Instance,
            new ConvertedPublishedContentCacheFactory(null, NullLogger<ConvertedPublishedContentCacheFactory>.Instance));
    }
}
