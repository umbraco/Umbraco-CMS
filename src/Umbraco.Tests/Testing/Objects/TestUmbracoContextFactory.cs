using Moq;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Common;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Testing.Objects
{
    /// <summary>
    /// Simplify creating test UmbracoContext's
    /// </summary>
    public class TestUmbracoContextFactory
    {
        public static IUmbracoContextFactory Create(GlobalSettings globalSettings = null,
            IUmbracoContextAccessor umbracoContextAccessor = null,
            IHttpContextAccessor httpContextAccessor = null,
            IPublishedUrlProvider publishedUrlProvider = null)
        {
            if (globalSettings == null) globalSettings = new GlobalSettings();
            if (umbracoContextAccessor == null) umbracoContextAccessor = new TestUmbracoContextAccessor();
            if (httpContextAccessor == null) httpContextAccessor = TestHelper.GetHttpContextAccessor();
            if (publishedUrlProvider == null) publishedUrlProvider = TestHelper.GetPublishedUrlProvider();

            var contentCache = new Mock<IPublishedContentCache>();
            var mediaCache = new Mock<IPublishedMediaCache>();
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(x => x.Content).Returns(contentCache.Object);
            snapshot.Setup(x => x.Media).Returns(mediaCache.Object);
            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(snapshot.Object);



            var umbracoContextFactory = new UmbracoContextFactory(
                umbracoContextAccessor,
                snapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                globalSettings,
                Mock.Of<IUserService>(),
                TestHelper.GetHostingEnvironment(),
                TestHelper.UriUtility,
                httpContextAccessor,
                new AspNetCookieManager(httpContextAccessor));

            return umbracoContextFactory;
        }
    }
}
