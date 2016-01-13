using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Caching;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Collections;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class FullDataSetCachePolicyTests
    {
        [Test]
        public void Get_All_Caches_As_Single_List()
        {
            var cached = new List<string>();
            IList list = null;

            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.InsertCacheItem(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, CacheItemPriority cip, CacheItemRemovedCallback circ, string[] s) =>
                {
                    cached.Add(cacheKey);

                    list = o() as IList;
                });
            cache.Setup(x => x.GetCacheItemsByKeySearch(It.IsAny<string>())).Returns(new AuditItem[] { });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => new[]
                {
                    new AuditItem(1, "blah", AuditType.Copy, 123),
                    new AuditItem(2, "blah2", AuditType.Copy, 123)
                });
            }

            Assert.AreEqual(1, cached.Count);
            Assert.IsNotNull(list);
        }

        [Test]
        public void Get_All_Without_Ids_From_Cache()
        {           
            var cache = new Mock<IRuntimeCacheProvider>();

            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(() => new DeepCloneableList<AuditItem>
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => new[] { (AuditItem)null });
                Assert.AreEqual(2, found.Length);
            }
        }
    }
}