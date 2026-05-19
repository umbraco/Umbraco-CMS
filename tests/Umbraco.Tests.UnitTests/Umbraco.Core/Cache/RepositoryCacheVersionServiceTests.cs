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

        _sut = new RepositoryCacheVersionService(
            _scopeProvider.Object,
            _repository.Object,
            NullLogger<RepositoryCacheVersionService>.Instance,
            _accessor.Object);
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
        _repository.Setup(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>())).Returns(Task.CompletedTask);

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
        _repository.Setup(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>())).Returns(Task.CompletedTask);

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
        _repository.Setup(x => x.SaveAsync(It.IsAny<RepositoryCacheVersion>())).Returns(Task.CompletedTask);

        await _sut.SetCacheUpdatedAsync<IContent>();
        await _sut.SetCacheUpdatedAsync<IContent>();

        _repository.Verify(
            x => x.SaveAsync(It.Is<RepositoryCacheVersion>(v => v.Identifier == cacheKey)),
            Times.Exactly(2),
            "Without a scope context every call must write a new version.");
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
