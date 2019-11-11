using System;
using System.Diagnostics;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;
using Umbraco.Web.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class WebCachingAppCacheTests : RuntimeAppCacheTests
    {
        private WebCachingAppCache _appCache;

        protected override int GetTotalItemCount => HttpRuntime.Cache.Count;

        public override void Setup()
        {
            base.Setup();
            var typeFinder = new TypeFinder(Mock.Of<ILogger>());
            _appCache = new WebCachingAppCache(HttpRuntime.Cache, typeFinder);
        }

        internal override IAppCache AppCache => _appCache;

        internal override IAppPolicyCache AppPolicyCache => _appCache;

        [Test]
        public void DoesNotCacheExceptions()
        {
            string value;
            Assert.Throws<Exception>(() => { value = (string)_appCache.Get("key", () => GetValue(1)); });
            Assert.Throws<Exception>(() => { value = (string)_appCache.Get("key", () => GetValue(2)); });

            // does not throw
            value = (string)_appCache.Get("key", () => GetValue(3));
            Assert.AreEqual("succ3", value);

            // cache
            value = (string)_appCache.Get("key", () => GetValue(4));
            Assert.AreEqual("succ3", value);
        }

        private static string GetValue(int i)
        {
            Debug.Print("get" + i);
            if (i < 3)
                throw new Exception("fail");
            return "succ" + i;
        }
    }
}
