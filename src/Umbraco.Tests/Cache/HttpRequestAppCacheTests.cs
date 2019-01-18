using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class HttpRequestAppCacheTests : AppCacheTests
    {
        private HttpRequestAppCache _appCache;
        private FakeHttpContextFactory _ctx;

        public override void Setup()
        {
            base.Setup();
            _ctx = new FakeHttpContextFactory("http://localhost/test");
            _appCache = new HttpRequestAppCache(_ctx.HttpContext);
        }

        internal override IAppCache AppCache
        {
            get { return _appCache; }
        }

        protected override int GetTotalItemCount
        {
            get { return _ctx.HttpContext.Items.Count; }
        }
    }

    [TestFixture]
    public class DictionaryAppCacheTests : AppCacheTests
    {
        private DictionaryAppCache _appCache;

        public override void Setup()
        {
            base.Setup();
            _appCache = new DictionaryAppCache();
        }

        internal override IAppCache AppCache
        {
            get { return _appCache; }
        }

        protected override int GetTotalItemCount
        {
            get { return _appCache.Items.Count; }
        }
    }
}
