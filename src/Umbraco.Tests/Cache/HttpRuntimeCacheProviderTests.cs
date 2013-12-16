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
    }
}