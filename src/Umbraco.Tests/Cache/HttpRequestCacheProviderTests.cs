using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Cache
{
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