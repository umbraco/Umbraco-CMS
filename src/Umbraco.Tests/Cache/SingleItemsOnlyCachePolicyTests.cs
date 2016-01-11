using System;
using System.Collections.Generic;
using System.Web.Caching;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class SingleItemsOnlyCachePolicyTests
    {
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

            var defaultPolicy = new SingleItemsOnlyRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => new[]
                {
                    new AuditItem(1, "blah", AuditType.Copy, 123),
                    new AuditItem(2, "blah2", AuditType.Copy, 123)
                });
            }

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

            var defaultPolicy = new SingleItemsOnlyRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.Get(1, o => new AuditItem(1, "blah", AuditType.Copy, 123));
            }
            Assert.IsTrue(isCached);
        }
    }
}