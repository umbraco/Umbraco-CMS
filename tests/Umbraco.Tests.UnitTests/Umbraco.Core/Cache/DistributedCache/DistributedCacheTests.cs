// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Sync;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache.DistributedCache;

/// <summary>
///     Ensures that calls to DistributedCache methods carry through to the IServerMessenger correctly
/// </summary>
[TestFixture]
public class DistributedCacheTests
{
    [SetUp]
    public void Setup()
    {
        ServerRegistrar = new TestServerRegistrar();
        ServerMessenger = new TestServerMessenger();

        var cacheRefresherCollection = new CacheRefresherCollection(() => new[] { new TestCacheRefresher() });

        _distributedCache = new global::Umbraco.Cms.Core.Cache.DistributedCache(ServerMessenger, cacheRefresherCollection);
    }

    private global::Umbraco.Cms.Core.Cache.DistributedCache _distributedCache;

    private IServerRoleAccessor ServerRegistrar { get; set; }

    private TestServerMessenger ServerMessenger { get; set; }

    [Test]
    public void RefreshIntId()
    {
        for (var i = 1; i < 11; i++)
        {
            _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
        }

        Assert.AreEqual(10, ServerMessenger.IntIdsRefreshed.Count);
    }

    [Test]
    public void RefreshIntIdFromObject()
    {
        for (var i = 0; i < 10; i++)
        {
            _distributedCache.Refresh(
                Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"),
                x => x.Id,
                new TestObjectWithId { Id = i });
        }

        Assert.AreEqual(10, ServerMessenger.IntIdsRefreshed.Count);
    }

    [Test]
    public void RefreshGuidId()
    {
        for (var i = 0; i < 11; i++)
        {
            _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), Guid.NewGuid());
        }

        Assert.AreEqual(11, ServerMessenger.GuidIdsRefreshed.Count);
    }

    [Test]
    public void RemoveIds()
    {
        for (var i = 1; i < 13; i++)
        {
            _distributedCache.Remove(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
        }

        Assert.AreEqual(12, ServerMessenger.IntIdsRemoved.Count);
    }

    [Test]
    public void FullRefreshes()
    {
        for (var i = 0; i < 13; i++)
        {
            _distributedCache.RefreshAll(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"));
        }

        Assert.AreEqual(13, ServerMessenger.CountOfFullRefreshes);
    }

    internal class TestObjectWithId
    {
        public int Id { get; set; }
    }

    internal class TestCacheRefresher : ICacheRefresher
    {
        public static readonly Guid UniqueId = Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73");

        public Guid RefresherUniqueId => UniqueId;

        public string Name => "Test Cache Refresher";

        public void RefreshAll()
        {
        }

        public void Refresh(int id)
        {
        }

        public void Remove(int id)
        {
        }

        public void Refresh(Guid id)
        {
        }
    }

    internal class TestServerMessenger : IServerMessenger
    {
        // Used for tests
        public List<int> IntIdsRefreshed { get; } = new();

        public List<Guid> GuidIdsRefreshed { get; } = new();

        public List<int> IntIdsRemoved { get; } = new();

        public List<string> PayloadsRemoved { get; } = new();

        public List<string> PayloadsRefreshed { get; } = new();

        public int CountOfFullRefreshes { get; private set; }

        public void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
        {
            // doing nothing
        }

        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances) =>
            IntIdsRefreshed.AddRange(instances.Select(getNumericId));

        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances) =>
            GuidIdsRefreshed.AddRange(instances.Select(getGuidId));

        public void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances) =>
            IntIdsRemoved.AddRange(instances.Select(getNumericId));

        public void QueueRemove(ICacheRefresher refresher, params int[] numericIds) =>
            IntIdsRemoved.AddRange(numericIds);

        public void QueueRefresh(ICacheRefresher refresher, params int[] numericIds) =>
            IntIdsRefreshed.AddRange(numericIds);

        public void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds) =>
            GuidIdsRefreshed.AddRange(guidIds);

        public void QueueRefreshAll(ICacheRefresher refresher) => CountOfFullRefreshes++;

        public void Sync()
        {
        }

        public void SendMessages()
        {
        }

        public void PerformRefresh(ICacheRefresher refresher, string jsonPayload) => PayloadsRefreshed.Add(jsonPayload);

        public void PerformRemove(ICacheRefresher refresher, string jsonPayload) => PayloadsRemoved.Add(jsonPayload);
    }

    internal class TestServerRegistrar : IServerRoleAccessor
    {
        public IEnumerable<IServerAddress> Registrations => new List<IServerAddress>
        {
            new TestServerAddress("localhost"),
        };

        public ServerRole CurrentServerRole => throw new NotImplementedException();
    }

    public class TestServerAddress : IServerAddress
    {
        public TestServerAddress(string address) => ServerAddress = address;

        public string ServerAddress { get; }
    }
}
