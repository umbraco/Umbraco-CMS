// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Cache;

/// <summary>
/// Contains unit tests for the <see cref="HttpContextRequestAppCache"/> class, verifying caching behavior related to <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
/// </summary>
[TestFixture]
public class HttpContextRequestAppCacheTests : AppCacheTests
{
    /// <summary>
    /// Sets up the test environment for <see cref="HttpContextRequestAppCacheTests"/> by initializing
    /// a mock <see cref="IHttpContextAccessor"/> with a default <see cref="DefaultHttpContext"/> and
    /// creating a new <see cref="HttpContextRequestAppCache"/> instance for use in tests.
    /// </summary>
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
