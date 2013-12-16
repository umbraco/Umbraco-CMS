using System.Web.UI;
using NUnit.Framework;
using Umbraco.Core.Cache;
using umbraco;

namespace Umbraco.Tests.Cache
{
    public abstract class CacheProviderTests
    {
        internal abstract ICacheProvider Provider { get; }
        protected abstract int GetTotalItemCount { get; }

        [SetUp]
        public virtual void Setup()
        {

        }

        [TearDown]
        public virtual void TearDown()
        {
            Provider.ClearAllCache();
        }

        [Test]
        public void Can_Remove_By_Type_Name()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new MacroCacheContent(new LiteralControl(), "Test4");
            Provider.GetCacheItem("Test1", () => cacheContent1);
            Provider.GetCacheItem("Test2", () => cacheContent2);
            Provider.GetCacheItem("Test3", () => cacheContent3);
            Provider.GetCacheItem("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.ClearCacheObjectTypes("umbraco.MacroCacheContent");

            Assert.AreEqual(0, GetTotalItemCount);
        }

        [Test]
        public void Can_Remove_By_Strong_Type()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new MacroCacheContent(new LiteralControl(), "Test4");
            Provider.GetCacheItem("Test1", () => cacheContent1);
            Provider.GetCacheItem("Test2", () => cacheContent2);
            Provider.GetCacheItem("Test3", () => cacheContent3);
            Provider.GetCacheItem("Test4", () => cacheContent4);

            Assert.AreEqual(4, GetTotalItemCount);

            Provider.ClearCacheObjectTypes<MacroCacheContent>();

            Assert.AreEqual(0, GetTotalItemCount);
        }
    }
}