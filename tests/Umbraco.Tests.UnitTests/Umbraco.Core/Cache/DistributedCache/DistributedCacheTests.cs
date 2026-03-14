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
    /// <summary>
    /// Initializes the test environment and required dependencies before each test is executed.
    /// This method is called automatically by the test framework.
    /// </summary>
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

    /// <summary>
    /// Tests that the Refresh method correctly refreshes integer IDs in the distributed cache.
    /// </summary>
    [Test]
    public void RefreshIntId()
    {
        for (var i = 1; i < 11; i++)
        {
            _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
        }

        Assert.AreEqual(10, ServerMessenger.IntIdsRefreshed.Count);
    }

    /// <summary>
    /// Tests that the Refresh method correctly updates integer IDs from objects.
    /// </summary>
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

    /// <summary>
    /// Tests that the Refresh method correctly updates the GUID IDs in the distributed cache.
    /// </summary>
    [Test]
    public void RefreshGuidId()
    {
        for (var i = 0; i < 11; i++)
        {
            _distributedCache.Refresh(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), Guid.NewGuid());
        }

        Assert.AreEqual(11, ServerMessenger.GuidIdsRefreshed.Count);
    }

    /// <summary>
    /// Tests the removal of multiple IDs from the distributed cache.
    /// </summary>
    [Test]
    public void RemoveIds()
    {
        for (var i = 1; i < 13; i++)
        {
            _distributedCache.Remove(Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73"), i);
        }

        Assert.AreEqual(12, ServerMessenger.IntIdsRemoved.Count);
    }

    /// <summary>
    /// Tests that the distributed cache performs full refreshes correctly and counts them.
    /// </summary>
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
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        public int Id { get; set; }
    }

    internal class TestCacheRefresher : ICacheRefresher
    {
        public static readonly Guid UniqueId = Guid.Parse("E0F452CB-DCB2-4E84-B5A5-4F01744C5C73");

    /// <summary>
    /// Gets the unique identifier for the refresher.
    /// </summary>
        public Guid RefresherUniqueId => UniqueId;

        /// <summary>
        /// Gets the name of the test cache refresher.
        /// </summary>
        public string Name => "Test Cache Refresher";

        /// <summary>
        /// Refreshes all cache entries.
        /// </summary>
        public void RefreshAll()
        {
        }

    /// <summary>
    /// Refreshes the cache for the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the cache item to refresh.</param>
        public void Refresh(int id)
        {
        }

    /// <summary>
    /// Removes the cache item with the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the cache item to remove.</param>
        public void Remove(int id)
        {
        }

    /// <summary>
    /// Refreshes the cache for the specified identifier.
    /// </summary>
    /// <param name="id">The identifier to refresh.</param>
        public void Refresh(Guid id)
        {
        }
    }

    internal class TestServerMessenger : IServerMessenger
    {
        // Used for tests
    /// <summary>
    /// Gets the list of refreshed integer IDs used for testing purposes.
    /// </summary>
        public List<int> IntIdsRefreshed { get; } = new();

    /// <summary>
    /// Gets the list of GUIDs that have been refreshed.
    /// </summary>
        public List<Guid> GuidIdsRefreshed { get; } = new();

    /// <summary>
    /// Gets the list of integer IDs that have been removed.
    /// </summary>
        public List<int> IntIdsRemoved { get; } = new();

    /// <summary>
    /// Gets the list of payloads that have been removed.
    /// </summary>
        public List<string> PayloadsRemoved { get; } = new();

        /// <summary>
        /// Gets the list of payloads that have been refreshed.
        /// </summary>
        public List<string> PayloadsRefreshed { get; } = new();

        /// <summary>
        /// Gets the count of full refreshes.
        /// </summary>
        public int CountOfFullRefreshes { get; private set; }

    /// <summary>
    /// Queues a refresh operation for the specified cache refresher and payload.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="payload">The payload items to refresh.</param>
        public void QueueRefresh<TPayload>(ICacheRefresher refresher, TPayload[] payload)
        {
            // doing nothing
        }

    /// <summary>
    /// Queues a refresh operation for the specified cache refresher and instances.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="getNumericId">A function to get the numeric ID from each instance.</param>
    /// <param name="instances">The instances to refresh.</param>
        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances) =>
            IntIdsRefreshed.AddRange(instances.Select(getNumericId));

    /// <summary>
    /// Queues a refresh operation for the specified cache refresher and instances.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="getGuidId">A function to get the unique identifier for each instance.</param>
    /// <param name="instances">The instances to refresh in the cache.</param>
        public void QueueRefresh<T>(ICacheRefresher refresher, Func<T, Guid> getGuidId, params T[] instances) =>
            GuidIdsRefreshed.AddRange(instances.Select(getGuidId));

    /// <summary>
    /// Queues the removal of cache entries for the specified instances.
    /// </summary>
    /// <param name="refresher">The cache refresher used to identify the cache to remove from.</param>
    /// <param name="getNumericId">A function to extract the numeric identifier from each instance.</param>
    /// <param name="instances">The instances whose cache entries should be removed.</param>
        public void QueueRemove<T>(ICacheRefresher refresher, Func<T, int> getNumericId, params T[] instances) =>
            IntIdsRemoved.AddRange(instances.Select(getNumericId));

    /// <summary>
    /// Queues the removal of cache items identified by numeric IDs for the specified cache refresher.
    /// </summary>
    /// <param name="refresher">The cache refresher used to identify the cache to remove items from.</param>
    /// <param name="numericIds">The numeric IDs of the items to remove from the cache.</param>
        public void QueueRemove(ICacheRefresher refresher, params int[] numericIds) =>
            IntIdsRemoved.AddRange(numericIds);

    /// <summary>
    /// Queues a refresh operation for the specified cache refresher and numeric IDs.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="numericIds">The numeric IDs to refresh.</param>
        public void QueueRefresh(ICacheRefresher refresher, params int[] numericIds) =>
            IntIdsRefreshed.AddRange(numericIds);

    /// <summary>
    /// Queues a refresh operation for the specified cache refresher and GUID identifiers.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="guidIds">The GUID identifiers to refresh.</param>
        public void QueueRefresh(ICacheRefresher refresher, params Guid[] guidIds) =>
            GuidIdsRefreshed.AddRange(guidIds);

    /// <summary>
    /// Queues a refresh for all items using the specified cache refresher.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh.</param>
        public void QueueRefreshAll(ICacheRefresher refresher) => CountOfFullRefreshes++;

    /// <summary>
    /// Represents a synchronization operation for the test server messenger in distributed cache tests.
    /// Currently, this method does not contain any implementation.
    /// </summary>
        public void Sync()
        {
        }

    /// <summary>
    /// Sends messages in the test server messenger.
    /// </summary>
        public void SendMessages()
        {
        }

    /// <summary>
    /// Performs a refresh operation using the specified cache refresher and JSON payload.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the refresh operation.</param>
    /// <param name="jsonPayload">The JSON payload representing the data to refresh.</param>
        public void PerformRefresh(ICacheRefresher refresher, string jsonPayload) => PayloadsRefreshed.Add(jsonPayload);

    /// <summary>
    /// Performs the removal operation using the specified cache refresher and JSON payload.
    /// </summary>
    /// <param name="refresher">The cache refresher to use for the removal.</param>
    /// <param name="jsonPayload">The JSON payload representing the data to remove.</param>
        public void PerformRemove(ICacheRefresher refresher, string jsonPayload) => PayloadsRemoved.Add(jsonPayload);
    }

    internal class TestServerRegistrar : IServerRoleAccessor
    {
    /// <summary>
    /// Gets the collection of registered server addresses.
    /// </summary>
        public IEnumerable<IServerAddress> Registrations => new List<IServerAddress>
        {
            new TestServerAddress("localhost"),
        };

    /// <summary>
    /// Gets a value indicating the current role of the server in the distributed cache system.
    /// </summary>
        public ServerRole CurrentServerRole => throw new NotImplementedException();
    }

    /// <summary>
    /// Tests the behavior of the distributed cache when handling server addresses.
    /// This ensures that server address logic functions as expected in a distributed environment.
    /// </summary>
    public class TestServerAddress : IServerAddress
    {
    /// <summary>Initializes a new instance of the <see cref="TestServerAddress"/> class.</summary>
    /// <param name="address">The server address.</param>
        public TestServerAddress(string address) => ServerAddress = address;

    /// <summary>
    /// Gets the address of the test server.
    /// </summary>
        public string ServerAddress { get; }
    }
}
