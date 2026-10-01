// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class RepositoryCacheVersionServiceTests
{
    private Mock<ICoreScopeProvider> _scopeProvider = null!;
    private Mock<IRepositoryCacheVersionRepository> _repository = null!;
    private Mock<IRepositoryCacheVersionAccessor> _accessor = null!;
    private FakeScopeContext _defaultScopeContext = null!;
    private RepositoryCacheVersionService _sut = null!;
    private RepositoryCacheVersionService _deferringSut = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new Mock<IRepositoryCacheVersionRepository>();
        _accessor = new Mock<IRepositoryCacheVersionAccessor>();

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

        // No request cache: versions are written as soon as they are registered.
        _sut = new RepositoryCacheVersionService(
            _scopeProvider.Object,
            _repository.Object,
            NullLogger<RepositoryCacheVersionService>.Instance,
            _accessor.Object,
            Mock.Of<IRequestCache>());

        // A request cache: versions are deferred until FlushCacheUpdatesAsync.
        _deferringSut = new RepositoryCacheVersionService(
            _scopeProvider.Object,
            _repository.Object,
            NullLogger<RepositoryCacheVersionService>.Instance,
            _accessor.Object,
            new DictionaryAppCache());

        _repository.Setup(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>())).Returns(Task.CompletedTask);
    }

    [Test]
    public async Task IsCacheSyncedAsync_ReturnsFalse_WhenAccessorReturnsStaleVersion()
    {
        // Arrange: SetCachesSyncedAsync writes V1 into _cacheVersions.
        var cacheKey = _sut.GetCacheKey<IContent>();
        var v0 = Guid.NewGuid();
        var v1 = Guid.NewGuid();

        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[] { new RepositoryCacheVersion { Identifier = cacheKey, Version = v1.ToString() } });

        await _sut.SetCachesSyncedAsync();

        // Accessor (scope / request cache on another request path) still returns stale V0.
        _accessor
            .Setup(x => x.GetAsync(cacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = cacheKey, Version = v0.ToString() });

        // Act
        var isSynced = await _sut.IsCacheSyncedAsync<IContent>();

        // Assert: local V1 ≠ accessor V0 → not synced.
        Assert.IsFalse(isSynced, "Cache should be out of sync when the accessor returns a stale version.");
    }

    [Test]
    public async Task IsCacheSyncedAsync_ReturnsTrue_WhenAccessorAndLocalVersionMatch()
    {
        var cacheKey = _sut.GetCacheKey<IContent>();
        var version = Guid.NewGuid();

        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[] { new RepositoryCacheVersion { Identifier = cacheKey, Version = version.ToString() } });

        await _sut.SetCachesSyncedAsync();

        _accessor
            .Setup(x => x.GetAsync(cacheKey))
            .ReturnsAsync(new RepositoryCacheVersion { Identifier = cacheKey, Version = version.ToString() });

        var isSynced = await _sut.IsCacheSyncedAsync<IContent>();

        Assert.IsTrue(isSynced);
    }

    [Test]
    public async Task SetCacheUpdatedAsync_WritesOnlyOncePerScopeForSameEntityType()
    {
        var cacheKey = _sut.GetCacheKey<IContent>();
        await _sut.SetCacheUpdatedAsync<IContent>();
        await _sut.SetCacheUpdatedAsync<IContent>();

        _repository.Verify(
            x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == cacheKey)),
            Times.Once,
            "Second call within the same scope must be deduplicated.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_WritesAgain_AfterScopeExit()
    {
        var cacheKey = _sut.GetCacheKey<IContent>();

        // First scope: one write.
        await _sut.SetCacheUpdatedAsync<IContent>();
        _defaultScopeContext.ScopeExit(completed: true);

        // New scope: deduplication state is cleared — write must happen again.
        var freshContext = new FakeScopeContext();
        _scopeProvider.Setup(x => x.Context).Returns(freshContext);

        await _sut.SetCacheUpdatedAsync<IContent>();

        _repository.Verify(
            x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == cacheKey)),
            Times.Exactly(2),
            "A new scope must allow a second write after the previous scope exited.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_WritesEveryTime_WhenNoScopeContext()
    {
        // Without a scope context there is no deduplication state to track.
        _scopeProvider.Setup(x => x.Context).Returns((IScopeContext?)null);

        var cacheKey = _sut.GetCacheKey<IContent>();
        await _sut.SetCacheUpdatedAsync<IContent>();
        await _sut.SetCacheUpdatedAsync<IContent>();

        _repository.Verify(
            x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == cacheKey)),
            Times.Exactly(2),
            "Without a scope context every call must write a new version.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_DoesNotPublishTheVersion_WhileARequestCacheIsAvailable()
    {
        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        _defaultScopeContext.ScopeExit(completed: true);

        _repository.Verify(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()), Times.Never);
        _accessor.Verify(x => x.VersionChanged(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public async Task FlushCacheUpdatesAsync_PublishesOneVersionPerPendingEntityType()
    {
        var contentKey = _deferringSut.GetCacheKey<IContent>();
        var mediaKey = _deferringSut.GetCacheKey<IMedia>();

        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        await _deferringSut.SetCacheUpdatedAsync<IMedia>();
        _defaultScopeContext.ScopeExit(completed: true);

        await _deferringSut.FlushCacheUpdatesAsync();

        _repository.Verify(x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == contentKey)), Times.Once);
        _repository.Verify(x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == mediaKey)), Times.Once);
        _accessor.Verify(x => x.VersionChanged(contentKey, It.IsAny<Guid>()), Times.Once);
        _accessor.Verify(x => x.VersionChanged(mediaKey, It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public async Task FlushCacheUpdatesAsync_PublishesTheVersionTheLocalCacheThenMatches()
    {
        var cacheKey = _deferringSut.GetCacheKey<IContent>();
        RepositoryCacheVersion? published = null;
        _repository
            .Setup(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()))
            .Callback<RepositoryCacheVersion>(v => published = v)
            .Returns(Task.CompletedTask);

        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        _defaultScopeContext.ScopeExit(completed: true);
        await _deferringSut.FlushCacheUpdatesAsync();

        Assert.That(published, Is.Not.Null);
        _accessor.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync(published);

        Assert.That(await _deferringSut.IsCacheSyncedAsync<IContent>(), Is.True, "The published version is not the one adopted locally.");
    }

    [Test]
    public async Task SetCacheUpdatedAsync_DropsThePendingVersion_WhenTheScopeDoesNotComplete()
    {
        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        _defaultScopeContext.ScopeExit(completed: false);

        await _deferringSut.FlushCacheUpdatesAsync();

        _repository.Verify(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()), Times.Never);
    }

    [Test]
    public async Task SetCacheUpdatedAsync_PendsImmediately_WithoutAScopeContext()
    {
        _scopeProvider.Setup(x => x.Context).Returns((IScopeContext?)null);

        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        _repository.Verify(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()), Times.Never);

        await _deferringSut.FlushCacheUpdatesAsync();

        _repository.Verify(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()), Times.Once);
    }

    [Test]
    public async Task FlushCacheUpdatesAsync_IsIdempotent()
    {
        await _deferringSut.SetCacheUpdatedAsync<IContent>();
        _defaultScopeContext.ScopeExit(completed: true);

        await _deferringSut.FlushCacheUpdatesAsync();
        await _deferringSut.FlushCacheUpdatesAsync();

        _repository.Verify(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>()), Times.Once);
    }

    [Test]
    public async Task SetCachesSyncedAsync_WithVersions_AdoptsThemWithoutReadingTheDatabase()
    {
        var cacheKey = _sut.GetCacheKey<IContent>();
        var version = new RepositoryCacheVersion { Identifier = cacheKey, Version = Guid.NewGuid().ToString() };

        await _sut.SetCachesSyncedAsync(new[] { version });

        _accessor.Setup(x => x.GetAsync(cacheKey)).ReturnsAsync(version);
        Assert.That(await _sut.IsCacheSyncedAsync<IContent>(), Is.True);
        _repository.Verify(x => x.GetAllAsync(), Times.Never);
        _accessor.Verify(x => x.CachesSynced(), Times.Once);
    }

    [Test]
    public async Task GetCacheVersionsAsync_ReturnsThePublishedVersions()
    {
        _repository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[]
            {
                new RepositoryCacheVersion { Identifier = "a", Version = Guid.NewGuid().ToString() },
                new RepositoryCacheVersion { Identifier = "b", Version = null },
            });

        IReadOnlyCollection<RepositoryCacheVersion> versions = await _sut.GetCacheVersionsAsync();

        Assert.That(versions.Select(x => x.Identifier), Is.EquivalentTo(new[] { "a", "b" }));
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
