using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Search.Core.Models.Searching.Sorting;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests;

// tests specifically related to the IndexValue.Texts collection
public class TextTests : SearcherTestBase
{
    [Test]
    public async Task CanFilterSingleDocumentBySpecificText()
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, ["single12"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(1));
                Assert.That(result.Documents.First().Id, Is.EqualTo(DocumentIds[12]));
            });
    }

    [Test]
    public async Task CanFilterMultipleDocumentsBySpecificText()
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, ["single11", "single22", "single33"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(3));

                var documents = result.Documents.ToList();
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(new[] { DocumentIds[11], DocumentIds[22], DocumentIds[33] }).AsCollection);
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFilterMultipleDocumentsByCommonText(bool even)
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, [even ? "even" : "odd"], false)]);

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
    public async Task CanFilterDocumentsBySpecificTextNegated()
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, ["single12"], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(99));
                Assert.That(
                    result.Documents.Select(d => d.Id),
                    Is.EqualTo(DocumentIds.Values.Except([DocumentIds[12]])).AsCollection);
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanFilterDocumentsByCommonTextNegated(bool even)
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, [even ? "even" : "odd"], true)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(50));

                var documents = result.Documents.ToList();
                var expectedIds = OddOrEvenIds(even is false);
                Assert.That(
                    documents.Select(d => d.Id),
                    Is.EqualTo(expectedIds.Select(id => DocumentIds[id])).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterAllDocumentsByWildcardText()
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, ["single"], false)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.Select(d => d.Id), Is.EqualTo(DocumentIds.Values).AsCollection);
            });
    }

    [Test]
    public async Task CanFilterSpecificDocumentsByWildcardText()
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldMultipleValues, ["single2"], false)]);

        // expect 2 and 20-29
        Assert.That(result.Total, Is.EqualTo(11));
        Assert.Multiple(
            () =>
            {
                Assert.That(
                    result.Documents.Select(d => d.Id),
                    Is.EquivalentTo(
                        new[]
                        {
                            DocumentIds[2],
                            DocumentIds[20],
                            DocumentIds[21],
                            DocumentIds[22],
                            DocumentIds[23],
                            DocumentIds[24],
                            DocumentIds[25],
                            DocumentIds[26],
                            DocumentIds[27],
                            DocumentIds[28],
                            DocumentIds[29],
                        }));
            });
    }

    [TestCase("spec", true)] // partial (wildcard) match
    [TestCase("special", true)] // exact match
    [TestCase("spec", false)]
    [TestCase("special", false)]
    [Ignore("We can't do textual relevance filtering at this time")]
    public async Task CanFilterAllDocumentsByTextSortedByTextualRelevance(string query, bool ascending)
    {
        SearchResult result = await SearchAsync(
            filters: [new TextFilter(FieldTextRelevance, [query], false)]);

        Assert.Multiple(
            () =>
            {
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
            });
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task CanSortDocumentsByText(bool ascending)
    {
        SearchResult result = await SearchAsync(
            sorters: [new TextSorter(FieldSingleValue, ascending ? Direction.Ascending : Direction.Descending)]);

        Assert.Multiple(
            () =>
            {
                Assert.That(result.Total, Is.EqualTo(100));
                Assert.That(result.Documents.First().Id, Is.EqualTo(ascending ? DocumentIds[1] : DocumentIds[99]));
            });
    }
}
