// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Web.Common.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Cache;

[TestFixture]
public class RepositoryCacheVersionAccessorTests
{
    private const string CacheKey = "test-cache-key";

    private DictionaryAppCache _requestCache = null!;
    private Mock<IRepositoryCacheVersionRepository> _repository = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessor = null!;
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private FakeScopeContext _defaultScopeContext = null!;
    private RepositoryCacheVersionAccessor _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _requestCache = new DictionaryAppCache();

        _repository = new Mock<IRepositoryCacheVersionRepository>();

        _httpContextAccessor = new Mock<IHttpContextAccessor>();
        _httpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        _defaultScopeContext = new FakeScopeContext();
        _scopeProvider = new Mock<ICoreScopeProvider>();
        _scopeProvider.Setup(x => x.Context).Returns(_defaultScopeContext);
        _scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher?>(),
                It.IsAny<IScopedNotificationPublisher?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(Mock.Of<ICoreScope>());

        _sut = new RepositoryCacheVersionAccessor(
            _requestCache,
            _httpContextAccessor.Object,
            _repository.Object,
            _scopeProvider.Object,
            NullLogger<RepositoryCacheVersionAccessor>.Instance);
    }

    // --- Scope cache tier ---

    [Test]
    public async Task GetAsync_WhenRepositoryReturnsVersion_PopulatesScopeCacheAndRequestCache()
    {
        var version = Guid.NewGuid();
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = version.ToString() });

        var result = await _sut.GetAsync(CacheKey);

        Assert.AreEqual(version.ToString(), result?.Version);
        Assert.IsNotNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        // Second call within the same scope context must not hit the DB (scope cache hit).
        await _sut.GetAsync(CacheKey);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    [Test]
    public async Task GetAsync_WithinSameScopeContext_HitsDbOnlyOnce()
    {
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() });

        await _sut.GetAsync(CacheKey);
        await _sut.GetAsync(CacheKey);
        await _sut.GetAsync(CacheKey);

        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    [Test]
    public async Task GetAsync_AfterVersionChangedWithGuid_ReturnsNewVersionFromScopeCacheWithoutDbCall()
    {
        var newGuid = Guid.NewGuid();
        _sut.VersionChanged(CacheKey, newGuid);

        var result = await _sut.GetAsync(CacheKey);

        Assert.AreEqual(newGuid.ToString(), result?.Version);
        _repository.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task VersionChanged_WithGuid_UpdatesScopeCacheAndClearsRequestCache()
    {
        var initialVersion = Guid.NewGuid();
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = initialVersion.ToString() });

        // Populate both caches.
        await _sut.GetAsync(CacheKey);
        Assert.IsNotNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        // Update scope cache in-place; request cache must be cleared.
        var newerGuid = Guid.NewGuid();
        _sut.VersionChanged(CacheKey, newerGuid);

        Assert.IsNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        // Next read returns the newer GUID from scope cache — no DB round-trip.
        var result = await _sut.GetAsync(CacheKey);
        Assert.AreEqual(newerGuid.ToString(), result?.Version);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    [Test]
    public async Task ScopeCacheEntry_RemovedAfterScopeExit()
    {
        var guid1 = Guid.NewGuid();
        _sut.VersionChanged(CacheKey, guid1);

        // Scope cache is populated: GetAsync returns from it without hitting the DB.
        var resultBeforeExit = await _sut.GetAsync(CacheKey);
        Assert.AreEqual(guid1.ToString(), resultBeforeExit?.Version);
        _repository.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);

        // Trigger scope exit — the Enlist cleanup callback fires and removes the entry.
        _defaultScopeContext.ScopeExit(completed: true);

        // Switch to a fresh scope context; its scope cache is empty.
        var freshContext = new FakeScopeContext();
        _scopeProvider.Setup(x => x.Context).Returns(freshContext);
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() });

        await _sut.GetAsync(CacheKey);

        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    [Test]
    public async Task GetAsync_DifferentScopeContexts_DoNotShareScopeCache()
    {
        var guid1 = Guid.NewGuid();
        _sut.VersionChanged(CacheKey, guid1);

        // Switch to a different scope context.
        var context2 = new FakeScopeContext();
        _scopeProvider.Setup(x => x.Context).Returns(context2);

        var differentVersion = Guid.NewGuid().ToString();
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = differentVersion });

        var result = await _sut.GetAsync(CacheKey);

        // Must not have served context1's scope cache entry.
        Assert.AreNotEqual(guid1.ToString(), result?.Version);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    // --- Request cache tier ---

    [Test]
    public async Task GetAsync_WhenScopeContextIsNull_ReturnsFromRequestCache()
    {
        _scopeProvider.Setup(x => x.Context).Returns((IScopeContext?)null);
        var cachedVersion = new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() };
        _requestCache.Set(CacheKey, cachedVersion);

        var result = await _sut.GetAsync(CacheKey);

        Assert.AreEqual(cachedVersion.Version, result?.Version);
        _repository.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void VersionChanged_WithoutGuid_ClearsRequestCache()
    {
        _requestCache.Set(CacheKey, new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() });
        Assert.IsNotNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        _sut.VersionChanged(CacheKey);

        Assert.IsNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));
    }

    // --- Database tier ---

    [Test]
    public async Task GetAsync_WhenNeitherCacheHasVersion_QueriesDatabase()
    {
        _scopeProvider.Setup(x => x.Context).Returns((IScopeContext?)null);
        var dbVersion = new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() };
        _repository.Setup(x => x.GetAsync(CacheKey)).ReturnsAsync(dbVersion);

        var result = await _sut.GetAsync(CacheKey);

        Assert.AreEqual(dbVersion.Version, result?.Version);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    [Test]
    public async Task GetAsync_WhenRepositoryReturnsNull_ReturnsNullAndDoesNotCache()
    {
        _repository.Setup(x => x.GetAsync(CacheKey)).ReturnsAsync((RepositoryCacheVersion?)null);

        var result = await _sut.GetAsync(CacheKey);

        Assert.IsNull(result);
        Assert.IsNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        // A second call must also hit the DB (nothing was cached).
        await _sut.GetAsync(CacheKey);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Exactly(2));
    }

    [Test]
    public async Task GetAsync_WhenRepositoryReturnsVersionWithNullVersion_CachesInRequestButNotInScopeCache()
    {
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = null });

        var result = await _sut.GetAsync(CacheKey);

        // Object is returned and request-cached even though Version is null.
        Assert.IsNotNull(result);
        Assert.IsNotNull(_requestCache.GetCacheItem<RepositoryCacheVersion>(CacheKey));

        // Scope cache was NOT populated (Version was null).
        // Removing the request cache entry forces the next call to fall through to the DB.
        _requestCache.Remove(CacheKey);
        await _sut.GetAsync(CacheKey);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Exactly(2));
    }

    // --- HTTP context ---

    [Test]
    public async Task GetAsync_WhenHttpContextHasNonNullRequestServicesAndIsNotBackOffice_ReturnsNull()
    {
        // Non-null RequestServices without UmbracoRequestPaths registered → IsBackOfficeRequest() returns false.
        var httpContext = new DefaultHttpContext { RequestServices = Mock.Of<IServiceProvider>() };
        _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var result = await _sut.GetAsync(CacheKey);

        Assert.IsNull(result);
        _repository.Verify(x => x.GetAsync(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task CachesSynced_ClearsBothScopeCacheAndRequestCache()
    {
        var guid1 = Guid.NewGuid();
        _sut.VersionChanged(CacheKey, guid1);
        // VersionChanged removed the request cache key, so Set succeeds.
        _requestCache.Set(CacheKey, new RepositoryCacheVersion { Identifier = CacheKey, Version = guid1.ToString() });

        _sut.CachesSynced();

        // Both caches cleared: next GetAsync must fall through to the DB.
        _repository
            .Setup(x => x.GetAsync(CacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = CacheKey, Version = Guid.NewGuid().ToString() });
        await _sut.GetAsync(CacheKey);
        _repository.Verify(x => x.GetAsync(CacheKey), Times.Once);
    }

    private sealed class FakeScopeContext : IScopeContext
    {
        private readonly Dictionary<string, Action<bool>> _actions = new();

        public Guid InstanceId { get; } = Guid.NewGuid();

        public int CreatedThreadId => Environment.CurrentManagedThreadId;

        public void Enlist(string key, Action<bool> action, int priority = 100)
            => _actions.TryAdd(key, action);

        public T? Enlist<T>(string key, Func<T> creator, Action<bool, T?>? action = null, int priority = 100)
            => throw new NotSupportedException();

        public T? GetEnlisted<T>(string key)
            => throw new NotSupportedException();

        public void ScopeExit(bool completed)
        {
            foreach (Action<bool> a in _actions.Values)
            {
                a(completed);
            }

            _actions.Clear();
        }
    }
}
