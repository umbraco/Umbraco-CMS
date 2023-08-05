using Examine;
using Examine.Lucene;
using Examine.Lucene.Directories;
using Examine.Lucene.Providers;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Infrastructure;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;
using Umbraco.Search;
using Umbraco.Search.Examine;
using Umbraco.Search.Examine.Lucene;
using Directory = Lucene.Net.Store.Directory;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Examine.Lucene.UmbracoExamine;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
public class PublishedContentQueryTests : UmbracoSearchBaseTests
{

    private PublishedContentQuery CreatePublishedContentQuery(IIndex indexer)
    {
        var examineManager = new Mock<IExamineManager>();
        var outarg = indexer;
        examineManager.Setup(x => x.TryGetIndex("TestIndex", out outarg)).Returns(true);

        var contentCache = new Mock<IPublishedContentCache>();
        contentCache.Setup(x => x.GetById(It.IsAny<int>()))
            .Returns((int intId) => Mock.Of<IPublishedContent>(x => x.Id == intId));
        var snapshot = Mock.Of<IPublishedSnapshot>(x => x.Content == contentCache.Object);
        var variationContext = new VariationContext();
        var variationContextAccessor = Mock.Of<IVariationContextAccessor>(x => x.VariationContext == variationContext);

        return new PublishedContentQuery(snapshot, variationContextAccessor);
    }

    [TestCase("fr-fr", ExpectedResult = "1, 3", Description = "Search Culture: fr-fr. Must return both fr-fr and invariant results")]
    [TestCase("en-us", ExpectedResult = "1, 2", Description = "Search Culture: en-us. Must return both en-us and invariant results")]
    [TestCase("*", ExpectedResult = "1, 2, 3", Description = "Search Culture: *. Must return all cultures and all invariant results")]
    [TestCase(null, ExpectedResult = "1", Description = "Search Culture: null. Must return only invariant results")]
    public string Search(string culture)
    {

    }
}
