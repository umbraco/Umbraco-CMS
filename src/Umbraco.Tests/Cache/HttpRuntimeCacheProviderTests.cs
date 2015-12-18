using System;
using System.Web;
using NUnit.Framework;
using Umbraco.Core.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class HttpRuntimeCacheProviderTests : RuntimeCacheProviderTests
    {
        private HttpRuntimeCacheProvider _provider;

        protected override int GetTotalItemCount
        {
            get { return HttpRuntime.Cache.Count; }
        }

        public override void Setup()
        {
            base.Setup();
            _provider = new HttpRuntimeCacheProvider(HttpRuntime.Cache);
        }

        internal override ICacheProvider Provider
        {
            get { return _provider; }
        }

        internal override IRuntimeCacheProvider RuntimeProvider
        {
            get { return _provider; }
        }

        [Test]
        public void DoesNotCacheExceptions()
        {
            string value;
            Assert.Throws<Exception>(() => { value = (string)_provider.GetCacheItem("key", () => GetValue(1)); });
            Assert.Throws<Exception>(() => { value = (string)_provider.GetCacheItem("key", () => GetValue(2)); });

            // does not throw
            value = (string)_provider.GetCacheItem("key", () => GetValue(3));
            Assert.AreEqual("succ3", value);

            // cache
            value = (string)_provider.GetCacheItem("key", () => GetValue(4));
            Assert.AreEqual("succ3", value);
        }

        private static string GetValue(int i)
        {
            Console.WriteLine("get" + i);
            if (i < 3)
                throw new Exception("fail");
            return "succ" + i;
        }
    }
}