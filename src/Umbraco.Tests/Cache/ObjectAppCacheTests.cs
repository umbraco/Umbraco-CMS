using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Logging;

namespace Umbraco.Tests.Cache
{
    [TestFixture]
    public class ObjectAppCacheTests : RuntimeAppCacheTests
    {
        private ObjectCacheAppCache _provider;

        protected override int GetTotalItemCount
        {
            get { return _provider.MemoryCache.Count(); }
        }

        public override void Setup()
        {
            base.Setup();
            var typeFinder = new TypeFinder(Mock.Of<ILogger>());
            _provider = new ObjectCacheAppCache(typeFinder);
        }

        internal override IAppCache AppCache
        {
            get { return _provider; }
        }

        internal override IAppPolicyCache AppPolicyCache
        {
            get { return _provider; }
        }
    }
}
