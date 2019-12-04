using System;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
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
    public class TemplateUtilitiesTests
    {
        [SetUp]
        public void SetUp()
        {
            UdiParser.ResetUdiTypes();
        }


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
            //setup a mock url provider which we'll use for testing
            var testUrlProvider = new Mock<IUrlProvider>();
            testUrlProvider
                .Setup(x => x.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns((UmbracoContext umbCtx, IPublishedContent content, UrlMode mode, string culture, Uri url) => UrlInfo.Url("/my-test-url"));

            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            var contentType = new PublishedContentType(666, "alias", PublishedItemType.Content, Enumerable.Empty<string>(), Enumerable.Empty<PublishedPropertyType>(), ContentVariation.Nothing);
            var publishedContent = Mock.Of<IPublishedContent>();
            Mock.Get(publishedContent).Setup(x => x.Id).Returns(1234);
            Mock.Get(publishedContent).Setup(x => x.ContentType).Returns(contentType);
            var contentCache = Mock.Of<IPublishedContentCache>();
            Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<int>())).Returns(publishedContent);
            Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<Guid>())).Returns(publishedContent);
            var snapshot = Mock.Of<IPublishedSnapshot>();
            Mock.Get(snapshot).Setup(x => x.Content).Returns(contentCache);
            var snapshotService = Mock.Of<IPublishedSnapshotService>();
            Mock.Get(snapshotService).Setup(x => x.CreatePublishedSnapshot(It.IsAny<string>())).Returns(snapshot);
            var media = Mock.Of<IPublishedContent>();
            Mock.Get(media).Setup(x => x.Url).Returns("/media/1001/my-image.jpg");
            var mediaCache = Mock.Of<IPublishedMediaCache>();
            Mock.Get(mediaCache).Setup(x => x.GetById(It.IsAny<Guid>())).Returns(media);

            var umbracoContextAccessor = new TestUmbracoContextAccessor();
            var umbracoContextFactory = new UmbracoContextFactory(
                umbracoContextAccessor,
                snapshotService,
                new TestVariationContextAccessor(),
                new TestDefaultCultureAccessor(),
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "Auto")),
                globalSettings,
                new UrlProviderCollection(new[] { testUrlProvider.Object }),
                new MediaUrlProviderCollection(Enumerable.Empty<IMediaUrlProvider>()),
                Mock.Of<IUserService>(),
                TestHelper.IOHelper);

            using (var reference = umbracoContextFactory.EnsureUmbracoContext(Mock.Of<HttpContextBase>()))
            {
                var output = TemplateUtilities.ParseInternalLinks(input, reference.UmbracoContext.UrlProvider, mediaCache);

                Assert.AreEqual(result, output);
            }
        }
    }
}
