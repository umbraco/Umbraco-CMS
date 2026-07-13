using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests specifically related to the IndexValue.Keywords collection
public  class KeywordTests : SearcherTestBase
{
    [Test]
    public async Task CanFilterSingleDocumentByKeyword()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["single1"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFilterMultipleDocumentsByKeyword(bool even)
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, [even ? "even" : "odd"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(50));

                var documents = result.Documents.ToList();
                var expectedIds = OddOrEvenIds(even);
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(expectedIds.Select(id => DocumentIds[id])).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterAllDocumentsByKeyword()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["all"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterDocumentsByKeywordNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["single1"], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [Test]
    public async Task CannotFilterMultipleDocumentByPartialKeyword()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["common"], false)]);

        Assert.That(result.Total, Is.EqualTo(0));
    }


    [Test]
    public async Task CanFilterSingleDocumentByKeywordWithSpace()
    {
        SearchResult result = await SearchAsync(
            filters: [new KeywordFilter(FieldMultipleValues, ["common single1"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByKeyword(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets: [new KeywordFacet(FieldSingleValue)],
            filters: filtered
                ? [new KeywordFilter(FieldSingleValue, ["single1", "single2", "single3"], false)]
                : []);

        // expecting the same facets whether filtering is enabled or not, because
        // both faceting and filtering is applied to the same field
        var expectedFacetValues = Enumerable
            .Range(1, filtered ? 3 : 100)
            .SelectMany(i => new[] { $"single{i}" })
            .GroupBy(i => i)
            .Select(group => new { Key = group.Key, Count = group.Count() })
            .ToArray();

        // expecting
        // - when filtered: 10, 20 and 30
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        KeywordFacetValue[] facetValues = facet.Values.OfType<KeywordFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(expectedFacetValues.Length));
        foreach (var expectedFacetValue in expectedFacetValues)
        {
            KeywordFacetValue? facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
            Assert.That(facetValue, Is.Not.Null);
            Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
        }
    }

    [Test]
    public async Task CanFacetDocumentsByMultipleKeyword()
    {
        SearchResult result = await SearchAsync(facets: [new KeywordFacet(FieldMultipleValues)]);

        Assert.That(result.Total, Is.EqualTo(100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldMultipleValues));

        KeywordFacetValue[] facetValues = facet.Values.OfType<KeywordFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(203));

        Assert.Multiple(() =>
        {
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "all")?.Count, Is.EqualTo(100));
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "even")?.Count, Is.EqualTo(50));
            Assert.That(facetValues.FirstOrDefault(f => f.Key == "odd")?.Count, Is.EqualTo(50));

            for (var i = 1; i <= 100; i++)
            {
                Assert.That(facetValues.FirstOrDefault(f => f.Key == $"single{i}")?.Count, Is.EqualTo(1));
                Assert.That(facetValues.FirstOrDefault(f => f.Key == $"common single{i}")?.Count, Is.EqualTo(1));
            }
        });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortDocumentsByKeyword(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters: [new KeywordSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.First().Id, Is.EqualTo(ascending ? DocumentIds[1] : DocumentIds[99]));
            });
    }
}
