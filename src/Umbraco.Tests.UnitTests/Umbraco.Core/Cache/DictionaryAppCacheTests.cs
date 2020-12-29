// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Core.Cache;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Cache
{
    [TestFixture]
    public class DictionaryAppCacheTests : AppCacheTests
    {
        private DictionaryAppCache _appCache;

        public override void Setup()
        {
            base.Setup();
            _appCache = new DictionaryAppCache();
        }

        internal override IAppCache AppCache => _appCache;

        protected override int GetTotalItemCount => _appCache.Count;
    }
}
