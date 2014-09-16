using NUnit.Framework;
using Umbraco.Core.Cache;
using umbraco.presentation.webservices;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class CacheRefresherTests
    {
        [TestCase("", "123456", "testmachine", true)] //empty hash will continue
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "123456", "testmachine", false)] //match, don't continue
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "12345", "testmachine", true)]
        [TestCase("2c8aabac795d189d444a9cdc6e2a1819", "123456", "testmachin", true)]
        [TestCase("2c8aabac795d189d444a9cdc6e2a181", "123456", "testmachine", true)]
        public void Continue_Refreshing_For_Request(string hash, string appDomainAppId, string machineName, bool expected)
        {
            var refresher = new CacheRefresher();
            Assert.AreEqual(expected, refresher.ContinueRefreshingForRequest(hash, appDomainAppId, machineName));
        }

    }

    [TestFixture]
    public class HttpRequestCacheProviderTests : CacheProviderTests
    {
        private HttpRequestCacheProvider _provider;
        private FakeHttpContextFactory _ctx;

        public override void Setup()
        {
            base.Setup();
            _ctx = new FakeHttpContextFactory("http://localhost/test");
            _provider = new HttpRequestCacheProvider(_ctx.HttpContext);
        }

        internal override ICacheProvider Provider
        {
            get { return _provider; }
        }

        protected override int GetTotalItemCount
        {
            get { return _ctx.HttpContext.Items.Count; }
        }
    }

    [TestFixture]
    public class StaticCacheProviderTests : CacheProviderTests
    {
        private StaticCacheProvider _provider;        

        public override void Setup()
        {
            base.Setup();
            _provider = new StaticCacheProvider();
        }

        internal override ICacheProvider Provider
        {
            get { return _provider; }
        }

        protected override int GetTotalItemCount
        {
            get { return _provider.StaticCache.Count; }
        }
    }
}