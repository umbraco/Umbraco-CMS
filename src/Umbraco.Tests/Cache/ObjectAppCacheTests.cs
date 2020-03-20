﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Cache;

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
            _provider = new ObjectCacheAppCache();
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
