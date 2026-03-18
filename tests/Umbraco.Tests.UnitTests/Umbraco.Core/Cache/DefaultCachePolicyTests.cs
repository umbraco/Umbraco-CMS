// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests that verify the behavior of the <see cref="DefaultRepositoryCachePolicy{TEntity, TId}"/> class in the Umbraco CMS core caching system.
/// </summary>
[TestFixture]
public class DefaultCachePolicyTests
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
    /// Tests that a single item is cached correctly using the default cache policy.
    /// </summary>
    [Test]
    public void Caches_Single()
    {
        var isCached = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback(() => isCached = true);

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());

        var unused = defaultPolicy.Get(1, id => new AuditItem(1, AuditType.Copy, 123, "test", "blah"), o => null);
        Assert.IsTrue(isCached);
    }

    /// <summary>
    /// Tests retrieving a single item from the cache using the default cache policy.
    /// </summary>
    [Test]
    public void Get_Single_From_Cache()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(new AuditItem(1, AuditType.Copy, 123, "test", "blah"));

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());

        var found = defaultPolicy.Get(1, id => null, ids => null);
        Assert.IsNotNull(found);
    }

    /// <summary>
    /// Tests that caching occurs per ID when calling GetAll on the default cache policy.
    /// </summary>
    [Test]
    public void Caches_Per_Id_For_Get_All()
    {
        var cached = new List<string>();
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b) => cached.Add(cacheKey));
        cache.Setup(x => x.SearchByKey(It.IsAny<string>())).Returns(new AuditItem[] { });

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());

        var unused = defaultPolicy.GetAll(
            new object[] { },
            ids => new[]
            {
                new AuditItem(1, AuditType.Copy, 123, "test", "blah"),
                new AuditItem(2, AuditType.Copy, 123, "test", "blah2"),
            });

        Assert.AreEqual(2, cached.Count);
    }

    /// <summary>
    /// Tests that getting all items without specifying IDs returns all cached items.
    /// </summary>
    [Test]
    public void Get_All_Without_Ids_From_Cache()
    {
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.SearchByKey(It.IsAny<string>())).Returns(new[]
        {
            new AuditItem(1, AuditType.Copy, 123, "test", "blah"),
            new AuditItem(2, AuditType.Copy, 123, "test", "blah2"),
        });

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());

        var found = defaultPolicy.GetAll(new object[] { }, ids => new[] { (AuditItem)null });
        Assert.AreEqual(2, found.Length);
    }

    /// <summary>
    /// Tests that if the CreateOrUpdate operation throws an exception, the cache is cleared (removed).
    /// </summary>
    [Test]
    public void If_CreateOrUpdate_Throws_Cache_Is_Removed()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>()))
            .Callback(() => cacheCleared = true);

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());
        try
        {
            defaultPolicy.Update(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
        }
        catch
        {
            // We need this catch or nunit throws up
        }
        finally
        {
            Assert.IsTrue(cacheCleared);
        }
    }

    /// <summary>
    /// Tests that if the cache removal operation throws an exception, the cache is still removed.
    /// </summary>
    [Test]
    public void If_Removes_Throws_Cache_Is_Removed()
    {
        var cacheCleared = false;
        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Clear(It.IsAny<string>()))
            .Callback(() => cacheCleared = true);

        var defaultPolicy =
            new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions(), new SingleServerCacheVersionService(), Mock.Of<ICacheSyncService>());
        try
        {
            defaultPolicy.Delete(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
        }
        catch
        {
            // We need this catch or nunit throws up
        }
        finally
        {
            Assert.IsTrue(cacheCleared);
        }
    }
}
