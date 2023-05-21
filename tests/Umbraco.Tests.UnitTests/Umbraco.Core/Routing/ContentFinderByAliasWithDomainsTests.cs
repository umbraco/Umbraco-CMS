using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByAliasWithDomainsTests : UrlRoutingTestBase
{
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
        // SetDomains1();
        var umbracoContextAccessor = GetUmbracoContextAccessor(inputUrl);
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        var request = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);

        // must lookup domain
        publishedRouter.FindDomain(request);

        if (expectedNode > 0)
        {
            Assert.AreEqual(expectedCulture, request.Culture);
        }

        var finder = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            Mock.Of<IPublishedValueFallback>(),
            VariationContextAccessor,
            umbracoContextAccessor);
        var result = await finder.TryFindContent(request);

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
