// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

[TestFixture]
public class HttpContextRequestAppCacheTests : AppCacheTests
{
    public override void Setup()
    {
        base.Setup();
        var httpContext = new DefaultHttpContext();

        _httpContextAccessor = Mock.Of<IHttpContextAccessor>(x => x.HttpContext == httpContext);
        _appCache = new HttpContextRequestAppCache(_httpContextAccessor);
    }

    private HttpContextRequestAppCache _appCache;
    private IHttpContextAccessor _httpContextAccessor;

    internal override IAppCache AppCache => _appCache;

    protected override int GetTotalItemCount => _httpContextAccessor.HttpContext.Items.Count;
}
