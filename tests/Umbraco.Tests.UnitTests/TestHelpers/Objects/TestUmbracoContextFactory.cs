// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Web.Common.AspNetCore;
using Umbraco.Cms.Web.Common.UmbracoContext;

namespace Umbraco.Cms.Tests.UnitTests.TestHelpers.Objects;

/// <summary>
///     Simplify creating test UmbracoContext's
/// </summary>
public class TestUmbracoContextFactory
{
    public static IUmbracoContextFactory Create(
        GlobalSettings globalSettings = null,
        IUmbracoContextAccessor umbracoContextAccessor = null,
        IHttpContextAccessor httpContextAccessor = null,
        IPublishedUrlProvider publishedUrlProvider = null,
        UmbracoRequestPathsOptions umbracoRequestPathsOptions = null)
    {
        if (globalSettings == null)
        {
            globalSettings = new GlobalSettings();
        }

        if (umbracoContextAccessor == null)
        {
            umbracoContextAccessor = new TestUmbracoContextAccessor();
        }

        if (httpContextAccessor == null)
        {
            httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        }

        if (publishedUrlProvider == null)
        {
            publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        }

        if (umbracoRequestPathsOptions == null)
        {
            umbracoRequestPathsOptions = new UmbracoRequestPathsOptions();
        }

        var contentCache = new Mock<IPublishedContentCache>();
        var mediaCache = new Mock<IPublishedMediaCache>();
        var cacheManager = new Mock<ICacheManager>();
        cacheManager.Setup(x => x.Content).Returns(contentCache.Object);
        cacheManager.Setup(x => x.Media).Returns(mediaCache.Object);

        var hostingEnvironment = TestHelper.GetHostingEnvironment();

        var umbracoContextFactory = new UmbracoContextFactory(
            umbracoContextAccessor,
            new UmbracoRequestPaths(Options.Create(globalSettings), hostingEnvironment, Options.Create(umbracoRequestPathsOptions)),
            hostingEnvironment,
            new UriUtility(hostingEnvironment),
            new AspNetCoreCookieManager(httpContextAccessor),
            httpContextAccessor,
            Mock.Of<IWebProfilerService>(),
            cacheManager.Object);

        return umbracoContextFactory;
    }
}
