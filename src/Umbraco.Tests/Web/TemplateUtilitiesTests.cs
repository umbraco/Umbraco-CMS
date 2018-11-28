using System;
using System.Linq;
using System.Web;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing.Objects.Accessors;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;
using Umbraco.Core.Configuration;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class TemplateUtilitiesTests
    {
        [SetUp]
        public void SetUp()
        {
            Current.Reset();

            // fixme - now UrlProvider depends on EntityService for GetUrl(guid) - this is bad
            // should not depend on more than IdkMap maybe - fix this!
            var entityService = new Mock<IEntityService>();
            entityService.Setup(x => x.GetId(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>())).Returns(Attempt<int>.Fail());
            var serviceContext = new ServiceContext(entityService: entityService.Object);

            // fixme - bad in a unit test - but Udi has a static ctor that wants it?!
            var container = new Mock<IFactory>();
            container.Setup(x => x.GetInstance(typeof(TypeLoader))).Returns(
                new TypeLoader(NullCacheProvider.Instance, SettingsForTests.GenerateMockGlobalSettings(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
            container.Setup(x => x.GetInstance(typeof (ServiceContext))).Returns(serviceContext);
            Current.Factory = container.Object;

            Umbraco.Web.Composing.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();

            Udi.ResetUdiTypes();

            UmbracoConfig.For.SetUmbracoSettings(SettingsForTests.GetDefaultUmbracoSettings());
        }

        [TearDown]
        public void TearDown()
        {
            Current.Reset();
        }

        [TestCase("", "")]
        [TestCase("hello href=\"{localLink:1234}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document-type/9931BDE0AAC34BABB838909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        //this one has an invalid char so won't match
        [TestCase("hello href=\"{localLink:umb^://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"{localLink:umb^://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ")]
        public void ParseLocalLinks(string input, string result)
        {
            var serviceCtxMock = new TestObjects(null).GetServiceContextMock();

            //setup a mock entity service from the service context to return an integer for a GUID
            var entityService = Mock.Get(serviceCtxMock.EntityService);
            //entityService.Setup(x => x.GetId(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()))
            //    .Returns((Guid id, UmbracoObjectTypes objType) =>
            //    {
            //        return Attempt.Succeed(1234);
            //    });

            //setup a mock url provider which we'll use fo rtesting
            var testUrlProvider = new Mock<IUrlProvider>();
            testUrlProvider
                .Setup(x => x.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<IPublishedContent>(), It.IsAny<UrlProviderMode>(), It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns((UmbracoContext umbCtx, IPublishedContent content, UrlProviderMode mode, string culture, Uri url) => "/my-test-url");

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

            using (var umbCtx = UmbracoContext.EnsureContext(
                Umbraco.Web.Composing.Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                snapshotService,
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                //setup a quick mock of the WebRouting section
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                //pass in the custom url provider
                new[]{ testUrlProvider.Object },
                globalSettings,
                new TestVariationContextAccessor(),
                true))
            {
                var output = TemplateUtilities.ParseInternalLinks(input, umbCtx.UrlProvider);

                Assert.AreEqual(result, output);
            }
        }
    }
}
