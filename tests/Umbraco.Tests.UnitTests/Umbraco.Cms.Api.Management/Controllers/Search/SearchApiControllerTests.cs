using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Search;
using Umbraco.Cms.Api.Management.Services;
using Umbraco.Cms.Api.Management.ViewModels.Search;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Faceting;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common;
using SearchDocument = Umbraco.Cms.Core.Search.Querying.Document;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Controllers.Search;

[TestFixture]
public class SearchApiControllerTests
{
    private const string IndexAlias = "TestIndex";

    private Mock<ISearcher> _searcher = null!;
    private Mock<IEntityService> _entityService = null!;
    private SearchApiController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _searcher = new Mock<ISearcher>();
        _entityService = new Mock<IEntityService>();

        var searcherResolver = new Mock<ISearcherResolver>();
        searcherResolver.Setup(x => x.GetSearcher(IndexAlias)).Returns(_searcher.Object);

        _controller = new SearchApiController(searcherResolver.Object, _entityService.Object, new TestVariationContextAccessor(), Mock.Of<IMemberPresentationService>());
    }

    [TestCase(UmbracoObjectTypes.Document)]
    [TestCase(UmbracoObjectTypes.Element)]
    public async Task Search_Hydrates_Publishable_Content_Hits_With_Name_And_Icon(UmbracoObjectTypes objectType)
    {
        var key = Guid.NewGuid();
        SetupSearchResult(new SearchDocument(key, objectType));
        SetupEntities(objectType, CreateEntity(objectType, key, "The Name", "icon-test"));

        SearchDocumentResponseModel document = (await SearchAsync(culture: null)).Single();

        Assert.Multiple(() =>
        {
            Assert.That(document.Name, Is.EqualTo("The Name"));
            Assert.That(document.Icon, Is.EqualTo("icon-test"));
        });
    }

    [TestCase(UmbracoObjectTypes.Document)]
    [TestCase(UmbracoObjectTypes.Element)]
    public async Task Search_Uses_The_Requested_Culture_Name_For_Variant_Publishable_Content_Hits(UmbracoObjectTypes objectType)
    {
        var key = Guid.NewGuid();
        SetupSearchResult(new SearchDocument(key, objectType));

        PublishableContentEntitySlim entity = CreateEntity(objectType, key, "English Name", "icon-test");
        entity.CultureNames = new Dictionary<string, string> { ["en-US"] = "English Name", ["da-DK"] = "Danish Name" };
        SetupEntities(objectType, entity);

        SearchDocumentResponseModel document = (await SearchAsync(culture: "da-DK")).Single();

        Assert.That(document.Name, Is.EqualTo("Danish Name"));
    }

    private static PublishableContentEntitySlim CreateEntity(UmbracoObjectTypes objectType, Guid key, string name, string icon)
    {
        PublishableContentEntitySlim entity = objectType is UmbracoObjectTypes.Element
            ? new ElementEntitySlim()
            : new DocumentEntitySlim();
        entity.Key = key;
        entity.Name = name;
        entity.ContentTypeIcon = icon;
        return entity;
    }

    private void SetupSearchResult(params SearchDocument[] documents)
        => _searcher
            .Setup(x => x.SearchAsync(
                IndexAlias,
                It.IsAny<string?>(),
                It.IsAny<IEnumerable<Filter>?>(),
                It.IsAny<IEnumerable<Facet>?>(),
                It.IsAny<IEnumerable<Sorter>?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<AccessContext?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<int>()))
            .ReturnsAsync(new SearchResult(documents.Length, documents, []));

    private void SetupEntities(UmbracoObjectTypes objectType, params IEntitySlim[] entities)
        => _entityService
            .Setup(x => x.GetAll(objectType, It.IsAny<Guid[]>()))
            .Returns(entities);

    private async Task<SearchDocumentResponseModel[]> SearchAsync(string? culture)
    {
        IActionResult actionResult = await _controller.Search(new SearchRequestModel { IndexAlias = IndexAlias, Query = "test", Culture = culture });

        var okResult = actionResult as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        var result = okResult!.Value as SearchResultResponseModel;
        Assert.That(result, Is.Not.Null);
        return result!.Documents.ToArray();
    }
}
