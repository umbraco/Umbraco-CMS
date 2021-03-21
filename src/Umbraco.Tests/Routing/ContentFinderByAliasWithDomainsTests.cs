using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Routing;

namespace Umbraco.Tests.Routing
{
    [TestFixture]
    public class ContentFinderByAliasWithDomainsTests : ContentFinderByAliasTests
    {
        private PublishedContentType _publishedContentType;

        protected override void Initialize()
        {
            base.Initialize();

            var properties = new[]
            {
                new PublishedPropertyType("umbracoUrlAlias", Constants.DataTypes.Textbox, false, ContentVariation.Nothing,
                    new PropertyValueConverterCollection(Enumerable.Empty<IPropertyValueConverter>()),
                    Mock.Of<IPublishedModelFactory>(),
                    Mock.Of<IPublishedContentTypeFactory>()),
            };
            _publishedContentType = new PublishedContentType(Guid.NewGuid(), 0, "Doc", PublishedItemType.Content, Enumerable.Empty<string>(), properties, ContentVariation.Nothing);
        }

        protected override PublishedContentType GetPublishedContentTypeByAlias(string alias)
        {
            if (alias == "Doc") return _publishedContentType;
            return null;
        }

        void SetDomains1()
        {
            SetupDomainServiceMock(new[]
            {
                // No culture
                new UmbracoDomain("http://domain1.com/") {Id = 1, LanguageId = LangDeId, RootContentId = 1001, LanguageIsoCode = "de-DE"},

                // Cultures organized by domains with one level paths
                new UmbracoDomain("http://domain2.com/de") {Id = 1, LanguageId = LangDeId, RootContentId = 1002, LanguageIsoCode = "de-DE"},
                new UmbracoDomain("http://domain2.com/en") {Id = 1, LanguageId = LangEngId, RootContentId = 1002, LanguageIsoCode = "en-US"},

                // Cultures organized by sub-domains
                new UmbracoDomain("http://de.domain3.com") {Id = 1, LanguageId = LangDeId, RootContentId = 1003, LanguageIsoCode = "de-DE"},
                new UmbracoDomain("http://en.domain3.com") {Id = 1, LanguageId = LangEngId, RootContentId = 1003, LanguageIsoCode = "en-US"},

                // Domain with port
                new UmbracoDomain("http://domain4.com:8080") {Id = 1, LanguageId = LangDeId, RootContentId = 1004, LanguageIsoCode = "de-DE"},
            });
        }

        [TestCase("http://domain1.com/this/is/my/wrong/alias", "de-DE", -1001)] // Alias does not exist
        [TestCase("http://domain1.com/this/is/my/alias", "de-DE", 1001)] // Alias exists
        [TestCase("http://domain1.com/myotheralias", "de-DE", 1001)] // Alias exists
        [TestCase("http://domain1.com/page2/alias", "de-DE", 10011)] // alias to sub-page works
        [TestCase("http://domain1.com/endanger", "de-DE", 10011)] // alias to sub-page works, even with "en..."
        [TestCase("http://domain1.com/en/flux", "en-US", -10011)] // alias to domain's page fails - no /en alias on domain's home
        [TestCase("http://domain1.com/prefix", "de-DE", 1001)] // Alias starts with a '/'

        [TestCase("http://domain2.com/test212", "de-DE", -1002)] // Alias does not exist
        [TestCase("http://domain2.com/de/test1", "de-DE", 1002)] // Alias exists
        [TestCase("http://domain2.com/de/foo/bar", "de-DE", 1002)] // Alias exists
        [TestCase("http://domain2.com/de/page2/alias", "de-DE", 10021)] // alias to sub-page works
        [TestCase("http://domain2.com/en/test1", "en-US", 1002)] // Alias exists
        [TestCase("http://domain2.com/en/foo/bar", "en-US", 1002)] // Alias exists
        [TestCase("http://domain2.com/en/prefix", "en-US", 1002)] // Alias starts with a '/'

        [TestCase("http://de.domain3.com/test1", "de-DE", 1003)] // Alias exists
        [TestCase("http://de.domain3.com/page2/alias", "de-DE", 10031)] // alias to sub-page works
        [TestCase("http://en.domain3.com/test1", "en-US", 1003)] // Alias exists
        [TestCase("http://en.domain3.com/test4", "en-US", -1003)] // Alias does not exist

        [TestCase("http://domain4.com:8080/test5", "de-DE", 1004)] // Alias exists
        public void Lookup_By_Url_Alias_And_Domain(string inputUrl, string expectedCulture, int expectedNode)
        {
            SetDomains1();

            var globalSettings = Mock.Get(Factory.GetInstance<IGlobalSettings>()); //this will modify the IGlobalSettings instance stored in the container
            globalSettings.Setup(x => x.HideTopLevelNodeFromPath).Returns(false);

            var umbracoSettings = Current.Configs.Settings();

            var umbracoContext = GetUmbracoContext(inputUrl, urlProviders: new[]
            {
                new DefaultUrlProvider(umbracoSettings.RequestHandler, Logger, globalSettings.Object, new SiteDomainHelper())
            }, globalSettings: globalSettings.Object);

            var publishedRouter = CreatePublishedRouter();
            var request = publishedRouter.CreateRequest(umbracoContext);
            // must lookup domain
            publishedRouter.FindDomain(request);

            if (expectedNode > 0)
                Assert.AreEqual(expectedCulture, request.Culture.Name);

            var finder = new ContentFinderByUrlAlias(Logger);
            var result = finder.TryFindContent(request);

            if (expectedNode > 0)
            {
                Assert.IsTrue(result);
                Assert.AreEqual(request.PublishedContent.Id, expectedNode);
            }
            else
            {
                Assert.IsFalse(result);
            }
        }
    }
}
