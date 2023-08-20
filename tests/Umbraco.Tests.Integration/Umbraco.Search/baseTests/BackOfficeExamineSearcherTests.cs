using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Search.Diagnostics;
using Umbraco.Search.Models;
using Umbraco.Search.SpecialisedSearchers;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.baseTests;

public class BackOfficeExamineSearcherTests : UmbracoSearchBaseTests
{
    protected IBackOfficeSearcher BackOfficeSearcher => Services.GetRequiredService<IBackOfficeSearcher>();

    //Test to check if the search provider deliver correct Diagnostics
    [Test]
    public void Search_For_Document()
    {
        var content = CreateContent();
        var apiIndex = SearchProvider.GetIndex<IContentBase>(Constants
            .UmbracoIndexes
            .DeliveryApiContentIndexName);
        apiIndex.IndexItems(content.ToArray());
        var response =
            BackOfficeSearcher.Search(new BackofficeSearchRequest("test", UmbracoEntityTypes.Document, 0, 10),
                out long totalFound);
        Assert.AreEqual(2, totalFound);
        Assert.AreEqual(2, response.Count());

    }
}
