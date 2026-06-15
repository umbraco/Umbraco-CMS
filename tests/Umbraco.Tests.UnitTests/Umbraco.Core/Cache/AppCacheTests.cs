// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

public abstract class AppCacheTests
{
    internal abstract IAppCache AppCache { get; }

    protected abstract int GetTotalItemCount { get; }

    [SetUp]
    public virtual void Setup()
    {
    }

    [TearDown]
    public virtual void TearDown() => AppCache.Clear();

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
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception, Is.AssignableFrom<InvalidOperationException>());
    }

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

        Assert.That(counter, Is.GreaterThan(1));
    }

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

        Assert.That(Get(), Is.Null);
        Assert.That(Get(), Is.Null);
        Assert.That(Get(), Is.EqualTo("Not a null value"));
        Assert.That(counter, Is.EqualTo(3));
    }

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

        Assert.That(counter, Is.EqualTo(1));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        var result = AppCache.SearchByKey("Tes");

        Assert.That(result.Count(), Is.EqualTo(3));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.ClearByRegex("^\\w+es\\d.*");

        Assert.That(GetTotalItemCount, Is.EqualTo(2));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.ClearByKey("Test");

        Assert.That(GetTotalItemCount, Is.EqualTo(2));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.Clear("Test1");
        AppCache.Clear("Test2");

        Assert.That(GetTotalItemCount, Is.EqualTo(2));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.Clear();

        Assert.That(GetTotalItemCount, Is.EqualTo(0));
    }

    [Test]
    public void Can_Add_When_Not_Available()
    {
        var cacheContent1 = new CacheContent();
        AppCache.Get("Test1", () => cacheContent1);
        Assert.That(GetTotalItemCount, Is.EqualTo(1));
    }

    [Test]
    public void Can_Get_When_Available()
    {
        var cacheContent1 = new CacheContent();
        var result = AppCache.Get("Test1", () => cacheContent1);
        var result2 = AppCache.Get("Test1", () => cacheContent1);
        Assert.That(GetTotalItemCount, Is.EqualTo(1));
        Assert.That(result2, Is.EqualTo(result));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.ClearOfType<CacheContent>();

        Assert.That(GetTotalItemCount, Is.EqualTo(1));
    }

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

        Assert.That(GetTotalItemCount, Is.EqualTo(4));

        AppCache.ClearOfType<CacheContent>();

        Assert.That(GetTotalItemCount, Is.EqualTo(1));
    }

    // Just used for these tests
    private class CacheContent
    {
    }

    private class LiteralControl
    {
    }
}
