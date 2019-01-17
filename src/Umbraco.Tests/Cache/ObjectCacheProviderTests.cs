using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class ObjectCacheProviderTests : RuntimeCacheProviderTests
    {
        private ObjectCacheAppCache _provider;

        protected override int GetTotalItemCount
        {
            get { return _provider.MemoryCache.Count(); }
        }

        public override void Setup()
        {
            base.Setup();
            _provider = new ObjectCacheAppCache();
        }

        internal override IAppCache Provider
        {
            get { return _provider; }
        }

        internal override IAppPolicedCache RuntimeProvider
        {
            get { return _provider; }
        }
    }
}
