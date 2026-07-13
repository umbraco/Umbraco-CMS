using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests specifically related to the IndexValue.DateTimeOffsets collection
public class DateTimeOffsetTests : SearcherTestBase
{
    [Test]
    public async Task CanFilterSingleDocumentByDateTimeOffsetExact()
    {
        SearchResult result = await SearchAsync(
            filters: [new DateTimeOffsetExactFilter(FieldMultipleValues, [StartDate().AddDays(1)], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterSingleDocumentByDateTimeOffsetRange()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new DateTimeOffsetRangeFilter(
                    FieldMultipleValues,
                    [new DateTimeOffsetRangeFilterRange(StartDate().AddDays(1), StartDate().AddDays(2))],
                    false)
            ]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[1]));
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByDateTimeOffsetExact()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new DateTimeOffsetExactFilter(
                    FieldMultipleValues,
                    [StartDate().AddDays(10), StartDate().AddDays(50), StartDate().AddDays(100)],
                    false)
            ]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(5));

                var documents = result.Documents.OrderBy(x => x.Id).ToList();
                // expecting 5 (10), 10 (10), 25 (50), 50 (50 + 100) and 100 (100)
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(
                        new[]
                        {
                            DocumentIds[5], DocumentIds[10], DocumentIds[25], DocumentIds[50], DocumentIds[100]
                        }.OrderBy(x => x).ToArray()).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsByDateTimeOffsetRange()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new DateTimeOffsetRangeFilter(
                    FieldMultipleValues,
                    [
                        new DateTimeOffsetRangeFilterRange(StartDate().AddDays(1), StartDate().AddDays(5)),
                        new DateTimeOffsetRangeFilterRange(StartDate().AddDays(20), StartDate().AddDays(25)),
                        new DateTimeOffsetRangeFilterRange(StartDate().AddDays(100), StartDate().AddDays(101))
                    ],
                    false)
            ]);

        Assert.Multiple(
            () =>
            {
                // expecting
                // - first range: 1, 2, 3, 4
                // - second range: 10 (20), 11 (22), 12 (24), 20, 21, 22, 23, 24
                // - third range: 50 (100), 100
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
                            DocumentIds[10],
                            DocumentIds[11],
                            DocumentIds[12],
                            DocumentIds[20],
                            DocumentIds[21],
                            DocumentIds[22],
                            DocumentIds[23],
                            DocumentIds[24],
                            DocumentIds[50],
                            DocumentIds[100],
                        }));
            });
    }

    [Test]
    public async Task CanFilterDocumentsByDateTimeOffsetExactNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new DateTimeOffsetExactFilter(FieldMultipleValues, [StartDate().AddDays(1)], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterDocumentsByDateTimeOffsetRangeNegated()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new DateTimeOffsetRangeFilter(
                    FieldMultipleValues,
                    [new DateTimeOffsetRangeFilterRange(StartDate().AddDays(1), StartDate().AddDays(2))],
                    true)
            ]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values.Skip(1)).AsCollection);
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByDateTimeOffsetExact(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets: [new DateTimeOffsetExactFacet(FieldSingleValue)],
            filters: filtered
                ?
                [
                    new DateTimeOffsetExactFilter(
                        FieldSingleValue,
                        [StartDate().AddDays(1), StartDate().AddDays(2), StartDate().AddDays(3)],
                        false)
                ]
                : []);

        // expecting the same facets whether filtering is enabled or not, because
        // both faceting and filtering is applied to the same field
        var expectedFacetValues = Enumerable
            .Range(1, filtered ? 3 : 100)
            .SelectMany(i => new[] { i }.Select(i2 => StartDate().AddDays(i2)))
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

        DateTimeOffsetExactFacetValue[] facetValues = facet.Values.OfType<DateTimeOffsetExactFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(expectedFacetValues.Length));
        foreach (var expectedFacetValue in expectedFacetValues)
        {
            DateTimeOffsetExactFacetValue?
                facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
            Assert.That(facetValue, Is.Not.Null);
            Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFacetDocumentsByDateTimeOffsetRange(bool filtered)
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new DateTimeOffsetRangeFacet(
                    FieldSingleValue,
                    [
                        new DateTimeOffsetRangeFacetRange("One", StartDate().AddDays(1), StartDate().AddDays(25)),
                        new DateTimeOffsetRangeFacetRange("Two", StartDate().AddDays(25), StartDate().AddDays(50)),
                        new DateTimeOffsetRangeFacetRange("Three", StartDate().AddDays(50), StartDate().AddDays(75)),
                        new DateTimeOffsetRangeFacetRange("Four", StartDate().AddDays(75), StartDate().AddDays(100))
                    ])
            ],
            filters: filtered
                ?
                [
                    new DateTimeOffsetExactFilter(
                        FieldSingleValue,
                        [StartDate().AddDays(1), StartDate().AddDays(2), StartDate().AddDays(3)],
                        false)
                ]
                : []);

        // expecting
        // - when filtered: 1, 2 and 3
        // - when not filtered: all of them
        Assert.That(result.Total, Is.EqualTo(filtered ? 3 : 100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.First();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        DateTimeOffsetRangeFacetValue[] facetValues = facet.Values.OfType<DateTimeOffsetRangeFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(4));

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
            // by default, Examine filters before calculating facets, so we expect the facet results to vary
            // between filtered and non-filtered search
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
                .Select(group => new { Key = group.Key, Count = group.Count() })
                .WhereNotNull()
                .ToArray();

            foreach (var expectedFacetValue in expectedFacetValues)
            {
                DateTimeOffsetRangeFacetValue?
                    facetValue = facetValues.FirstOrDefault(f => f.Key == expectedFacetValue.Key);
                Assert.That(facetValue, Is.Not.Null);
                Assert.That(facetValue.Count, Is.EqualTo(expectedFacetValue.Count));
            }
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortDocumentsByDateTimeOffset(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters:
            [
                new DateTimeOffsetSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)
            ]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.First().Id, Is.EqualTo(ascending ? DocumentIds[1] : DocumentIds[100]));
            });
    }
}
