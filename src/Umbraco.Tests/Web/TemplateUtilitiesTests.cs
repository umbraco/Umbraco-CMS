using System;
using System.Globalization;
using System.Web;
using LightInject;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Stubs;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;

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
            var container = new Mock<IServiceContainer>();
            container.Setup(x => x.GetInstance(typeof(TypeLoader))).Returns(
                new TypeLoader(NullCacheProvider.Instance, SettingsForTests.GenerateMockGlobalSettings(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
            container.Setup(x => x.GetInstance(typeof (ServiceContext))).Returns(serviceContext);
            Current.Container = container.Object;

            Umbraco.Web.Composing.Current.UmbracoContextAccessor = new TestUmbracoContextAccessor();
            
            Udi.ResetUdiTypes();
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
            entityService.Setup(x => x.GetId(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()))
                .Returns((Guid id, UmbracoObjectTypes objType) =>
                {
                    return Attempt.Succeed(1234);
                });

            //setup a mock url provider which we'll use fo rtesting
            var testUrlProvider = new Mock<IUrlProvider>();
            testUrlProvider.Setup(x => x.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>(), It.IsAny<CultureInfo>()))
                .Returns((UmbracoContext umbCtx, int id, Uri url, UrlProviderMode mode) =>
                {
                    return "/my-test-url";
                });

            var globalSettings = SettingsForTests.GenerateMockGlobalSettings();

            using (var umbCtx = UmbracoContext.EnsureContext(
                Umbraco.Web.Composing.Current.UmbracoContextAccessor,
                Mock.Of<HttpContextBase>(),
                Mock.Of<IPublishedSnapshotService>(),
                new Mock<WebSecurity>(null, null, globalSettings).Object,
                //setup a quick mock of the WebRouting section
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                //pass in the custom url provider
                new[]{ testUrlProvider.Object },
                globalSettings,
                entityService.Object,
                true))
            {
                var output = TemplateUtilities.ParseInternalLinks(input, umbCtx.UrlProvider);

                Assert.AreEqual(result, output);
            }
        }
    }
}
