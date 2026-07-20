using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;
using Umbraco.Cms.Search.Core.Services;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.WithoutZeroDowntime;

public class SearchTests : SearcherTestBase
{
    [Test]
    public async Task CanFindAllDocuments()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["all"], false)]);

        Assert.That(result.Total, Is.EqualTo(10));
    }

    [Test]
    public async Task CanQueryByFreeText()
    {
        SearchResult result = await SearchAsync(query: "document3");

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[3]));
        });
    }

    [Test]
    public async Task CanFilterByKeyword()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["even"], false)]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(5));
            Assert.That(
                result.Documents.Select(d => d.Id),
                Is.EquivalentTo(new[] { DocumentIds[2], DocumentIds[4], DocumentIds[6], DocumentIds[8], DocumentIds[10] }));
        });
    }

    [Test]
    public async Task CanFilterByExactKeyword()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldSingleValue, ["single7"], false)]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(1));
            Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[7]));
        });
    }

    [Test]
    public async Task CanSortByInteger()
    {
        SearchResult result = await SearchAsync(
            sorters: [new IntegerSorter(FieldSingleValue, Direction.Descending)]);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[10]));
            Assert.That(result.Documents.Last().Id, Is.EqualTo(DocumentIds[1]));
        });
    }

    [Test]
    public async Task CanFacetByKeyword()
    {
        SearchResult result = await SearchAsync(
            facets: [new KeywordFacet(FieldMultipleValues)]);

        Assert.That(result.Total, Is.EqualTo(10));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        KeywordFacetValue[] facetValues = facets[0].Values.OfType<KeywordFacetValue>().ToArray();
        Assert.Multiple(() =>
        {
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "all")?.Count, Is.EqualTo(10));
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "even")?.Count, Is.EqualTo(5));
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "odd")?.Count, Is.EqualTo(5));
        });
    }

    [Test]
    public async Task CanPaginate()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["all"], false)],
            skip: 0,
            take: 3);

        Assert.Multiple(() =>
        {
            Assert.That(result.Total, Is.EqualTo(10));
            Assert.That(result.Documents.Count(), Is.EqualTo(3));
        });
    }

    [Test]
    public async Task CanGetIndexMetadata()
    {
        IIndexer indexer = GetRequiredService<IIndexer>();
        var metadata = await indexer.GetMetadataAsync("Umb_PublishedContent");

        Assert.Multiple(() =>
        {
            Assert.That(metadata.HealthStatus, Is.EqualTo(HealthStatus.Healthy));
            Assert.That(metadata.DocumentCount, Is.EqualTo(10));
        });
    }
}
