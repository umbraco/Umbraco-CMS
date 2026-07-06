using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Tests.Search.Examine.Integration.Tests;

// tests specifically related to free text querying
public class QueryTests : SearcherTestBase
{
    [TestCase(null)]
    [TestCase("R1")]
    [TestCase("R2")]
    [TestCase("R3")]
    public async Task CanQuerySingleDocument(string? relevanceLevel)
    {
        var query = $"texts{(relevanceLevel is not null ? $"_{relevanceLevel.ToLowerInvariant()}" : null)}_12";
        SearchResult result = await SearchAsync(query: query);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[12]));
            });
    }

    [Test]
    public async Task CanQueryMultipleDocumentsByWildcardQuery()
    {
        SearchResult result = await SearchAsync(query: "single1");

        Assert.Multiple(
            () =>
            {
                // expected: 1, 10-19, 100
                Assert.That(result.Total, Is.EqualTo(12));

                var documents = result.Documents.ToList();
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(
                        new[]
                        {
                            DocumentIds[1],
                            DocumentIds[10],
                            DocumentIds[11],
                            DocumentIds[12],
                            DocumentIds[13],
                            DocumentIds[14],
                            DocumentIds[15],
                            DocumentIds[16],
                            DocumentIds[17],
                            DocumentIds[18],
                            DocumentIds[19],
                            DocumentIds[100],
                        }).AsCollection);
            });
    }

    [Test]
    public async Task CanQuerySingleDocumentByPhrase()
    {
        SearchResult result = await SearchAsync(query: "phrase search single12");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[12]));
            });
    }

    [Test]
    public async Task CanQuerySingleDocumentByPhraseInverted()
    {
        SearchResult result = await SearchAsync(query: "single12 search phrase");

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[12]));
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanQueryMultipleDocumentsByCommonWord(bool even)
    {
        SearchResult result = await SearchAsync(query: even ? "even" : "odd");

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

    [TestCase(true, Ignore = "Examine does not seem to support ascending score sorting.")]
    [TestCase(false)]
    public async Task CanQueryDocumentsByTextualRelevance(bool ascending)
    {
        SearchResult result = await SearchAsync(
            query: "special",
            sorters: [new ScoreSorter(ascending ? Direction.Ascending : Direction.Descending)]);

        Assert.That(result.Total, Is.EqualTo(4));

        Guid[] expectedDocumentIdsByOrderOfRelevance =
        [
            DocumentIds[30], // TextsR1
            DocumentIds[20], // TextsR2
            DocumentIds[40], // TextsR3
            DocumentIds[10] // Texts
        ];
        if (ascending)
        {
            expectedDocumentIdsByOrderOfRelevance = expectedDocumentIdsByOrderOfRelevance.Reverse().ToArray();
        }

        Assert.That(
            result.Documents.Select(d => d.Id),
            Is.EqualTo(expectedDocumentIdsByOrderOfRelevance).AsCollection);
    }

    // "specia" is a prefix of "special" but not a complete token, so the analyzed exact-match
    // clause cannot hit. Only the wildcard clause matches. This proves the wildcard query
    // itself carries the relevance-tier boost (R1 > R2 > R3 > Texts) — without the boost
    // on the wildcard, all four documents would tie at the same constant wildcard score.
    [Test]
    public async Task WildcardOnlyMatchesRespectRelevanceTierBoost()
    {
        SearchResult result = await SearchAsync(
            query: "specia",
            sorters: [new ScoreSorter(Direction.Descending)]);

        Assert.That(result.Total, Is.EqualTo(4));

        Guid[] expectedDocumentIdsByOrderOfRelevance =
        [
            DocumentIds[30], // TextsR1
            DocumentIds[20], // TextsR2
            DocumentIds[40], // TextsR3
            DocumentIds[10]  // Texts
        ];

        Assert.That(
            result.Documents.Select(d => d.Id),
            Is.EqualTo(expectedDocumentIdsByOrderOfRelevance).AsCollection);
    }
}
