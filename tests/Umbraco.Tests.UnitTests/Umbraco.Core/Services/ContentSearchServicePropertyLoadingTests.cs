using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Search;
using Umbraco.Cms.Core.Search.Querying;
using Umbraco.Cms.Core.Search.Querying.Faceting;
using Umbraco.Cms.Core.Search.Querying.Filtering;
using Umbraco.Cms.Core.Search.Querying.Sorting;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

/// <summary>
///     Verifies that the property aliases and template loading options passed to a content search service reach the
///     underlying service on both the database (no query) and index (query) paths.
/// </summary>
[TestFixture]
public class ContentSearchServicePropertyLoadingTests
{
    private static readonly Guid ParentKey = Guid.NewGuid();
    private static readonly Guid ResultKey = Guid.NewGuid();
    private static readonly string[] Aliases = ["title"];

    private Mock<IContentService> _contentService = null!;
    private Mock<IMediaService> _mediaService = null!;
    private Mock<ISearcherResolver> _searcherResolver = null!;
    private Mock<IIdKeyMap> _idKeyMap = null!;

    [SetUp]
    public void SetUp()
    {
        _contentService = new Mock<IContentService>();
        _mediaService = new Mock<IMediaService>();
        _idKeyMap = new Mock<IIdKeyMap>();
        _idKeyMap
            .Setup(x => x.GetIdForKey(ParentKey, It.IsAny<UmbracoObjectTypes>()))
            .Returns(Attempt.Succeed(1234));

        var searcher = new Mock<ISearcher>();
        searcher
            .Setup(x => x.SearchAsync(
                It.IsAny<string>(),
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
            .ReturnsAsync(new SearchResult(1, [new Document(ResultKey, UmbracoObjectTypes.Document)], []));
        _searcherResolver = new Mock<ISearcherResolver>();
        _searcherResolver.Setup(x => x.GetSearcher(It.IsAny<string>())).Returns(searcher.Object);
    }

    [Test]
    public async Task Content_Without_Query_Passes_Property_Aliases_And_Template_Loading_To_Content_Service()
    {
        ContentSearchService sut = CreateContentSearchService();

        await sut.SearchChildrenAsync(null, ParentKey, Aliases, null, loadTemplates: false, skip: 0, take: 10);

        _contentService.Verify(
            x => x.GetPagedChildren(
                1234,
                0,
                10,
                out It.Ref<long>.IsAny,
                It.Is<string[]?>(aliases => aliases != null && aliases.SequenceEqual(Aliases)),
                null,
                It.IsAny<Ordering?>(),
                false),
            Times.Once);
    }

    [Test]
    public async Task Content_With_Query_Passes_Property_Aliases_And_Template_Loading_To_Content_Service()
    {
        ContentSearchService sut = CreateContentSearchService();

        await sut.SearchChildrenAsync("anything", ParentKey, Aliases, null, loadTemplates: false, skip: 0, take: 10);

        _contentService.Verify(
            x => x.GetByIds(
                It.Is<IEnumerable<Guid>>(keys => keys.SequenceEqual(new[] { ResultKey })),
                It.Is<string[]?>(aliases => aliases != null && aliases.SequenceEqual(Aliases)),
                false),
            Times.Once);
    }

    [Test]
    public async Task Media_Without_Query_Passes_Property_Aliases_To_Media_Service()
    {
        MediaSearchService sut = CreateMediaSearchService();

        await sut.SearchChildrenAsync(null, ParentKey, Aliases, null, loadTemplates: false, skip: 0, take: 10);

        _mediaService.Verify(
            x => x.GetPagedChildren(
                1234,
                0,
                10,
                out It.Ref<long>.IsAny,
                It.Is<string[]?>(aliases => aliases != null && aliases.SequenceEqual(Aliases)),
                null,
                It.IsAny<Ordering?>()),
            Times.Once);
    }

    [Test]
    public async Task Media_With_Query_Passes_Property_Aliases_To_Media_Service()
    {
        MediaSearchService sut = CreateMediaSearchService();

        await sut.SearchChildrenAsync("anything", ParentKey, Aliases, null, loadTemplates: false, skip: 0, take: 10);

        _mediaService.Verify(
            x => x.GetByIds(
                It.Is<IEnumerable<Guid>>(keys => keys.SequenceEqual(new[] { ResultKey })),
                It.Is<string[]?>(aliases => aliases != null && aliases.SequenceEqual(Aliases))),
            Times.Once);
    }

    private ContentSearchService CreateContentSearchService()
        => new(_searcherResolver.Object, _contentService.Object, _idKeyMap.Object, NullLogger<ContentSearchService>.Instance);

    private MediaSearchService CreateMediaSearchService()
        => new(_searcherResolver.Object, _mediaService.Object, _idKeyMap.Object, NullLogger<MediaSearchService>.Instance);
}
