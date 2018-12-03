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
        private ObjectCacheRuntimeCacheProvider _provider;

        protected override int GetTotalItemCount
        {
            get { return _provider.MemoryCache.Count(); }
        }

        public override void Setup()
        {
            base.Setup();
            _provider = new ObjectCacheRuntimeCacheProvider();
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
