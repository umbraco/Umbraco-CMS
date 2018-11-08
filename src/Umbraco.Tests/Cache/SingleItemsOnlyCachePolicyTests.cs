using System;
using System.Collections.Generic;
using System.Web.Caching;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;
using Umbraco.Core.Scoping;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class SingleItemsOnlyCachePolicyTests
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
        public void Get_All_Doesnt_Cache()
        {
            var cached = new List<string>();
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.InsertCacheItem(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, CacheItemPriority cip, CacheItemRemovedCallback circ, string[] s) =>
                {
                    cached.Add(cacheKey);
                });
            cache.Setup(x => x.GetCacheItemsByKeySearch(It.IsAny<string>())).Returns(new AuditItem[] { });

            var defaultPolicy = new SingleItemsOnlyRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var unused = defaultPolicy.GetAll(new object[] { }, ids => new[]
                    {
                        new AuditItem(1, AuditType.Copy, 123, "test", "blah"),
                        new AuditItem(2, AuditType.Copy, 123, "test", "blah2")
                    });

            Assert.AreEqual(0, cached.Count);
        }

        [Test]
        public void Caches_Single()
        {
            var isCached = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.InsertCacheItem(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback(() =>
                {
                    isCached = true;
                });

            var defaultPolicy = new SingleItemsOnlyRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var unused = defaultPolicy.Get(1, id => new AuditItem(1, AuditType.Copy, 123, "test", "blah"), ids => null);
            Assert.IsTrue(isCached);
        }
    }
}
