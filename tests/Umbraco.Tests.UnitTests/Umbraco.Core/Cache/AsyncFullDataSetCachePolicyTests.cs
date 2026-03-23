// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections;
using System.Collections.Concurrent;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Cache;
using IScopeAccessor = Umbraco.Cms.Core.Scoping.EFCore.IScopeAccessor;
using Umbraco.Cms.Tests.Common.Attributes;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class AsyncFullDataSetCachePolicyTests
{
    private IScopeAccessor DefaultAccessor
    {
        get
        {
            var accessor = new Mock<IScopeAccessor>();
            var scope = new Mock<ICoreScope>();
            scope.Setup(x => x.RepositoryCacheMode).Returns(RepositoryCacheMode.Default);
            accessor.Setup(x => x.AmbientScope).Returns(scope.Object);
            return accessor.Object;
        }
    }

    private AsyncFullDataSetRepositoryCachePolicy<AuditItem, object> CreatePolicy(
        IAppPolicyCache cache, bool expires = false)
        => new(cache, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, expires);

    private static AuditItem MakeItem(int id) => new(id, AuditType.Copy, 123, "test", $"item{id}");

    private static DeepCloneableList<AuditItem> CacheList(params AuditItem[] items)
    {
        var list = new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce);
        foreach (var item in items)
        {
            list.Add(item);
        }

        return list;
    }

    [Test]
    public async Task Caches_Single()
    {
        AuditItem[] getAll = [MakeItem(1), MakeItem(2)];

        var isCached = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback(() => isCached = true);

        var unused = await CreatePolicy(cache.Object).GetAsync(1, async id => MakeItem(1), async () => getAll);
        Assert.IsTrue(isCached);
    }

    [Test]
    public async Task Get_Single_From_Cache()
    {
        AuditItem[] getAll = [MakeItem(1), MakeItem(2)];

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(CacheList(MakeItem(1)));

        var found = await CreatePolicy(cache.Object).GetAsync(1, async id => null, async () => getAll);
        Assert.IsNotNull(found);
    }

    [Test]
    public async Task Get_All_Caches_Empty_List()
    {
        var getAll = new AuditItem[] { };
        var cached = new List<string>();
        IList? list = null;

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b) =>
            {
                cached.Add(cacheKey);
                list = o() as IList;
            });

        // Return null on first pass (not yet cached), then return cached list.
        cache.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(() => cached.Any() ? new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce) : null);

        var found = await CreatePolicy(cache.Object).GetAllAsync(async () => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);

        // Do it again with a new policy instance — must come from cache, no new Insert.
        found = await CreatePolicy(cache.Object).GetAllAsync(async () => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

    [Test]
    public async Task Get_All_Caches_As_Single_List()
    {
        AuditItem[] getAll = [MakeItem(1), MakeItem(2)];

        var cached = new List<string>();
        IList? list = null;

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b) =>
            {
                cached.Add(cacheKey);
                list = o() as IList;
            });
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns((object?)null);

        var found = await CreatePolicy(cache.Object).GetAllAsync(async () => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

    [Test]
    public async Task Get_All_Without_Ids_From_Cache()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(() => CacheList(MakeItem(1), MakeItem(2)));

        var found = await CreatePolicy(cache.Object).GetAllAsync(async () => null);
        Assert.AreEqual(2, found.Length);
    }

    [Test]
    public async Task If_CreateOrUpdate_Throws_Cache_Is_Removed()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>())).Callback(() => cacheCleared = true);

        try
        {
            await CreatePolicy(cache.Object).UpdateAsync(MakeItem(1), item => throw new Exception("blah!"));
        }
        catch
        {
            // We need this catch or nunit throws up.
        }
        finally
        {
            Assert.IsTrue(cacheCleared);
        }
    }

    [Test]
    public async Task If_Removes_Throws_Cache_Is_Removed()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>())).Callback(() => cacheCleared = true);

        try
        {
            await CreatePolicy(cache.Object).DeleteAsync(MakeItem(1), item => throw new Exception("blah!"));
        }
        catch
        {
            // We need this catch or nunit throws up.
        }
        finally
        {
            Assert.IsTrue(cacheCleared);
        }
    }

    [Test]
    public async Task GetCachedAsync_Returns_Null_On_Cache_Miss()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns((object?)null);

        var result = await CreatePolicy(cache.Object).GetCachedAsync(1);
        Assert.IsNull(result);
    }

    [Test]
    public async Task GetCachedAsync_Returns_Entity_From_Cache()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(CacheList(MakeItem(1), MakeItem(2)));

        var result = await CreatePolicy(cache.Object).GetCachedAsync(1);
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Id);
    }

    [Test]
    public async Task ExistsAsync_Returns_True_When_Entity_Exists()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(CacheList(MakeItem(1), MakeItem(2)));

        var result = await CreatePolicy(cache.Object).ExistsAsync(1, async id => false, async () => null);
        Assert.IsTrue(result);
    }

    [Test]
    public async Task ExistsAsync_Returns_False_When_Entity_Does_Not_Exist()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(CacheList(MakeItem(1), MakeItem(2)));

        var result = await CreatePolicy(cache.Object).ExistsAsync(99, async id => true, async () => null);
        Assert.IsFalse(result);
    }

    [Test]
    public async Task If_Create_Throws_Cache_Is_Removed()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>())).Callback(() => cacheCleared = true);

        try
        {
            await CreatePolicy(cache.Object).CreateAsync(MakeItem(1), item => throw new Exception("blah!"));
        }
        catch
        {
            // We need this catch or nunit throws up.
        }
        finally
        {
            Assert.IsTrue(cacheCleared);
        }
    }

    [Test]
    public async Task CreateOrUpdate_Clears_Cache_On_Success()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>())).Callback(() => cacheCleared = true);

        await CreatePolicy(cache.Object).UpdateAsync(MakeItem(1), async item => { });

        Assert.IsTrue(cacheCleared);
    }

    [Test]
    public async Task Delete_Clears_Cache_On_Success()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>())).Callback(() => cacheCleared = true);

        await CreatePolicy(cache.Object).DeleteAsync(MakeItem(1), async item => { });

        Assert.IsTrue(cacheCleared);
    }

    [Test]
    public async Task GetManyAsync_Returns_Only_Requested_Ids()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(CacheList(MakeItem(1), MakeItem(2), MakeItem(3), MakeItem(4), MakeItem(5)));

        var result = await CreatePolicy(cache.Object).GetManyAsync(
            new object[] { 1, 3, 5 },
            async ids => null,
            async () => null);

        Assert.AreEqual(3, result.Length);
        Assert.IsTrue(result.Any(x => x.Id == 1));
        Assert.IsTrue(result.Any(x => x.Id == 3));
        Assert.IsTrue(result.Any(x => x.Id == 5));
    }

    [Test]
    public async Task GetAllAsync_With_Expires_Uses_Timeout_Insert()
    {
        TimeSpan? capturedTimeout = null;
        bool capturedIsSliding = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string key, Func<object> factory, TimeSpan? timeout, bool isSliding) =>
            {
                capturedTimeout = timeout;
                capturedIsSliding = isSliding;
            });
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns((object?)null);

        await CreatePolicy(cache.Object, expires: true).GetAllAsync(async () => [MakeItem(1)]);

        Assert.IsNotNull(capturedTimeout);
        Assert.IsTrue(capturedIsSliding);
    }

    [Test]
    public async Task GetAllAsync_Does_Not_Cache_When_PerformGetAll_Returns_Null()
    {
        var insertCallCount = 0;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback(() => insertCallCount++);
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns((object?)null);

        var policy = CreatePolicy(cache.Object);

        // First call — performGetAll returns null, so Insert is never called.
        var result = await policy.GetAllAsync(async () => null);
        Assert.AreEqual(0, result.Length);
        Assert.AreEqual(0, insertCallCount);

        // Second call — cache is still cold so the DB is hit again.
        var dbCallCount = 0;
        await policy.GetAllAsync(async () =>
        {
            dbCallCount++;
            return null;
        });

        Assert.AreEqual(1, dbCallCount);
    }

    [Test]
    public async Task GetAllAsync_Filters_Null_Items_From_Result()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns((object?)null);

        IEnumerable<AuditItem> dataWithNulls = [MakeItem(1), null!, MakeItem(3)];

        var result = await CreatePolicy(cache.Object).GetAllAsync(async () => dataWithNulls);

        Assert.AreEqual(2, result.Length);
        Assert.IsTrue(result.All(x => x is not null));
    }

    /// <summary>
    /// Verifies that GetAllCachedAsync is thread-safe when multiple tasks encounter a cache miss simultaneously.
    /// </summary>
    /// <remarks>
    /// This test exposes the "thundering herd" problem where multiple concurrent tasks detect a cache miss
    /// and all attempt to query the database and populate the cache simultaneously.
    /// See: https://github.com/umbraco/Umbraco-CMS/issues/21350.
    /// </remarks>
    [Test]
    [LongRunning]
    public async Task GetAllCached_Is_ThreadSafe_On_Cache_Miss()
    {
        // Arrange
        const int TaskCount = 20;
        var threadSafetyExceptions = new ConcurrentBag<Exception>();
        var databaseCallCount = 0;

        AuditItem[] getAll = Enumerable.Range(1, 100).Select(MakeItem).ToArray();

        // Use a non-locking cache to expose race conditions.
        var policy = CreatePolicy(new NonLockingCache());

        // Act - start all tasks simultaneously for maximum contention.
        var startSignal = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, TaskCount).Select(_ => Task.Run(async () =>
        {
            await startSignal.Task; // All tasks start together for maximum contention.
            try
            {
                var result = await policy.GetAllAsync(async () =>
                {
                    Interlocked.Increment(ref databaseCallCount);
                    await Task.Delay(10); // Simulate DB latency to increase race window.
                    return getAll;
                });

                // Verify result integrity.
                Assert.AreEqual(100, result.Length, "Result should contain all items");
            }
            catch (Exception e)
            {
                // Only collect thread-safety exceptions.
                if (e is InvalidOperationException && e.Message.Contains("concurrent"))
                {
                    threadSafetyExceptions.Add(e);
                }
            }
        })).ToArray();

        startSignal.SetResult(); // Release all tasks simultaneously.
        await Task.WhenAll(tasks);

        // Assert - no thread-safety violations.
        Assert.IsEmpty(
            threadSafetyExceptions,
            $"Thread safety violation detected: {string.Join(Environment.NewLine, threadSafetyExceptions.Select(e => e.Message))}");

        // After the fix, database should only be called once due to semaphore locking.
        TestContext.WriteLine($"Database was called {databaseCallCount} times (should be 1 after fix)");
        Assert.That(databaseCallCount, Is.EqualTo(1), "Database should only be called once with proper locking");
    }

    /// <summary>
    /// Verifies that GetAllCachedAsync is thread-safe when reader tasks iterate the cache
    /// while writer tasks clear and repopulate it.
    /// </summary>
    /// <remarks>
    /// This test exposes race conditions where one task iterates the cached DeepCloneableList
    /// via ToArray() while another task clears or replaces the cache entry.
    /// See: https://github.com/umbraco/Umbraco-CMS/issues/21350.
    /// </remarks>
    [Test]
    [LongRunning]
    public async Task GetAllCached_Is_ThreadSafe_During_Iteration_And_Cache_Clear()
    {
        // Arrange
        const int TaskCount = 20;
        const int Iterations = 50;
        var threadSafetyExceptions = new ConcurrentBag<Exception>();

        // Use a non-locking cache to expose race conditions.
        var policy = CreatePolicy(new NonLockingCache());

        // Pre-populate cache.
        var initialData = Enumerable.Range(1, 50).Select(MakeItem).ToArray();
        await policy.GetAllAsync(async () => initialData);

        // Act - half tasks read, half tasks trigger cache refresh.
        var startSignal = new TaskCompletionSource();

        var tasks = Enumerable.Range(0, TaskCount).Select(taskIndex => Task.Run(async () =>
        {
            await startSignal.Task; // All tasks start together.

            for (int j = 0; j < Iterations; j++)
            {
                try
                {
                    if (taskIndex % 2 == 0)
                    {
                        // Reader task.
                        var result = await policy.GetAllAsync(async () => initialData);
                        Assert.IsNotNull(result);
                        Assert.IsTrue(result.Length > 0);
                    }
                    else
                    {
                        // Writer task - clears and repopulates cache.
                        await policy.ClearAllAsync();
                        var newData = Enumerable.Range(1, 50).Select(MakeItem).ToArray();
                        await policy.GetAllAsync(async () => newData);
                    }
                }
                catch (Exception e)
                {
                    // Only collect thread-safety exceptions.
                    if (e is InvalidOperationException && e.Message.Contains("concurrent"))
                    {
                        threadSafetyExceptions.Add(e);
                    }
                }
            }
        })).ToArray();

        startSignal.SetResult(); // Release all tasks.
        await Task.WhenAll(tasks);

        // Assert - no thread-safety violations.
        Assert.IsEmpty(
            threadSafetyExceptions,
            $"Thread safety violation detected: {string.Join(Environment.NewLine, threadSafetyExceptions.Select(e => e.Message))}");
    }

    /// <summary>
    /// A simple non-thread-safe cache for testing purposes.
    /// This allows race conditions to manifest that would be hidden by ObjectCacheAppCache's internal locking.
    /// </summary>
    private sealed class NonLockingCache : IAppPolicyCache
    {
        private readonly Dictionary<string, object?> _cache = [];

        public object? Get(string key)
            => _cache.TryGetValue(key, out var value) ? value : null;

        public object? Get(string key, Func<object?> factory) => Get(key, factory, null, false);

        public object? Get(string key, Func<object?> factory, TimeSpan? timeout, bool isSliding = false)
        {
            if (_cache.TryGetValue(key, out var value))
            {
                return value;
            }

            value = factory();
            _cache[key] = value;
            return value;
        }

        public IEnumerable<object> SearchByKey(string keyStartsWith)
            => _cache.Where(kvp => kvp.Key.StartsWith(keyStartsWith)).Select(kvp => kvp.Value!);

        public IEnumerable<object> SearchByRegex(string regex) => throw new NotImplementedException();

        public void Clear() => _cache.Clear();

        public void Clear(string key) => _cache.Remove(key);

        public void ClearOfType(Type type) => throw new NotImplementedException();

        public void ClearOfType<T>() => throw new NotImplementedException();

        public void ClearOfType<T>(Func<string, T, bool> predicate) => throw new NotImplementedException();

        public void ClearByKey(string keyStartsWith)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(keyStartsWith)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

        public void ClearByRegex(string regex) => throw new NotImplementedException();

        public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
            => _cache[key] = factory();
    }
}
