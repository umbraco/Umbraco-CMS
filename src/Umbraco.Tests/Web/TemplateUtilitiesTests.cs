using System;
using System.Web;
using HtmlAgilityPack;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Profiling;
using Umbraco.Core.Scoping;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;

namespace Umbraco.Tests.Web
{
    [TestFixture]
    public class TemplateUtilitiesTests : BaseUmbracoApplicationTest
    {
        [SetUp]
        public void Setup()
        {
            Udi.ResetUdiTypes();
        }

        [TestCase("", "")]
        [TestCase("hello href=\"{localLink:1234}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        [TestCase("hello href=\"{localLink:umb://document-type/9931BDE0AAC34BABB838909A7B47570E}\" world ", "hello href=\"/my-test-url\" world ")]
        //this one has an invalid char so won't match
        [TestCase("hello href=\"{localLink:umb^://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ", "hello href=\"{localLink:umb^://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\" world ")]
        // with a-tag with data-udi attribute, that needs to be stripped
        [TestCase("hello <a data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\"> world</a> ", "hello <a  href=\"/my-test-url\"> world</a> ")]
        // with a-tag with data-udi attribute spelled wrong, so don't need stripping
        [TestCase("hello <a data-uid=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" href=\"{localLink:umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570E}\"> world</a> ", "hello <a data-uid=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" href=\"/my-test-url\"> world</a> ")]
        // with a img-tag with data-udi id, that needs to be strippde
        [TestCase("hello <img data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" src=\"imageofcats.jpg\"> world ", "hello <img  src=\"imageofcats.jpg\"> world ")]
        // with a img-tag with data-udi id spelled wrong, so don't need stripping
        [TestCase("hello <img data-uid=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" src=\"imageofcats.jpg\"> world ", "hello <img data-uid=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" src=\"imageofcats.jpg\"> world ")]
        public void ParseLocalLinks(string input, string result)
        {
            var serviceCtxMock = MockHelper.GetMockedServiceContext();

            //setup a mock entity service from the service context to return an integer for a GUID
            var entityService = Mock.Get(serviceCtxMock.EntityService);
            entityService.Setup(x => x.GetIdForKey(It.IsAny<Guid>(), It.IsAny<UmbracoObjectTypes>()))
                .Returns((Guid id, UmbracoObjectTypes objType) =>
                {
                    return Attempt.Succeed(1234);
                });

            //setup a mock url provider which we'll use fo rtesting
            var testUrlProvider = new Mock<IUrlProvider>();
            testUrlProvider.Setup(x => x.GetUrl(It.IsAny<UmbracoContext>(), It.IsAny<int>(), It.IsAny<Uri>(), It.IsAny<UrlProviderMode>()))
                .Returns((UmbracoContext umbCtx, int id, Uri url, UrlProviderMode mode) =>
                {
                    return "/my-test-url";
                });

            using (var appCtx = new ApplicationContext(new DatabaseContext(new Mock<IScopeProviderInternal>().Object, Mock.Of<ILogger>(), Mock.Of<ISqlSyntaxProvider>(), "test"),
                serviceCtxMock,
                CacheHelper.CreateDisabledCacheHelper(),
                new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())))
            using (var umbCtx = UmbracoContext.EnsureContext(
                Mock.Of<HttpContextBase>(), appCtx, new Mock<WebSecurity>(null, null).Object,
                //setup a quick mock of the WebRouting section
                Mock.Of<IUmbracoSettingsSection>(section => section.WebRouting == Mock.Of<IWebRoutingSection>(routingSection => routingSection.UrlProviderMode == "AutoLegacy")),
                //pass in the custom url provider
                new[] { testUrlProvider.Object },
                true))
            {
                var output = TemplateUtilities.ParseInternalLinks(input, umbCtx.UrlProvider);

                Assert.AreEqual(result, output);
            }
        }

        [Test]
        public void StripDataUdiAttributesHtmlNullInput()
        {
            HtmlDocument htmlDocument = null;
            Assert.Throws<ArgumentNullException>(() => TemplateUtilities.StripUdiDataAttributes(htmlDocument));
        }

        [Test]
        public void StripDataUdiAttributesUsingHmtlDocOnLinks()
        {
            var input = "hello <a data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" href=\"/my-test-url\"> world</a> ";
            var expected = "hello <a href=\"/my-test-url\"> world</a> ";

            var htmlDocInput = new HtmlDocument();
            htmlDocInput.LoadHtml(input);

            var htmlDocOutput = TemplateUtilities.StripUdiDataAttributes(htmlDocInput);

            Assert.AreEqual(expected, htmlDocOutput.DocumentNode.OuterHtml);
        }

        [Test]
        public void StripDataUdiAttributesUsingHmtlDocOnImages()
        {
            var input = "hello <img data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" src=\"imageofcats.jpg\"> world ";
            var expected = "hello <img src=\"imageofcats.jpg\"> world ";

            var htmlDocInput = new HtmlDocument();
            htmlDocInput.LoadHtml(input);

            var htmlDocOutput = TemplateUtilities.StripUdiDataAttributes(htmlDocInput);

            Assert.AreEqual(expected, htmlDocOutput.DocumentNode.OuterHtml);
        }

        [Test]
        public void StripDataUdiAttributesUsingSrtringOnLinks()
        {
            var input = "hello <a data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" href=\"/my-test-url\"> world</a> ";
            var expected = "hello <a  href=\"/my-test-url\"> world</a> ";
           
            var result = TemplateUtilities.StripUdiDataAttributes(input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void StripDataUdiAttributesUsingStringOnImages()
        {
            var input = "hello <img data-udi=\"umb://document-type/9931BDE0-AAC3-4BAB-B838-909A7B47570\" src=\"imageofcats.jpg\"> world ";
            var expected = "hello <img  src=\"imageofcats.jpg\"> world ";

            var result = TemplateUtilities.StripUdiDataAttributes(input);

            Assert.AreEqual(expected, result);
        }
    }
}
