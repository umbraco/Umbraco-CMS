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
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests that verify the behavior of the <see cref="FullDataSetRepositoryCachePolicy{TEntity, TId}"/> class.
/// </summary>
[TestFixture]
public class FullDataSetCachePolicyTests
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

    /// <summary>
    /// Tests that a single item is cached when using the FullDataSetRepositoryCachePolicy.
    /// </summary>
    [Test]
    public void Caches_Single()
    {
        AuditItem[] getAll =
        {
            new(1, AuditType.Copy, 123, "test", "blah"),
            new(2, AuditType.Copy, 123, "test", "blah2"),
        };

        var isCached = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback(() => isCached = true);

        var policy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        var unused = policy.Get(1, id => new AuditItem(1, AuditType.Copy, 123, "test", "blah"), ids => getAll);
        Assert.IsTrue(isCached);
    }

    /// <summary>
    /// Tests retrieving a single item from the cache using the FullDataSetRepositoryCachePolicy.
    /// </summary>
    [Test]
    public void Get_Single_From_Cache()
    {
        AuditItem[] getAll =
        {
            new(1, AuditType.Copy, 123, "test", "blah"), new(2, AuditType.Copy, 123, "test", "blah2"),
        };

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(new AuditItem(1, AuditType.Copy, 123, "test", "blah"));

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        var found = defaultPolicy.Get(1, id => null, ids => getAll);
        Assert.IsNotNull(found);
    }

    /// <summary>
    /// Tests that the GetAll method caches an empty list correctly and returns the cached list on subsequent calls.
    /// </summary>
    [Test]
    public void Get_All_Caches_Empty_List()
    {
        var getAll = new AuditItem[] { };

        var cached = new List<string>();

        IList list = null;

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b) =>
            {
                cached.Add(cacheKey);

                list = o() as IList;
            });

        // Return null if this is the first pass.
        cache.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(() => cached.Any() ? new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce) : null);

        var policy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        var found = policy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);

        // Do it again, ensure that its coming from the cache!
        policy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        found = policy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

    /// <summary>
    /// Tests that all cache entries are retrieved and combined into a single list.
    /// Ensures that the caching mechanism aggregates multiple cache items as expected.
    /// </summary>
    [Test]
    public void Get_All_Caches_As_Single_List()
    {
        AuditItem[] getAll =
        {
            new(1, AuditType.Copy, 123, "test", "blah"),
            new(2, AuditType.Copy, 123, "test", "blah2"),
        };

        var cached = new List<string>();
        IList list = null;

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b) =>
            {
                cached.Add(cacheKey);

                list = o() as IList;
            });
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(new AuditItem[] { });

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        var found = defaultPolicy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

    /// <summary>
    /// Tests that getting all items without specifying IDs returns all cached items.
    /// </summary>
    [Test]
    public void Get_All_Without_Ids_From_Cache()
    {
        AuditItem[] getAll = { null };

        var cache = new Mock<IAppPolicyCache>();

        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(() =>
            new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce)
            {
                new(1, AuditType.Copy, 123, "test", "blah"),
                new(2, AuditType.Copy, 123, "test", "blah2"),
            });

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);

        var found = defaultPolicy.GetAll(new object[] { }, ids => getAll);
        Assert.AreEqual(2, found.Length);
    }

    /// <summary>
    /// Tests that if the CreateOrUpdate operation throws an exception, the cache is cleared (removed).
    /// </summary>
    [Test]
    public void If_CreateOrUpdate_Throws_Cache_Is_Removed()
    {
        AuditItem[] getAll =
        {
            new(1, AuditType.Copy, 123, "test", "blah"),
            new(2, AuditType.Copy, 123, "test", "blah2"),
        };

        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>()))
            .Callback(() => cacheCleared = true);

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);
        try
        {
            defaultPolicy.Update(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
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

    /// <summary>
    /// Tests that if an exception is thrown during the removal of an item,
    /// the cache is cleared as expected.
    /// </summary>
    [Test]
    public void If_Removes_Throws_Cache_Is_Removed()
    {
        AuditItem[] getAll =
        {
            new(1, AuditType.Copy, 123, "test", "blah"),
            new(2, AuditType.Copy, 123, "test", "blah2"),
        };

        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>()))
            .Callback(() => cacheCleared = true);

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>(), item => item.Id, false);
        try
        {
            defaultPolicy.Delete(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
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

    /// <summary>
    /// Verifies that GetAllCached is thread-safe when multiple threads encounter a cache miss simultaneously.
    /// </summary>
    /// <remarks>
    /// This test exposes the "thundering herd" problem where multiple threads detect a cache miss
    /// and all attempt to query the database and populate the cache concurrently.
    /// See: https://github.com/umbraco/Umbraco-CMS/issues/21350.
    /// </remarks>
    [Test]
    [LongRunning]
    public void GetAllCached_Is_ThreadSafe_On_Cache_Miss()
    {
        // Arrange
        const int ThreadCount = 20;
        var threads = new Thread[ThreadCount];
        var barrier = new ManualResetEventSlim(false);
        var threadSafetyExceptions = new ConcurrentBag<Exception>();
        var databaseCallCount = 0;

        AuditItem[] getAll = Enumerable.Range(1, 100)
            .Select(i => new AuditItem(i, AuditType.Copy, 123, "test", $"item{i}"))
            .ToArray();

        // Use a non-locking cache to expose race conditions.
        var cache = new NonLockingCache();

        var policy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(
            cache,
            DefaultAccessor,
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>(),
            item => item.Id,
            false);

        // Act - spawn threads that all call GetAll concurrently.
        for (int i = 0; i < ThreadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                barrier.Wait(); // All threads start together for maximum contention.
                try
                {
                    var result = policy.GetAll(null, ids =>
                    {
                        Interlocked.Increment(ref databaseCallCount);
                        Thread.Sleep(10); // Simulate DB latency to increase race window.
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
            });
        }

        // Start all threads with suppressed execution context to avoid context leakage.
        using (ExecutionContext.SuppressFlow())
        {
            foreach (var t in threads)
            {
                t.Start();
            }
        }

        barrier.Set(); // Release all threads simultaneously.
        foreach (var t in threads)
        {
            t.Join();
        }

        // Assert - no thread-safety violations.
        Assert.IsEmpty(
            threadSafetyExceptions,
            $"Thread safety violation detected: {string.Join(Environment.NewLine, threadSafetyExceptions.Select(e => e.Message))}");

        // After the fix, database should ideally be called only once due to locking.
        // Before the fix, multiple threads may all call the database (thundering herd).
        TestContext.WriteLine($"Database was called {databaseCallCount} times (should be 1 after fix)");
        Assert.That(databaseCallCount, Is.EqualTo(1), "Database should only be called once with proper locking");
    }

    /// <summary>
    /// Verifies that GetAllCached is thread-safe when reader threads iterate the cache
    /// while writer threads clear and repopulate it.
    /// </summary>
    /// <remarks>
    /// This test exposes race conditions where one thread iterates the cached DeepCloneableList
    /// via ToArray() while another thread clears or replaces the cache entry.
    /// See: https://github.com/umbraco/Umbraco-CMS/issues/21350.
    /// </remarks>
    [Test]
    [LongRunning]
    public void GetAllCached_Is_ThreadSafe_During_Iteration_And_Cache_Clear()
    {
        // Arrange
        const int ThreadCount = 20;
        const int Iterations = 50;
        var threads = new Thread[ThreadCount];
        var barrier = new ManualResetEventSlim(false);
        var threadSafetyExceptions = new ConcurrentBag<Exception>();

        // Use a non-locking cache to expose race conditions.
        var cache = new NonLockingCache();

        var policy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(
            cache,
            DefaultAccessor,
            new SingleServerCacheVersionService(),
            Mock.Of<ICacheSyncService>(),
            item => item.Id,
            false);

        // Pre-populate cache.
        var initialData = Enumerable.Range(1, 50)
            .Select(i => new AuditItem(i, AuditType.Copy, 123, "test", $"item{i}"))
            .ToArray();
        policy.GetAll(null, _ => initialData);

        // Act - half threads read, half threads trigger cache refresh.
        for (int i = 0; i < ThreadCount; i++)
        {
            int threadIndex = i;
            threads[i] = new Thread(() =>
            {
                barrier.Wait(); // All threads start together.

                for (int j = 0; j < Iterations; j++)
                {
                    try
                    {
                        if (threadIndex % 2 == 0)
                        {
                            // Reader thread.
                            var result = policy.GetAll(null, _ => initialData);
                            Assert.IsNotNull(result);
                            Assert.IsTrue(result.Length > 0);
                        }
                        else
                        {
                            // Writer thread - clears and repopulates cache.
                            policy.ClearAll();
                            var newData = Enumerable.Range(1, 50)
                                .Select(k => new AuditItem(k, AuditType.Copy, 123, "test", $"new{k}"))
                                .ToArray();
                            policy.GetAll(null, _ => newData);
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
            });
        }

        // Start all threads with suppressed execution context.
        using (ExecutionContext.SuppressFlow())
        {
            foreach (var t in threads)
            {
                t.Start();
            }
        }

        barrier.Set(); // Release all threads.
        foreach (var t in threads)
        {
            t.Join();
        }

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

    /// <summary>
    /// Gets the cached object associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the cached object to get.</param>
    /// <returns>The cached object if found; otherwise, null.</returns>
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

    /// <summary>
    /// Searches the cache for entries with keys that start with the specified prefix.
    /// </summary>
    /// <param name="keyStartsWith">The prefix to match keys against.</param>
    /// <returns>An enumerable of cache values whose keys start with the specified prefix.</returns>
        public IEnumerable<object> SearchByKey(string keyStartsWith)
            => _cache.Where(kvp => kvp.Key.StartsWith(keyStartsWith)).Select(kvp => kvp.Value!);

    /// <summary>
    /// Searches the cache using the specified regular expression.
    /// </summary>
    /// <param name="regex">The regular expression to match cache entries.</param>
    /// <returns>An enumerable collection of objects matching the regular expression.</returns>
        public IEnumerable<object> SearchByRegex(string regex) => throw new NotImplementedException();

    /// <summary>
    /// Clears all items from the cache.
    /// </summary>
        public void Clear() => _cache.Clear();

        public void Clear(string key) => _cache.Remove(key);

    /// <summary>
    /// Clears the cache entries of the specified type.
    /// </summary>
    /// <param name="type">The type of cache entries to clear.</param>
        public void ClearOfType(Type type) => throw new NotImplementedException();

    /// <summary>
    /// Clears all cached items of the specified type.
    /// </summary>
        public void ClearOfType<T>() => throw new NotImplementedException();

    /// <summary>
    /// Clears items of the specified type from the cache based on the given predicate.
    /// </summary>
    /// <param name="predicate">A function to test each cache item for a condition.</param>
        public void ClearOfType<T>(Func<string, T, bool> predicate) => throw new NotImplementedException();

    /// <summary>
    /// Clears all cache entries with keys that start with the specified prefix.
    /// </summary>
    /// <param name="keyStartsWith">The prefix of the keys to clear from the cache.</param>
        public void ClearByKey(string keyStartsWith)
        {
            var keysToRemove = _cache.Keys.Where(k => k.StartsWith(keyStartsWith)).ToList();
            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }
        }

    /// <summary>
    /// Clears the cache entries that match the specified regular expression.
    /// </summary>
    /// <param name="regex">The regular expression pattern to match cache keys.</param>
        public void ClearByRegex(string regex) => throw new NotImplementedException();

    /// <summary>
    /// Inserts an item into the cache with the specified key, using the provided factory function to generate the value.
    /// </summary>
    /// <param name="key">The key to identify the cached item.</param>
    /// <param name="factory">A function to generate the cached item if it does not exist.</param>
    /// <param name="timeout">An optional timeout after which the cache entry expires.</param>
    /// <param name="isSliding">Indicates whether the timeout is sliding (resets on access) or absolute.</param>
        public void Insert(string key, Func<object?> factory, TimeSpan? timeout = null, bool isSliding = false)
            => _cache[key] = factory();
    }
}
