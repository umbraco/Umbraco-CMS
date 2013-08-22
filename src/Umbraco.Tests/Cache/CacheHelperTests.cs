using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using NUnit.Framework;
using Umbraco.Core;
using umbraco;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheHelperTests
    {

        private CacheHelper _helper;

        [SetUp]
        public void Setup()
        {
            _helper = new CacheHelper(HttpRuntime.Cache);
        }

        [TearDown]
        public void TearDown()
        {
            _helper.ClearAllCache();
        }

        [Test]
        public void Can_Remove_By_Type_Name()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new MacroCacheContent(new LiteralControl(), "Test4");
            _helper.InsertCacheItem("Test1", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent1);
            _helper.InsertCacheItem("Test2", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent2);
            _helper.InsertCacheItem("Test3", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent3);
            _helper.InsertCacheItem("Test4", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent4);

            Assert.AreEqual(4, HttpRuntime.Cache.Count);

            _helper.ClearCacheObjectTypes("umbraco.MacroCacheContent");

            Assert.AreEqual(0, HttpRuntime.Cache.Count);
        }

        [Test]
        public void Can_Remove_By_Strong_Type()
        {
            var cacheContent1 = new MacroCacheContent(new LiteralControl(), "Test1");
            var cacheContent2 = new MacroCacheContent(new LiteralControl(), "Test2");
            var cacheContent3 = new MacroCacheContent(new LiteralControl(), "Test3");
            var cacheContent4 = new MacroCacheContent(new LiteralControl(), "Test4");
            _helper.InsertCacheItem("Test1", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent1);
            _helper.InsertCacheItem("Test2", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent2);
            _helper.InsertCacheItem("Test3", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent3);
            _helper.InsertCacheItem("Test4", CacheItemPriority.Default, new TimeSpan(0, 0, 60), () => cacheContent4);

            Assert.AreEqual(4, HttpRuntime.Cache.Count);

            _helper.ClearCacheObjectTypes<MacroCacheContent>();

            Assert.AreEqual(0, HttpRuntime.Cache.Count);
        }

        [Test]
        public void Can_Add_Remove_Struct_Strongly_Typed_With_Null()
        {
            var now = DateTime.Now;
            _helper.InsertCacheItem("DateTimeTest", CacheItemPriority.Default, new TimeSpan(0, 0, 0, 0, 200), () => now);
            Assert.AreEqual(now, _helper.GetCacheItem<DateTime>("DateTimeTest"));
            Assert.AreEqual(now, _helper.GetCacheItem<DateTime?>("DateTimeTest"));
            
            Thread.Sleep(300); //sleep longer than the cache expiration

            Assert.AreEqual(default(DateTime), _helper.GetCacheItem<DateTime>("DateTimeTest"));
            Assert.AreEqual(null, _helper.GetCacheItem<DateTime?>("DateTimeTest"));
        }

    }
}
