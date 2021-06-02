using Moq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
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
        public static IUmbracoContextFactory Create(IGlobalSettings globalSettings = null, IUrlProvider urlProvider = null,
            IMediaUrlProvider mediaUrlProvider = null,
            IUmbracoContextAccessor umbracoContextAccessor = null)
        {
            if (globalSettings == null) globalSettings = SettingsForTests.GenerateMockGlobalSettings();
            if (urlProvider == null) urlProvider = Mock.Of<IUrlProvider>();
            if (mediaUrlProvider == null) mediaUrlProvider = Mock.Of<IMediaUrlProvider>();
            if (umbracoContextAccessor == null) umbracoContextAccessor = new TestUmbracoContextAccessor();

            var contentCache = new Mock<IPublishedContentCache>();
            var mediaCache = new Mock<IPublishedMediaCache>();            
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(x => x.Content).Returns(contentCache.Object);
            snapshot.Setup(x => x.Media).Returns(mediaCache.Object);
            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(snapshot.Object);
            var urlProviders = new[] { urlProvider };
            var mediaUrlProviders = new[] { mediaUrlProvider };
            var umbracoSettings = Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "Auto"));
            var umbracoContextFactory = new UmbracoContextFactory(
                umbracoContextAccessor,
                snapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(), umbracoSettings
                ,
                globalSettings,
                new UrlProviderCollection(urlProviders),
                new MediaUrlProviderCollection(mediaUrlProviders),
                Mock.Of<IUserService>(),
                 new ContextUrlProviderFactory(
                    umbracoSettings.WebRouting,
                     urlProviders,
                     mediaUrlProviders,
                     new TestVariationContextAccessor()
                    ));

            return umbracoContextFactory;
        }
    }
}
