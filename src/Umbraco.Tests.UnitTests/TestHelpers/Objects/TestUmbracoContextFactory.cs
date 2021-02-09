// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Web;
using Umbraco.Core.Security;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Common.AspNetCore;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.UnitTests.TestHelpers.Objects
{
    /// <summary>
    /// Simplify creating test UmbracoContext's
    /// </summary>
    public class TestUmbracoContextFactory
    {
        public static IUmbracoContextFactory Create(
            GlobalSettings globalSettings = null,
            IUmbracoContextAccessor umbracoContextAccessor = null,
            IHttpContextAccessor httpContextAccessor = null,
            IPublishedUrlProvider publishedUrlProvider = null)
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

            var contentCache = new Mock<IPublishedContentCache>();
            var mediaCache = new Mock<IPublishedMediaCache>();
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(x => x.Content).Returns(contentCache.Object);
            snapshot.Setup(x => x.Media).Returns(mediaCache.Object);
            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(snapshot.Object);

            IHostingEnvironment hostingEnvironment = TestHelper.GetHostingEnvironment();

            var backofficeSecurityAccessorMock = new Mock<IBackOfficeSecurityAccessor>();
            backofficeSecurityAccessorMock.Setup(x => x.BackOfficeSecurity).Returns(Mock.Of<IBackOfficeSecurity>());

            var umbracoContextFactory = new UmbracoContextFactory(
                umbracoContextAccessor,
                snapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                new UmbracoRequestPaths(Options.Create(globalSettings), hostingEnvironment),
                hostingEnvironment,
                new UriUtility(hostingEnvironment),
                new AspNetCoreCookieManager(httpContextAccessor),
                Mock.Of<IRequestAccessor>(),
                backofficeSecurityAccessorMock.Object);

            return umbracoContextFactory;
        }
    }
}
