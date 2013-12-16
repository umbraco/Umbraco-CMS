using System;
using System.Threading;
using System.Web.Caching;
using NUnit.Framework;
using Umbraco.Core.Cache;

namespace Umbraco.Tests.Cache
{
    public abstract class RuntimeCacheProviderTests : CacheProviderTests
    {

        internal abstract IRuntimeCacheProvider RuntimeProvider { get; }
        

        [Test]
        public void Can_Add_And_Expire_Struct_Strongly_Typed_With_Null()
        {
            var now = DateTime.Now;
            RuntimeProvider.InsertCacheItem("DateTimeTest", CacheItemPriority.Default, new TimeSpan(0, 0, 0, 0, 200), () => now);
            Assert.AreEqual(now, Provider.GetCacheItem<DateTime>("DateTimeTest"));
            Assert.AreEqual(now, Provider.GetCacheItem<DateTime?>("DateTimeTest"));
            
            Thread.Sleep(300); //sleep longer than the cache expiration

            Assert.AreEqual(default(DateTime), Provider.GetCacheItem<DateTime>("DateTimeTest"));
            Assert.AreEqual(null, Provider.GetCacheItem<DateTime?>("DateTimeTest"));
        }

    }
}