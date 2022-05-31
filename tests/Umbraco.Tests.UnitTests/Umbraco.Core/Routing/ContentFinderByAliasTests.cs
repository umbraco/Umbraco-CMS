using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

// TODO: We should be able to decouple this from the base db tests since we're just mocking the services now
[TestFixture]
public class ContentFinderByAliasTests : UrlRoutingTestBase
{
    [TestCase("/this/is/my/alias", 1001)]
    [TestCase("/anotheralias", 1001)]
    [TestCase("/page2/alias", 10011)]
    [TestCase("/2ndpagealias", 10011)]
    [TestCase("/only/one/alias", 100111)]
    [TestCase("/ONLY/one/Alias", 100111)]
    [TestCase("/alias43", 100121)]
    public async Task Lookup_By_Url_Alias(string urlAsString, int nodeMatch)
    {
        var umbracoContextAccessor = GetUmbracoContextAccessor(urlAsString);
        var publishedRouter = CreatePublishedRouter(umbracoContextAccessor);
        var umbracoContext = umbracoContextAccessor.GetRequiredUmbracoContext();

        var frequest = await publishedRouter.CreateRequestAsync(umbracoContext.CleanedUmbracoUrl);
        var lookup =
            new ContentFinderByUrlAlias(Mock.Of<ILogger<ContentFinderByUrlAlias>>(), Mock.Of<IPublishedValueFallback>(), VariationContextAccessor, umbracoContextAccessor);

        var result = await lookup.TryFindContent(frequest);

        Assert.IsTrue(result);
        Assert.AreEqual(frequest.PublishedContent.Id, nodeMatch);
    }
}
