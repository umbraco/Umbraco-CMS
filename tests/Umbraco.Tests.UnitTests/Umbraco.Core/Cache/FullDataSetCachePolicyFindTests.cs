// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Concurrent;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Tests for <see cref="FullDataSetRepositoryCachePolicy{TEntity,TId}.FindCached"/> and
/// <see cref="FullDataSetRepositoryCachePolicy{TEntity,TId}.ExistsCached"/> methods.
/// </summary>
[TestFixture]
public class FullDataSetCachePolicyFindTests
{
    private IScopeAccessor DefaultAccessor
    {
        get
        {
            var accessor = new Mock<IScopeAccessor>();
            var scope = new Mock<IScope>();
            scope.Setup(x => x.RepositoryCacheMode).Returns(RepositoryCacheMode.Default);
            accessor.Setup(x => x.AmbientScope).Returns(scope.Object);
            return accessor.Object;
        }
    }

    private FullDataSetRepositoryCachePolicy<CloneCountingEntity, int> CreatePolicy(
        IAppPolicyCache? cache = null,
        bool expires = false)
    {
        cache ??= new NonLockingCache();
        return new FullDataSetRepositoryCachePolicy<CloneCountingEntity, int>(
            cache,
            DefaultAccessor,
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>(),
            item => item.Id,
            expires);
    }

    private static CloneCountingEntity[] CreateEntities(int count)
    {
        var entities = new CloneCountingEntity[count];
        for (var i = 0; i < count; i++)
        {
            entities[i] = new CloneCountingEntity(i + 1, $"entity-{i + 1}");
        }

        return entities;
    }

    [Test]
    public void FindCached_Returns_Matching_Entity()
    {
        var entities = CreateEntities(5);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        CloneCountingEntity.ResetCloneCount();

        var found = policy.FindCached(x => x.Id == 3, _ => entities);

        Assert.IsNotNull(found);
        Assert.AreEqual(3, found!.Id);
        Assert.AreEqual("entity-3", found.Name);
    }

    [Test]
    public void FindCached_Returns_Deep_Clone()
    {
        var entities = CreateEntities(3);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        var found = policy.FindCached(x => x.Id == 2, _ => entities);

        Assert.IsNotNull(found);
        Assert.AreEqual(2, found!.Id);

        // Must be a different reference than the cached original.
        var cachedOriginal = entities.First(x => x.Id == 2);
        Assert.AreNotSame(cachedOriginal, found);
    }

    [Test]
    public void FindCached_Returns_Null_When_No_Match()
    {
        var entities = CreateEntities(3);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        var found = policy.FindCached(x => x.Id == 999, _ => entities);

        Assert.IsNull(found);
    }

    [Test]
    public void FindCached_Populates_Cache_On_Miss()
    {
        var entities = CreateEntities(3);
        var performGetAllCallCount = 0;

        var policy = CreatePolicy();

        // Cache is empty — FindCached should trigger performGetAll.
        var found = policy.FindCached(x => x.Id == 2, _ =>
        {
            performGetAllCallCount++;
            return entities;
        });

        Assert.IsNotNull(found);
        Assert.AreEqual(2, found!.Id);
        Assert.AreEqual(1, performGetAllCallCount, "performGetAll should be called once to populate cache");

        // Second call should NOT trigger performGetAll again.
        var found2 = policy.FindCached(x => x.Id == 1, _ =>
        {
            performGetAllCallCount++;
            return entities;
        });

        Assert.IsNotNull(found2);
        Assert.AreEqual(1, performGetAllCallCount, "performGetAll should not be called again — cache is warm");
    }

    [Test]
    public void ExistsCached_Returns_True_For_Match()
    {
        var entities = CreateEntities(5);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        var exists = policy.ExistsCached(x => x.Id == 3, _ => entities);

        Assert.IsTrue(exists);
    }

    [Test]
    public void ExistsCached_Returns_False_For_No_Match()
    {
        var entities = CreateEntities(5);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        var exists = policy.ExistsCached(x => x.Id == 999, _ => entities);

        Assert.IsFalse(exists);
    }

    [Test]
    public void FindCached_Clones_Only_Matched_Entity()
    {
        const int entityCount = 50;
        var entities = CreateEntities(entityCount);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        CloneCountingEntity.ResetCloneCount();

        // FindCached should clone only the 1 matched entity.
        var found = policy.FindCached(x => x.Id == 25, _ => entities);

        Assert.IsNotNull(found);
        Assert.AreEqual(25, found!.Id);
        Assert.AreEqual(1, CloneCountingEntity.CloneCount, "FindCached should trigger exactly 1 DeepClone call");
    }

    [Test]
    public void GetAll_Clones_All_Entities()
    {
        // Verify the baseline: GetAll clones every entity.
        const int entityCount = 50;
        var entities = CreateEntities(entityCount);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        CloneCountingEntity.ResetCloneCount();

        // GetAll clones every entity.
        var all = policy.GetAll(null, _ => entities);

        Assert.AreEqual(entityCount, all.Length);
        Assert.AreEqual(entityCount, CloneCountingEntity.CloneCount, "GetAll should trigger N DeepClone calls");
    }

    [Test]
    public void ExistsCached_Does_Not_Clone()
    {
        const int entityCount = 50;
        var entities = CreateEntities(entityCount);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        CloneCountingEntity.ResetCloneCount();

        var exists = policy.ExistsCached(x => x.Id == 25, _ => entities);

        Assert.IsTrue(exists);
        Assert.AreEqual(0, CloneCountingEntity.CloneCount, "ExistsCached should trigger zero DeepClone calls");
    }

    [Test]
    [LongRunning]
    public void FindCached_Is_ThreadSafe()
    {
        const int threadCount = 20;
        var threads = new Thread[threadCount];
        var barrier = new ManualResetEventSlim(false);
        var exceptions = new ConcurrentBag<Exception>();

        var entities = CreateEntities(100);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        for (var i = 0; i < threadCount; i++)
        {
            var targetId = (i % 100) + 1;
            threads[i] = new Thread(() =>
            {
                barrier.Wait();
                try
                {
                    var result = policy.FindCached(x => x.Id == targetId, _ => entities);
                    Assert.IsNotNull(result);
                    Assert.AreEqual(targetId, result!.Id);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            });
        }

        using (ExecutionContext.SuppressFlow())
        {
            foreach (var t in threads)
            {
                t.Start();
            }
        }

        barrier.Set();
        foreach (var t in threads)
        {
            t.Join();
        }

        Assert.IsEmpty(
            exceptions,
            $"Thread safety violation: {string.Join(Environment.NewLine, exceptions.Select(e => e.Message))}");
    }

    [Test]
    public void FindAllCached_Clones_Only_Matched_Entities()
    {
        const int entityCount = 50;
        var entities = CreateEntities(entityCount);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        CloneCountingEntity.ResetCloneCount();

        // Find 5 entities out of 50.
        var targetIds = new HashSet<int> { 5, 10, 15, 20, 25 };
        var found = policy.FindAllCached(x => targetIds.Contains(x.Id), _ => entities);

        Assert.AreEqual(5, found.Length);
        Assert.AreEqual(5, CloneCountingEntity.CloneCount, "FindAllCached should clone only the 5 matched entities, not all 50");
    }

    [Test]
    public void FindAllCached_Returns_Empty_When_No_Match()
    {
        var entities = CreateEntities(10);
        var policy = CreatePolicy();

        // Populate cache.
        policy.GetAll(null, _ => entities);

        var found = policy.FindAllCached(x => x.Id > 999, _ => entities);

        Assert.IsEmpty(found);
    }

    /// <summary>
    /// A test entity that counts how many times DeepClone is called across all instances.
    /// </summary>
    private sealed class CloneCountingEntity : EntityBase
    {
        private static int _cloneCount;

        public CloneCountingEntity(int id, string name)
        {
            DisableChangeTracking();
            Id = id;
            Name = name;
            EnableChangeTracking();
        }

        public string Name { get; set; }

        public static int CloneCount => _cloneCount;

        public static void ResetCloneCount() => Interlocked.Exchange(ref _cloneCount, 0);

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);
            Interlocked.Increment(ref _cloneCount);
        }
    }
}
