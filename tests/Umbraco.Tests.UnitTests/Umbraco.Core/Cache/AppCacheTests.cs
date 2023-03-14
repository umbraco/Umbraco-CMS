// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
        Assert.IsNotNull(exception);
        Assert.IsAssignableFrom<InvalidOperationException>(exception);
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

        Assert.Greater(counter, 1);
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

        Assert.AreEqual(1, counter);
    }

    [Test]
    public void Can_Get_By_Search()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        var result = AppCache.SearchByKey("Tes");

        Assert.AreEqual(3, result.Count());
    }

    [Test]
    public void Can_Clear_By_Expression()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("TTes1t", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearByRegex("^\\w+es\\d.*");

        Assert.AreEqual(2, GetTotalItemCount);
    }

    [Test]
    public void Can_Clear_By_Search()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Tester2", () => cacheContent2);
        AppCache.Get("Tes3", () => cacheContent3);
        AppCache.Get("different4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearByKey("Test");

        Assert.AreEqual(2, GetTotalItemCount);
    }

    [Test]
    public void Can_Clear_By_Key()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
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

    [Test]
    public void Can_Clear_All_Items()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.Clear();

        Assert.AreEqual(0, GetTotalItemCount);
    }

    [Test]
    public void Can_Add_When_Not_Available()
    {
        var cacheContent1 = new MacroCacheContent();
        AppCache.Get("Test1", () => cacheContent1);
        Assert.AreEqual(1, GetTotalItemCount);
    }

    [Test]
    public void Can_Get_When_Available()
    {
        var cacheContent1 = new MacroCacheContent();
        var result = AppCache.Get("Test1", () => cacheContent1);
        var result2 = AppCache.Get("Test1", () => cacheContent1);
        Assert.AreEqual(1, GetTotalItemCount);
        Assert.AreEqual(result, result2);
    }

    [Test]
    public void Can_Remove_By_Type_Name()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        ////Provider.ClearCacheObjectTypes("umbraco.MacroCacheContent");
        AppCache.ClearOfType<MacroCacheContent>();

        Assert.AreEqual(1, GetTotalItemCount);
    }

    [Test]
    public void Can_Remove_By_Strong_Type()
    {
        var cacheContent1 = new MacroCacheContent();
        var cacheContent2 = new MacroCacheContent();
        var cacheContent3 = new MacroCacheContent();
        var cacheContent4 = new LiteralControl();
        AppCache.Get("Test1", () => cacheContent1);
        AppCache.Get("Test2", () => cacheContent2);
        AppCache.Get("Test3", () => cacheContent3);
        AppCache.Get("Test4", () => cacheContent4);

        Assert.AreEqual(4, GetTotalItemCount);

        AppCache.ClearOfType<MacroCacheContent>();

        Assert.AreEqual(1, GetTotalItemCount);
    }

    // Just used for these tests
    private class MacroCacheContent
    {
    }

    private class LiteralControl
    {
    }
}
