using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests specifically related to the IndexValue.Integers collection
public class IntegerTests : SearcherTestBase
{
    [Test]
    public async Task CanFilterSingleDocumentByIntegerExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldMultipleValues, [1], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByNegativeIntegerExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldMultipleValues, [-2], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[2]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByIntegerRange()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerRangeFilter(FieldMultipleValues, [new IntegerRangeFilterRange(1, 2)], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByNegativeIntegerRange()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerRangeFilter(FieldMultipleValues, [new IntegerRangeFilterRange(-2, -1)], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[2]));
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByIntegerExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldMultipleValues, [10, 50, 100], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(5));

                IOrderedEnumerable<Document> documents = result.Documents.OrderBy(x => x.Id);
                // expecting 1 (10), 5 (50), 10 (10 + 100), 50 (50) and 100 (100)
                Assert.That(
                    documents.Select(d => d.Id).ToArray(),
                    Is.EqualTo(
                        new[]
                        {
                            DocumentIds[1], DocumentIds[5], DocumentIds[10], DocumentIds[50], DocumentIds[100]
                        }.OrderBy(x => x).ToArray()).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByIntegerRange()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new IntegerRangeFilter(
                    FieldMultipleValues,
                    [
                        new IntegerRangeFilterRange(1, 5),
                        new IntegerRangeFilterRange(20, 25),
                        new IntegerRangeFilterRange(100, 101)
                    ],
                    false)
            ]);

        Assert.Multiple(
            () =>
            {
                // expecting
                // - first range: 1, 2, 3, 4
                // - second range: 2 (20), 20, 21, 22, 23, 24
                // - third range: 10 (100), 100
                Assert.That(result.Total, Is.EqualTo(11));

                var documents = result.Documents.ToList();
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EquivalentTo(
                        new[]
                        {
                            DocumentIds[1],
                            DocumentIds[2],
                            DocumentIds[3],
                            DocumentIds[4],
                            DocumentIds[10],
                            DocumentIds[20],
                            DocumentIds[21],
                            DocumentIds[22],
                            DocumentIds[23],
                            DocumentIds[24],
                            DocumentIds[100],
                        }));
            });
    }

    [Test]
    public async Task CanFilterDocumentsByIntegerExactNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldMultipleValues, [1], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterDocumentsByIntegerRangeNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerRangeFilter(FieldMultipleValues, [new IntegerRangeFilterRange(1, 2)], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByIntegerExact(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets: [new IntegerExactFacet(FieldSingleValue)],
            filters: filtered ? [new IntegerExactFilter(FieldSingleValue, [1, 2, 3], false)] : []);

        // expecting the same facets whether filtering is enabled or not, because
        // both faceting and filtering is applied to the same field
        var expectedFacetValues = Enumerable
            .Range(1, filtered ? 3 : 100)
            .SelectMany(i => new[] { i })
            .GroupBy(i => i)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToArray();

        // expecting
        // - when filtered: 1, 2 and 3
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        IntegerExactFacetValue[] facetValues = facet.Values.OfType<IntegerExactFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(expectedFacetValues.Length));
        foreach (var expectedFacetValue in expectedFacetValues)
        {
            IntegerExactFacetValue? facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
            Assert.That(facetValue, Is.Not.Null);
            Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByIntegerRange(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new IntegerRangeFacet(
                    FieldSingleValue,
                    [
                        new IntegerRangeFacetRange("One", 1, 25),
                        new IntegerRangeFacetRange("Two", 25, 50),
                        new IntegerRangeFacetRange("Three", 50, 75),
                        new IntegerRangeFacetRange("Four", 75, 100)
                    ])
            ],
            filters: filtered ? [new IntegerExactFilter(FieldSingleValue, [1, 2, 3], false)] : []);

        // expecting
        // - when filtered: 1, 2 and 3
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        IntegerRangeFacetValue[] facetValues = facet.Values.OfType<IntegerRangeFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(4));

        // by default, Examine filters before calculating facets, so we expect the facet results to vary
        // between filtered and non-filtered search
        if (filtered)
        {
            var expectedFacetValues = new[]
            {
                new { Key = "One", Count = 3 },
                new { Key = "Two", Count = 0 },
                new { Key = "Three", Count = 0 },
                new { Key = "Four", Count = 0 },
            };

            for (var i = 0; i < expectedFacetValues.Length; i++)
            {
                Assert.That(facetValues[i].Key, Is.EqualTo(expectedFacetValues[i].Key));
                Assert.That(facetValues[i].Count, Is.EqualTo(expectedFacetValues[i].Count));
            }
        }
        else
        {
            var expectedFacetValues = Enumerable
                .Range(1, 100)
                .SelectMany(
                    i => new[] { i }
                        .Select(
                            value => value switch
                            {
                                < 25 => "One",
                                < 50 => "Two",
                                < 75 => "Three",
                                < 100 => "Four",
                                _ => null
                            })
                        .WhereNotNull()
                        .Distinct())
                .GroupBy(key => key)
                .Select(group => new { group.Key, Count = group.Count() })
                .WhereNotNull()
                .ToArray();

            foreach (var expectedFacetValue in expectedFacetValues)
            {
                IntegerRangeFacetValue? facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
                Assert.That(facetValue, Is.Not.Null);
                Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortDocumentsByInteger(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters: [new IntegerSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.First().Id, Is.EqualTo(ascending ? DocumentIds[1] : DocumentIds[100]));
            });
    }
}
