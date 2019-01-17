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

        [Test]
        public void Caches_Single()
        {
            var isCached = false;
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback(() =>
                {
                    isCached = true;
                });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var unused = defaultPolicy.Get(1, id => new AuditItem(1, AuditType.Copy, 123, "test", "blah"), o => null);
            Assert.IsTrue(isCached);
        }

        [Test]
        public void Get_Single_From_Cache()
        {
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.Get(It.IsAny<string>())).Returns(new AuditItem(1, AuditType.Copy, 123, "test", "blah"));

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var found = defaultPolicy.Get(1, id => null, ids => null);
            Assert.IsNotNull(found);
        }

        [Test]
        public void Caches_Per_Id_For_Get_All()
        {
            var cached = new List<string>();
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.Insert(It.IsAny<string>(), It.IsAny<Func<object>>(), It.IsAny<TimeSpan?>(), It.IsAny<bool>(),
                It.IsAny<CacheItemPriority>(), It.IsAny<CacheItemRemovedCallback>(), It.IsAny<string[]>()))
                .Callback((string cacheKey, Func<object> o, TimeSpan? t, bool b, CacheItemPriority cip, CacheItemRemovedCallback circ, string[] s) =>
                {
                    cached.Add(cacheKey);
                });
            cache.Setup(x => x.SearchByKey(It.IsAny<string>())).Returns(new AuditItem[] {});

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var unused = defaultPolicy.GetAll(new object[] {}, ids => new[]
                    {
                        new AuditItem(1, AuditType.Copy, 123, "test", "blah"),
                        new AuditItem(2, AuditType.Copy, 123, "test", "blah2")
                    });

            Assert.AreEqual(2, cached.Count);
        }

        [Test]
        public void Get_All_Without_Ids_From_Cache()
        {
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.SearchByKey(It.IsAny<string>())).Returns(new[]
            {
                new AuditItem(1, AuditType.Copy, 123, "test", "blah"),
                new AuditItem(2, AuditType.Copy, 123, "test", "blah2")
            });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());

            var found = defaultPolicy.GetAll(new object[] {}, ids => new[] { (AuditItem)null });
            Assert.AreEqual(2, found.Length);
        }

        [Test]
        public void If_CreateOrUpdate_Throws_Cache_Is_Removed()
        {
            var cacheCleared = false;
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.Clear(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());
            try
            {
                defaultPolicy.Update(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
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
            var cache = new Mock<IAppPolicedCache>();
            cache.Setup(x => x.Clear(It.IsAny<string>()))
                .Callback(() =>
                {
                    cacheCleared = true;
                });

            var defaultPolicy = new DefaultRepositoryCachePolicy<AuditItem, object>(cache.Object, DefaultAccessor, new RepositoryCachePolicyOptions());
            try
            {
                defaultPolicy.Delete(new AuditItem(1, AuditType.Copy, 123, "test", "blah"), item => throw new Exception("blah!"));
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
