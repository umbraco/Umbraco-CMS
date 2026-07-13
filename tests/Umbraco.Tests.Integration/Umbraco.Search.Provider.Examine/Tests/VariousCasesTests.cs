using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Faceting;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// various tests unrelated to specific IndexValue collections or spanning multiple IndexValue collections
public class VariousCasesTests : SearcherTestBase
{
    [Test]
    public async Task SearchingWithoutParametersYieldsNoResults()
    {
        SearchResult result = await SearchAsync();
        Assert.That(result.Total, Is.Zero);
    }

    [Test]
    public async Task FilteringWithoutFacetsYieldsNoFacetValues()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldSingleValue, [1, 2, 3], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(3));
                Assert.That(result.Facets, Is.Empty);
            });
    }

    [Test]
    public async Task CanRetrieveObjectTypes()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldSingleValue, [1, 26, 51, 76], false)]);

        Assert.That(result.Total, Is.EqualTo(4));

        Assert.Multiple(
            () =>
            {
                Document[] documents = result.Documents.ToArray();
                Assert.That(documents[0].ObjectType, Is.EqualTo(UmbracoObjectTypes.Document));
                Assert.That(documents[1].ObjectType, Is.EqualTo(UmbracoObjectTypes.Media));
                Assert.That(documents[2].ObjectType, Is.EqualTo(UmbracoObjectTypes.Member));
                Assert.That(documents[3].ObjectType, Is.EqualTo(UmbracoObjectTypes.Unknown));
            });
    }

    [Test]
    public async Task CanCombineFacetsWithinFields()
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new IntegerExactFacet(FieldSingleValue),
                new KeywordFacet(FieldSingleValue)
            ]);

        Assert.That(result.Total, Is.EqualTo(100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(2));
        Assert.Multiple(
            () =>
            {
                Assert.That(facets[0].FieldName, Is.EqualTo(FieldSingleValue));
                Assert.That(facets[1].FieldName, Is.EqualTo(FieldSingleValue));
            });

        IntegerExactFacetValue[] integerFacetValues = facets[0].Values.OfType<IntegerExactFacetValue>().ToArray();
        KeywordFacetValue[] keywordFacetValues = facets[1].Values.OfType<KeywordFacetValue>().ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(integerFacetValues, Has.Length.EqualTo(100));
                Assert.That(keywordFacetValues, Has.Length.EqualTo(100));
            });

        for (var i = 0; i < 100; i++)
        {
            Assert.Multiple(
                () =>
                {
                    Assert.That(integerFacetValues[i].Key, Is.EqualTo(i + 1));
                    Assert.That(integerFacetValues[i].Count, Is.EqualTo(1));

                    KeywordFacetValue? keywordFacetValue = keywordFacetValues
                        .FirstOrDefault(v => v.Key == $"single{i + 1}");
                    Assert.That(keywordFacetValue?.Count, Is.EqualTo(1));
                });
        }
    }

    [Test]
    [Ignore("You cannot have multiple facets for 1 field in examine as of now, hopefully this gets resolved in a future version")]
    public async Task CanHaveSameTypeFacetsWithinFields()
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new IntegerExactFacet(FieldSingleValue),
                new IntegerRangeFacet(FieldSingleValue, [new IntegerRangeFacetRange("range", 1, 11)])
            ]);

        Assert.That(result.Total, Is.EqualTo(100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(2));
        Assert.Multiple(
            () =>
            {
                Assert.That(facets[0].FieldName, Is.EqualTo(FieldSingleValue));
                Assert.That(facets[1].FieldName, Is.EqualTo(FieldSingleValue));
            });

        IntegerExactFacetValue[] integerExactFacetValues = facets[0].Values.OfType<IntegerExactFacetValue>().ToArray();
        Assert.That(integerExactFacetValues, Has.Length.EqualTo(100));

        IntegerRangeFacetValue[] integerRangeFacetValues = facets[1].Values.OfType<IntegerRangeFacetValue>().ToArray();
        Assert.That(integerRangeFacetValues, Has.Length.EqualTo(1));
        Assert.Multiple(
            () =>
            {
                Assert.That(integerRangeFacetValues.First().Count, Is.EqualTo(10));
            });

        for (var i = 0; i < 100; i++)
        {
            Assert.Multiple(
                () =>
                {
                    Assert.That(integerExactFacetValues[i].Key, Is.EqualTo(i + 1));
                    Assert.That(integerExactFacetValues[i].Count, Is.EqualTo(1));
                });
        }
    }

    [Test]
    public async Task CanCombineFacetsAcrossFields()
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new IntegerExactFacet(FieldSingleValue),
                new KeywordFacet(FieldMultipleValues)
            ]);

        Assert.That(result.Total, Is.EqualTo(100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(2));
        Assert.Multiple(
            () =>
            {
                Assert.That(facets[0].FieldName, Is.EqualTo(FieldSingleValue));
                Assert.That(facets[1].FieldName, Is.EqualTo(FieldMultipleValues));
            });

        IntegerExactFacetValue[] integerFacetValues = facets[0].Values.OfType<IntegerExactFacetValue>().ToArray();
        KeywordFacetValue[] keywordFacetValues = facets[1].Values.OfType<KeywordFacetValue>().ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(integerFacetValues, Has.Length.EqualTo(100));
                Assert.That(keywordFacetValues, Has.Length.EqualTo(203));
            });

        Assert.Multiple(
            () =>
            {
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "all")?.Count, Is.EqualTo(100));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "odd")?.Count, Is.EqualTo(50));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "even")?.Count, Is.EqualTo(50));
            });

        for (var i = 0; i < 100; i++)
        {
            Assert.Multiple(
                () =>
                {
                    Assert.That(integerFacetValues[i].Key, Is.EqualTo(i + 1));
                    Assert.That(integerFacetValues[i].Count, Is.EqualTo(1));

                    KeywordFacetValue? keywordFacetValue = keywordFacetValues
                        .FirstOrDefault(v => v.Key == $"single{i + 1}");
                    Assert.That(keywordFacetValue?.Count, Is.EqualTo(1));
                });
        }
    }

    [Test]
    public async Task CanCombineFacetsWithFilteringAcrossFields()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldSingleValue, [1, 10, 25, 50, 100], false)],
            facets:
            [
                new IntegerExactFacet(FieldSingleValue),
                new KeywordFacet(FieldSingleValue)
            ]);

        Assert.That(result.Total, Is.EqualTo(5));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(2));
        Assert.Multiple(
            () =>
            {
                Assert.That(facets[0].FieldName, Is.EqualTo(FieldSingleValue));
                Assert.That(facets[1].FieldName, Is.EqualTo(FieldSingleValue));
            });

        IntegerExactFacetValue[] integerFacetValues = facets[0].Values.OfType<IntegerExactFacetValue>().ToArray();
        KeywordFacetValue[] keywordFacetValues = facets[1].Values.OfType<KeywordFacetValue>().ToArray();
        Assert.Multiple(
            () =>
            {
                Assert.That(integerFacetValues, Has.Length.EqualTo(5));
                Assert.That(keywordFacetValues, Has.Length.EqualTo(5));
            });

        Assert.Multiple(
            () =>
            {
                // These are supposed to be here, when filters no longer rule out facets
                // Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "all")?.Count, Is.EqualTo(5));
                // Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "odd")?.Count, Is.EqualTo(2));
                // Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "even")?.Count, Is.EqualTo(3));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "single1")?.Count, Is.EqualTo(1));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "single10")?.Count, Is.EqualTo(1));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "single25")?.Count, Is.EqualTo(1));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "single50")?.Count, Is.EqualTo(1));
                Assert.That(keywordFacetValues.FirstOrDefault(v => v.Key == "single100")?.Count, Is.EqualTo(1));
            });

        Assert.Multiple(() =>
        {
            Assert.That(integerFacetValues.FirstOrDefault(v => v.Key == 1)?.Count, Is.EqualTo(1));
            Assert.That(integerFacetValues.FirstOrDefault(v => v.Key == 10)?.Count, Is.EqualTo(1));
            Assert.That(integerFacetValues.FirstOrDefault(v => v.Key == 25)?.Count, Is.EqualTo(1));
            Assert.That(integerFacetValues.FirstOrDefault(v => v.Key == 50)?.Count, Is.EqualTo(1));
            Assert.That(integerFacetValues.FirstOrDefault(v => v.Key == 100)?.Count, Is.EqualTo(1));
        });
    }

    [Test]
    public async Task FilteringOneFieldLimitsFacetCountForAnotherField()
    {
        SearchResult result = await SearchAsync(
            filters: [new IntegerExactFilter(FieldMultipleValues, [1, 10, 25, 51, 100], false)],
            facets: [new IntegerExactFacet(FieldSingleValue)]);

        Assert.That(result.Total, Is.EqualTo(5));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        var expectedFacets = new[]
        {
            new { Key = 100, Count = 1 },
            new { Key = 51, Count = 1 },
            new { Key = 25, Count = 1 },
            new { Key = 10, Count = 1 },
            new { Key = 1, Count = 1 },
        };

        IntegerExactFacetValue[] facetValues = facets[0].Values.OfType<IntegerExactFacetValue>().ToArray();
        foreach (var expectedFacet in expectedFacets)
        {
            Assert.That(
                facetValues.SingleOrDefault(v => v.Key == expectedFacet.Key)?.Count,
                Is.EqualTo(expectedFacet.Count),
                $"Expected facet {expectedFacet.Key} to have count {expectedFacet.Count}");
        }
    }

    [Test]
    public async Task CanMixRegularAndNegatedFilters()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new IntegerExactFilter(FieldSingleValue, [1, 2, 3, 4, 5, 6, 7, 8, 9, 10], false),
                new DecimalExactFilter(FieldSingleValue, [1, 2, 3, 4, 5], true)
            ]);

        Assert.That(result.Total, Is.EqualTo(5));

        Assert.Multiple(
            () =>
            {
                // expecting 6, 7, 8, 9 and 10
                Document[] documents = result.Documents.ToArray();
                Assert.That(documents[0].Id, Is.EqualTo(DocumentIds[6]));
                Assert.That(documents[1].Id, Is.EqualTo(DocumentIds[7]));
                Assert.That(documents[2].Id, Is.EqualTo(DocumentIds[8]));
                Assert.That(documents[3].Id, Is.EqualTo(DocumentIds[9]));
                Assert.That(documents[4].Id, Is.EqualTo(DocumentIds[10]));
            });
    }

    [Test]
    public async Task CanMixFiltersAcrossFields()
    {
        SearchResult result = await SearchAsync(
            filters:
            [
                new IntegerExactFilter(FieldSingleValue, [1, 2, 3, 4, 5, 6], false),
                new IntegerExactFilter(FieldMultipleValues, [30, 50, 70, 100], false)
            ]);

        Assert.That(result.Total, Is.EqualTo(2));

        Assert.Multiple(
            () =>
            {
                // expecting 3 (30) and 5 (50)
                Document[] documents = result.Documents.ToArray();
                Assert.That(documents[0].Id, Is.EqualTo(DocumentIds[3]));
                Assert.That(documents[1].Id, Is.EqualTo(DocumentIds[5]));
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortOnMultipleFields(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters:
            [
                new KeywordSorter(FieldMultiSorting, ascending ? Direction.Ascending : Direction.Descending),
                // NOTE: to spice things up, the integer sort order is reversed (i.e. descending when the test case is ascending)
                new IntegerSorter(FieldSingleValue, ascending ? Direction.Descending : Direction.Ascending)
            ]);

        Assert.That(result.Total, Is.EqualTo(100));

        // expected: all documents sorted by "even"/"odd", subsequently by integer value reversed
        var expectedSortOrder = OddOrEvenIds(true).Reverse().Union(OddOrEvenIds(false).Reverse()).ToArray();
        if (ascending is false)
        {
            expectedSortOrder = expectedSortOrder.Reverse().ToArray();
        }

        Document[] documents = result.Documents.ToArray();
        for (var i = 0; i < 100; i++)
        {
            Assert.That(documents[i].Id, Is.EqualTo(DocumentIds[expectedSortOrder[i]]));
        }
    }

    [Test]
    public async Task IgnoresDuplicateFacets()
    {
        SearchResult result = await SearchAsync(
            facets:
            [
                new IntegerRangeFacet(
                    FieldSingleValue,
                    [
                        new IntegerRangeFacetRange("one", 1, 11)
                    ]),
                new IntegerRangeFacet(
                    FieldSingleValue,
                    [
                        new IntegerRangeFacetRange("one", 1, 5),
                        new IntegerRangeFacetRange("two", 6, 10),
                        new IntegerRangeFacetRange("three", 11, 20),
                        new IntegerRangeFacetRange("four", 21, 25),
                    ])
            ]);

        Assert.That(result.Total, Is.EqualTo(100));

        FacetResult[] facets = result.Facets.ToArray();
        Assert.That(facets, Has.Length.EqualTo(1));

        FacetResult facet = facets.Single();
        Assert.That(facet.FieldName, Is.EqualTo(FieldSingleValue));

        IntegerRangeFacetValue[] facetValues = facet.Values.OfType<IntegerRangeFacetValue>().ToArray();
        Assert.That(facetValues, Has.Length.EqualTo(1));
        Assert.That(facetValues.First().Count, Is.EqualTo(10));
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanPaginate(bool ascending)
    {
        // expected: all documents sorted by "even"/"odd", subsequently by integer value
        var expectedSortOrder = OddOrEvenIds(true).Union(OddOrEvenIds(false)).ToArray();
        if (ascending is false)
        {
            expectedSortOrder = expectedSortOrder.Reverse().ToArray();
        }

        for (var i = 0; i < 10; i++)
        {
            SearchResult result = await SearchAsync(
                sorters:
                [
                    new KeywordSorter(FieldMultiSorting, ascending ? Direction.Ascending : Direction.Descending),
                    new IntegerSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)
                ],
                skip: i * 10,
                take: 10);

            Assert.That(result.Total, Is.EqualTo(100));
            Assert.That(result.Documents.Count(), Is.EqualTo(10));

            Document[] documents = result.Documents.ToArray();
            Guid[] documentIds = documents.Select(x => x.Id).ToArray();
            Guid[] expectedDocumentIds = expectedSortOrder.Skip(i * 10).Take(10).Select(id => DocumentIds[id]).ToArray();
            Assert.That(documentIds, Is.EqualTo(expectedDocumentIds).AsCollection);
        }
    }
}
