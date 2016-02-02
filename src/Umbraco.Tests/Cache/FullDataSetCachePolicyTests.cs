using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public void Caches_Single()
        {
            var getAll = new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            };

            var isCached = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.InsertCacheItem(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback(() =>
                {
                    isCached = true;
                });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.Get(1, o => new AuditItem(1, "blah", AuditType.Copy, 123));
            }
            Assert.IsTrue(isCached);
        }

        [Test]
        public void Get_Single_From_Cache()
        {
            var getAll = new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            };

            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(new AuditItem(1, "blah", AuditType.Copy, 123));

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.Get(1, o => (AuditItem)null);
                Assert.IsNotNull(found);
            }
        }

        [Test]
        public void Get_All_Caches_Empty_List()
        {
            var getAll = new AuditItem[] {};

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
            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(() =>
            {
                //return null if this is the first pass
                return cached.Any() ? new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce) : null;
            });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] {}, o => getAll);
            }

            Assert.AreEqual(1, cached.Count);
            Assert.IsNotNull(list);

            //Do it again, ensure that its coming from the cache!
            defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => getAll);
            }

            Assert.AreEqual(1, cached.Count);
            Assert.IsNotNull(list);
        }

        [Test]
        public void Get_All_Caches_As_Single_List()
        {
            var getAll = new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            };

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
            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(new AuditItem[] { });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => getAll);
            }

            Assert.AreEqual(1, cached.Count);
            Assert.IsNotNull(list);
        }

        [Test]
        public void Get_All_Without_Ids_From_Cache()
        {
            var getAll = new[] { (AuditItem)null };

            var cache = new Mock<IRuntimeCacheProvider>();

            cache.Setup(x => x.GetCacheItem(It.IsAny<string>())).Returns(() => new DeepCloneableList<AuditItem>(ListCloneBehavior.CloneOnce)
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
            using (defaultPolicy)
            {
                var found = defaultPolicy.GetAll(new object[] { }, o => getAll);
                Assert.AreEqual(2, found.Length);
            }
        }

        [Test]
        public void If_CreateOrUpdate_Throws_Cache_Is_Removed()
        {
            var getAll = new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            };

            var cacheCleared = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.ClearCacheItem(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
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
            var getAll = new[]
            {
                new AuditItem(1, "blah", AuditType.Copy, 123),
                new AuditItem(2, "blah2", AuditType.Copy, 123)
            };

            var cacheCleared = false;
            var cache = new Mock<IRuntimeCacheProvider>();
            cache.Setup(x => x.ClearCacheItem(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new FullDataSetRepositoryCachePolicy<AuditItem, object>(cache.Object, item => item.Id, () => getAll, false);
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