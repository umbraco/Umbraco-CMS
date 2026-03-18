// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Provides unit tests for verifying the behavior and functionality of the AppCache implementation in Umbraco.
/// </summary>
public abstract class AppCacheTests
{
    internal abstract IAppCache AppCache { get; }

    protected abstract int GetTotalItemCount { get; }

    /// <summary>
    /// Sets up the test environment before each test.
    /// </summary>
    [SetUp]
    public virtual void Setup()
    {
    }

    /// <summary>
    /// Cleans up after each test by clearing the application cache.
    /// </summary>
    [TearDown]
    public virtual void TearDown() => AppCache.Clear();

    /// <summary>
    /// Tests that an InvalidOperationException is thrown when there is a reentry into the cache.
    /// </summary>
    [Test]
    public void Throws_On_Reentry()
    {
        // don't run for DictionaryAppCache - not making sense
        if (GetType() == typeof(DictionaryAppCacheTests))
        {
            Assert.Ignore("Do not run for DictionaryAppCache.");
        }

        Exception exception = null;
        var result = AppCache.Get("blah", () =>
        {
            try
            {
                var result2 = AppCache.Get("blah");
            }
            catch (Exception e)
            {
                exception = e;
            }

            return "value";
        });
        Assert.IsNotNull(exception);
        Assert.IsAssignableFrom<InvalidOperationException>(exception);
    }

    /// <summary>
    /// Tests that exceptions thrown during cache retrieval are not cached.
    /// Ensures that the cache does not store results when an exception occurs.
    /// </summary>
    [Test]
    public void Does_Not_Cache_Exceptions()
    {
        var counter = 0;

        object result;
        try
        {
            result = AppCache.Get("Blah", () =>
            {
                counter++;
                throw new Exception("Do not cache this");
            });
        }
        catch (Exception)
        {
        }

        try
        {
            result = AppCache.Get("Blah", () =>
            {
                counter++;
                throw new Exception("Do not cache this");
            });
        }
        catch (Exception)
        {
        }

        Assert.Greater(counter, 1);
    }

    /// <summary>
    /// Tests that null values are not cached by the AppCache.
    /// </summary>
    [Test]
    public void Does_Not_Cache_Null_Values()
    {
        var counter = 0;

        object? Factory()
        {
            counter++;
            return counter == 3 ? "Not a null value" : null;
        }

        object? Get() => AppCache.Get("Blah", Factory);

        Assert.IsNull(Get());
        Assert.IsNull(Get());
        Assert.AreEqual("Not a null value", Get());
        Assert.AreEqual(3, counter);
    }

    /// <summary>
    /// Ensures that the delegate result is cached only once.
    /// </summary>
    [Test]
    public void Ensures_Delegate_Result_Is_Cached_Once()
    {
        var counter = 0;

        object result;

        result = AppCache.Get("Blah", () =>
        {
            counter++;
            return string.Empty;
        });

        result = AppCache.Get("Blah", () =>
        {
            counter++;
            return string.Empty;
        });

        Assert.AreEqual(1, counter);
    }

    /// <summary>
    /// Tests that items can be retrieved from the cache by searching keys.
    /// </summary>
    [Test]
    public void Can_Get_By_Search()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        var result = AppCache.SearchByKey("Tes");

        Assert.AreEqual(3, result.Count());
    }

    /// <summary>
    /// Tests that the cache can be cleared by using a regular expression to match cache keys.
    /// </summary>
    [Test]
    public void Can_Clear_By_Expression()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("TTes1t", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearByRegex("^\\w+es\\d.*");

        Assert.AreEqual(2, GetTotalItemCount);
    }

    /// <summary>
    /// Verifies that cache entries whose keys match a specified prefix can be cleared using <see cref="IAppCache.ClearByKey(string)"/>.
    /// Adds several items to the cache, clears those with keys starting with "Test", and asserts that only the expected items remain.
    /// </summary>
    [Test]
    public void Can_Clear_By_Search()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearByKey("Test");

        Assert.AreEqual(2, GetTotalItemCount);
    }

    /// <summary>
    /// Tests that the cache can be cleared by specific keys.
    /// </summary>
    [Test]
    public void Can_Clear_By_Key()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.Clear("Test1");
        AppCache.Clear("Test2");

        Assert.AreEqual(2, GetTotalItemCount);
    }

    /// <summary>
    /// Tests that all items can be cleared from the cache.
    /// </summary>
    [Test]
    public void Can_Clear_All_Items()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.Clear();

        Assert.AreEqual(0, GetTotalItemCount);
    }

    /// <summary>
    /// Tests that an item can be added to the cache when it is not already available.
    /// </summary>
    [Test]
    public void Can_Add_When_Not_Available()
    {
        var cacheContent1 = new CacheContent();
        AppCache.Get("Test1", () => cacheContent1);
        Assert.AreEqual(1, GetTotalItemCount);
    }

    /// <summary>
    /// Verifies that retrieving an item from the cache by key returns the same instance when the item is already available in the cache.
    /// Ensures that the cache does not create a new instance for the same key.
    /// </summary>
    [Test]
    public void Can_Get_When_Available()
    {
        var cacheContent1 = new CacheContent();
        var result = AppCache.Get("Test1", () => cacheContent1);
        var result2 = AppCache.Get("Test1", () => cacheContent1);
        Assert.AreEqual(1, GetTotalItemCount);
        Assert.AreEqual(result, result2);
    }

    /// <summary>
    /// Tests that cache items can be removed by their type name.
    /// </summary>
    [Test]
    public void Can_Remove_By_Type_Name()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearOfType<CacheContent>();

        Assert.AreEqual(1, GetTotalItemCount);
    }

    /// <summary>
    /// Tests that items can be removed from the cache by their strong type.
    /// </summary>
    [Test]
    public void Can_Remove_By_Strong_Type()
    {
        var cacheContent1 = new CacheContent();
        var cacheContent2 = new CacheContent();
        var cacheContent3 = new CacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearOfType<CacheContent>();

        Assert.AreEqual(1, GetTotalItemCount);
    }

    // Just used for these tests
    private class CacheContent
    {
    }

    private class LiteralControl
    {
    }
}
