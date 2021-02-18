using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Routing;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Routing;
using Constants = Umbraco.Cms.Core.Constants;

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
                new PublishedPropertyType(
                    propertyTypeAlias:"umbracoUrlAlias",
                    dataTypeId: Constants.DataTypes.Textbox,
                    isUserProperty:false,
                    variations: ContentVariation.Nothing,
                    propertyValueConverters:new PropertyValueConverterCollection(Enumerable.Empty<IPropertyValueConverter>()),
                    contentType:Mock.Of<IPublishedContentType>(),
                    publishedModelFactory:Mock.Of<IPublishedModelFactory>(),
                    factory:Mock.Of<IPublishedContentTypeFactory>()
                    )
            };
            _publishedContentType = new PublishedContentType(Guid.NewGuid(), 0, "Doc", PublishedItemType.Content, Enumerable.Empty<string>(), properties, ContentVariation.Nothing);
        }

        protected override PublishedContentType GetPublishedContentTypeByAlias(string alias)
        {
            if (alias == "Doc") return _publishedContentType;
            return null;
        }

        [TestCase("http://domain1.com/this/is/my/alias", "de-DE", -1001)] // alias to domain's page fails - no alias on domain's home
        [TestCase("http://domain1.com/page2/alias", "de-DE", 10011)] // alias to sub-page works
        [TestCase("http://domain1.com/en/flux", "en-US", -10011)] // alias to domain's page fails - no alias on domain's home
        [TestCase("http://domain1.com/endanger", "de-DE", 10011)] // alias to sub-page works, even with "en..."
        [TestCase("http://domain1.com/en/endanger", "en-US", -10011)] // no
        [TestCase("http://domain1.com/only/one/alias", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/entropy", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/bar/foo", "de-DE", 100111)] // ok
        [TestCase("http://domain1.com/en/bar/foo", "en-US", -100111)] // no, alias must include "en/"
        [TestCase("http://domain1.com/en/bar/nil", "en-US", 100111)] // ok, alias includes "en/"
        public async Task Lookup_By_Url_Alias_And_Domain(string inputUrl, string expectedCulture, int expectedNode)
        {
            //SetDomains1();

            var umbracoContext = GetUmbracoContext(inputUrl);
            var publishedRouter = CreatePublishedRouter(GetUmbracoContextAccessor(umbracoContext));
            var request = await publishedRouter .CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
            // must lookup domain
            publishedRouter.FindDomain(request);

            if (expectedNode > 0)
            {
                Assert.AreEqual(expectedCulture, request.Culture);
            }

            var finder = new ContentFinderByUrlAlias(LoggerFactory.CreateLogger<ContentFinderByUrlAlias>(), Mock.Of<IPublishedValueFallback>(), VariationContextAccessor, GetUmbracoContextAccessor(umbracoContext));
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
