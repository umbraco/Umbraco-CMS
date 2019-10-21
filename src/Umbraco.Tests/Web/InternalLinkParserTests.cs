using System;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Templates;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class InternalLinkParserTests
    {
        [TestCase("", "")]
        [TestCase("hello href=\"{localLink:1234}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document/9931BDE0AAC34BABB838909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://media/9931BDE0AAC34BABB838909A7B47570E}\" world ", "hello href=\"/media/1001/my-image.jpg\" world ")]
        //this one has an invalid char so won't match
        [TestCase("hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"{localLink:umb^://document/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ")]
        [TestCase("hello href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"#\" world ")]
        public void ParseLocalLinks(string input, string result)
        {
            var serviceCtxMock = new TestObjects(null).GetServiceContextMock();

            //setup a mock url provider which we'll use for testing
            var testUrlProvider = new Mock<IUrlProvider>();
            testUrlProvider
                .Setup(x => x.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(UrlInfo.Url("/my-test-url"));

            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            var contentType = new PublishedContentType(666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var publishedContent = new Mock<IPublishedContent>();
            publishedContent.Setup(x => x.Id).Returns(1234);
            publishedContent.Setup(x => x.ContentType).Returns(contentType);
            var contentCache = new Mock<IPublishedContentCache>();
            contentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(publishedContent.Object);
            contentCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(publishedContent.Object);
            var mediaType = new PublishedContentType(777, "image", PublishedItemType.Media, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var media = new Mock<IPublishedContent>();
            media.Setup(x => x.Url).Returns("/media/1001/my-image.jpg");
            media.Setup(x => x.ContentType).Returns(mediaType);
            var mediaCache = new Mock<IPublishedMediaCache>();
            mediaCache.Setup(x => x.GetById(It.IsAny<int>())).Returns(media.Object);
            mediaCache.Setup(x => x.GetById(It.IsAny<Guid>())).Returns(media.Object);
            var snapshot = new Mock<IPublishedSnapshot>();
            snapshot.Setup(x => x.Content).Returns(contentCache.Object);
            snapshot.Setup(x => x.Media).Returns(mediaCache.Object);
            var snapshotService = new Mock<IPublishedSnapshotService>();
            snapshotService.Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(snapshot.Object);                        
            var mediaUrlProvider = new Mock<IMediaUrlProvider>();
            mediaUrlProvider.Setup(x => x.GetMediaUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<string>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(UrlInfo.Url("/media/1001/my-image.jpg"));

            var umbracoContextAccessor = new TestUmbracoContextAccessor();

            var umbracoContextFactory = new UmbracoContextFactory(
                umbracoContextAccessor,
                snapshotService.Object,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "Auto")),
                globalSettings,
                new UrlProviderCollection(new[] { testUrlProvider.Object }),
                new MediaUrlProviderCollection(new[] { mediaUrlProvider.Object }),
                Mock.Of<IUserService>());

            using (var reference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>()))
            {
                var linkParser = new InternalLinkParser(umbracoContextAccessor);

                var output = linkParser.ParseInternalLinks(input);

                Assert.AreEqual(result, output);
            }
        }
    }
}
