// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache
{
    [TestFixture]
    public class HttpRequestAppCacheTests : AppCacheTests
    {
        private HttpRequestAppCache _appCache;
        private HttpContext _httpContext;

        public override void Setup()
        {
            base.Setup();
            _httpContext = new DefaultHttpContext();
            _appCache = new HttpRequestAppCache(() => _httpContext.Items);
        }

        internal override IAppCache AppCache => _appCache;

        protected override int GetTotalItemCount => _httpContext.Items.Count;
    }
}
