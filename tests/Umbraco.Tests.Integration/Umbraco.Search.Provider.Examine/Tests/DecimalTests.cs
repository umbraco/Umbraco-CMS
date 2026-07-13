using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests specifically related to the IndexValue.Decimals collection
public class DecimalTests : SearcherTestBase
{
    [Test]
    public async Task CanFilterSingleDocumentByDecimalExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalExactFilter(FieldMultipleValues, [1.5m], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByNegativeDecimalExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalExactFilter(FieldMultipleValues, [-1.5m], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByDecimalRange()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalRangeFilter(FieldMultipleValues, [new DecimalRangeFilterRange(1m, 2m)], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByNegativeDecimalRange()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalRangeFilter(FieldMultipleValues, [new DecimalRangeFilterRange(-1.9m, -1.1m)], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByDecimalExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalExactFilter(FieldMultipleValues, [15m, 30m, 42m], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(6));

                var documents = result.Documents.ToList();
                // expecting 10 (15), 15 (15), 20 (30), 28 (42), 30 (30) and 42 (42)
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(
                        new[]
                        {
                            DocumentIds[10],
                            DocumentIds[15],
                            DocumentIds[20],
                            DocumentIds[28],
                            DocumentIds[30],
                            DocumentIds[42]
                        }).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByDecimalRange()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new DecimalRangeFilter(
                    FieldMultipleValues,
                    [
                        new DecimalRangeFilterRange(1m, 5m),
                        new DecimalRangeFilterRange(20m, 25m),
                        new DecimalRangeFilterRange(100m, 101m)
                    ],
                    false)
            ]);

        Assert.Multiple(
            () =>
            {
                // expecting
                // - first range: 1, 2, 3, 4
                // - second range: 14 (21), 15 (22.5), 16 (24), 20, 21, 22, 23, 24
                // - third range: 67 (100.5), 100
                Assert.That(result.Total, Is.EqualTo(14));

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
                            DocumentIds[14],
                            DocumentIds[15],
                            DocumentIds[16],
                            DocumentIds[20],
                            DocumentIds[21],
                            DocumentIds[22],
                            DocumentIds[23],
                            DocumentIds[24],
                            DocumentIds[67],
                            DocumentIds[100],
                        }));
            });
    }

    [Test]
    public async Task CanFilterDocumentsByDecimalExactNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalExactFilter(FieldMultipleValues, [1.5m], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterDocumentsByDecimalRangeNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new DecimalRangeFilter(FieldMultipleValues, [new DecimalRangeFilterRange(1m, 2m)], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByDecimalExact(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets: [new DecimalExactFacet(FieldSingleValue)],
            filters: filtered ? [new DecimalExactFilter(FieldSingleValue, [1, 2, 3], false)] : []);

        // expecting the same facets whether filtering is enabled or not, because
        // both faceting and filtering is applied to the same field
        var expectedFacetValues = Enumerable
            .Range(1, filtered ? 3 : 100)
            .SelectMany(i => new[] { i })
            .GroupBy(i => i)
            .Select(group => new { Key = group.Key, Count = group.Count() })
            .ToArray();

        // expecting
        // - when filtered: 1, 2 and 3
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        DecimalExactFacetValue[] facetValues = facet.Values.OfType<DecimalExactFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(expectedFacetValues.Length));
        foreach (var expectedFacetValue in expectedFacetValues)
        {
            DecimalExactFacetValue? facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
            Assert.That(facetValue, Is.Not.Null);
            Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByDecimalRange(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new DecimalRangeFacet(
                    FieldSingleValue,
                    [
                        new DecimalRangeFacetRange("One", 1m, 25m),
                        new DecimalRangeFacetRange("Two", 25m, 50m),
                        new DecimalRangeFacetRange("Three", 50m, 75m),
                        new DecimalRangeFacetRange("Four", 75m, 100m)
                    ])
            ],
            filters: filtered ? [new DecimalExactFilter(FieldSingleValue, [1m, 2m, 3m], false)] : []);

        // expecting
        // - when filtered: 1, 2 and 3
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        DecimalRangeFacetValue[] facetValues = facet.Values.OfType<DecimalRangeFacetValue>().ToArray();
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
                    i => new[] { i * 1m }
                        .Select(
                            value => value switch
                            {
                                < 25m => "One",
                                < 50m => "Two",
                                < 75m => "Three",
                                < 100m => "Four",
                                _ => null
                            })
                        .WhereNotNull()
                        .Distinct())
                .GroupBy(key => key)
                .Select(group => new { Key = group.Key, Count = group.Count() })
                .WhereNotNull()
                .ToArray();

            foreach (var expectedFacetValue in expectedFacetValues)
            {
                DecimalRangeFacetValue? facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
                Assert.That(facetValue, Is.Not.Null);
                Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortDocumentsByDecimal(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters: [new DecimalSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.First().Id, Is.EqualTo(ascending ? DocumentIds[1] : DocumentIds[100]));
            });
    }
}
