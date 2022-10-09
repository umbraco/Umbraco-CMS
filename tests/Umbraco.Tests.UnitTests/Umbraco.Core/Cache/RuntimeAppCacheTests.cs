// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

public abstract class RuntimeAppCacheTests : AppCacheTests
{
    internal abstract IAppPolicyCache AppPolicyCache { get; }

    [Test]
    [Explicit("Testing for timeouts cannot work on VSTS.")]
    public void Can_Add_And_Expire_Struct_Strongly_Typed_With_Null()
    {
        var now = DateTime.Now;
        AppPolicyCache.Insert("DateTimeTest", () => now, new TimeSpan(0, 0, 0, 0, 200));
        Assert.AreEqual(now, AppCache.GetCacheItem<DateTime>("DateTimeTest"));
        Assert.AreEqual(now, AppCache.GetCacheItem<DateTime?>("DateTimeTest"));

        Thread.Sleep(300); // sleep longer than the cache expiration

        Assert.AreEqual(default(DateTime), AppCache.GetCacheItem<DateTime>("DateTimeTest"));
        Assert.AreEqual(null, AppCache.GetCacheItem<DateTime?>("DateTimeTest"));
    }
}
