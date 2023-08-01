using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
public class UmbracoSearchTests : UmbracoIntegrationTest
{
    [Test]
    public void Create_New()
    {
        var indexes = SearchProvider.GetAllIndexes();
        Assert.AreEqual(4, indexes.Count());
    }

    [Test]
    public void GetIndexes()
    {
        var apiIndex = SearchProvider.GetIndex(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, apiIndex);
        var member = SearchProvider.GetIndex(Constants
            .UmbracoIndexes
            .MembersIndexName);
        Assert.AreNotEqual(null, member);
        var internalIndex = SearchProvider.GetIndex(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, internalIndex);
        var externalIndex = SearchProvider.GetIndex(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, externalIndex);
    }
    [Test]
    public void GetSearchers()
    {
        var apiIndex = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, apiIndex);
        var member = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .MembersIndexName);
        Assert.AreNotEqual(null, member);
        var internalIndex = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, internalIndex);
        var externalIndex = SearchProvider.GetSearcher(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        Assert.AreNotEqual(null, externalIndex);
    }
    [Test]
    public void IndexContent()
    {

        var internalIndex = SearchProvider.GetIndex<IContent>(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
       // internalIndex.IndexItems();
    }
    [Test]
    public void IndexMultipleContent()
    {
        var internalIndex = SearchProvider.GetIndex<IContent>(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
       // internalIndex.IndexItems();
    }
}
