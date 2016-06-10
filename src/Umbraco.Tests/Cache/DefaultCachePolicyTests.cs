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
    public class DefaultCachePolicyTests
    {
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

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.Get(1, o => new AuditItem(1, "blah", AuditType.Copy, 123));
            }
            Assert.IsTrue(isCached);
        }

        [Test]
        public void Get_Single_From_Cache()
        {
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(new AuditItem(1, "blah", AuditType.Copy, 123));

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.Get(1, o => (AuditItem) null);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Caches_Per_Id_For_Get_All()
        {
            var cached = new List<string>();
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.InsertCacheItem(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, CacheItemPriority cip, CacheItemRemovedCallback circ, string[] s) =>
                {
                    cached.Add(cacheKey);
                });
            cache.Setup(x => x.GetCacheItemsByKeySearch(It.IsAny<string>())).Returns(new AuditItem[] {});

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] {}, o => new[]
                {
                    new AuditItem(1, "blah", AuditType.Copy, 123),
                    new AuditItem(2, "blah2", AuditType.Copy, 123)
                });
            }

            Assert.AreEqual(2, cached.Count);
        }

        [Test]
        public void Get_All_Without_Ids_From_Cache()
        {
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.GetCacheItemsByKeySearch(It.IsAny<string>())).Returns(new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] {}, o => new[] {(AuditItem) null});
                Assert.AreEqual(2, found.Length);
            }
        }

        [Test]
        public void If_CreateOrUpdate_Throws_Cache_Is_Removed()
        {
            var cacheCleared = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.ClearCacheItem(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            try
            {
                using (defaultPolicy)
                {
                    defaultPolicy.CreateOrUpdate(new AuditItem(1, "blah", AuditType.Copy, 123), item =>
                    {
                        throw new Exception("blah!");
                    });
                }
            }
            catch
            {
                //we need this catch or nunit throw up
            }
            finally
            {
                Assert.IsTrue(cacheCleared);
            }
        }

        [Test]
        public void If_Removes_Throws_Cache_Is_Removed()
        {
            var cacheCleared = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.ClearCacheItem(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, new RepositoryCachePolicyOptions());
            try
            {
                using (defaultPolicy)
                {
                    defaultPolicy.Remove(new AuditItem(1, "blah", AuditType.Copy, 123), item =>
                    {
                        throw new Exception("blah!");
                    });
                }
            }
            catch
            {
                //we need this catch or nunit throw up
            }
            finally
            {
                Assert.IsTrue(cacheCleared);
            }
        }
    }
}