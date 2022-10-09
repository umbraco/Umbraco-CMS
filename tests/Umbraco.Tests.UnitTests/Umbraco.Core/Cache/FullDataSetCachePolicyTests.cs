// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Collections;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

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
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<string[]>()))
            .Callback(() => isCached = true);

        var policy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        var unused = policy.Get(1, id => new AuditItem(1, AuditType.Copy, 123, "test", "blah"), ids => getAll);
        Assert.IsTrue(isCached);
    }

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
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        var found = defaultPolicy.Get(1, id => null, ids => getAll);
        Assert.IsNotNull(found);
    }

    [Test]
    public void Get_All_Caches_Empty_List()
    {
        var getAll = new AuditItem[] { };

        var cached = new List<string>();

        IList list = null;

        var cache = new Mock<IAppPolicyCache>();
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<string[]>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, string[] s) =>
            {
                cached.Add(cacheKey);

                list = o() as IList;
            });

        // Return null if this is the first pass.
        cache.Setup(x => x.Get(It.IsAny<string>()))
            .Returns(() => cached.Any() ? new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce) : null);

        var policy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        var found = policy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);

        // Do it again, ensure that its coming from the cache!
        policy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        found = policy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

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
        cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(), It.IsAny<string[]>()))
            .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, string[] s) =>
            {
                cached.Add(cacheKey);

                list = o() as IList;
            });
        cache.Setup(x => x.Get(It.IsAny<string>())).Returns(new AuditItem[] { });

        var defaultPolicy =
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        var found = defaultPolicy.GetAll(new object[] { }, ids => getAll);

        Assert.AreEqual(1, cached.Count);
        Assert.IsNotNull(list);
    }

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
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);

        var found = defaultPolicy.GetAll(new object[] { }, ids => getAll);
        Assert.AreEqual(2, found.Length);
    }

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
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);
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
            new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, item => item.Id, false);
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
}
