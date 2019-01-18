using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class HttpRequestAppCacheTests : AppCacheTests
    {
        private HttpRequestAppCache _provider;
        private FakeHttpContextFactory _ctx;

        public override void Setup()
        {
            base.Setup();
            _ctx = new FakeHttpContextFactory("http://localhost/test");
            _provider = new HttpRequestAppCache(_ctx.HttpContext);
        }

        internal override IAppCache AppCache
        {
            get { return _provider; }
        }

        protected override int GetTotalItemCount
        {
            get { return _ctx.HttpContext.Items.Count; }
        }
    }

    [TestFixture]
    public class StaticAppCacheTests : AppCacheTests
    {
        private DictionaryCacheProvider _provider;

        public override void Setup()
        {
            base.Setup();
            _provider = new DictionaryCacheProvider();
        }

        internal override IAppCache AppCache
        {
            get { return _provider; }
        }

        protected override int GetTotalItemCount
        {
            get { return _provider.Items.Count; }
        }
    }
}
