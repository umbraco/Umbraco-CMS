using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Searching;
using Umbraco.Cms.Search.Core.Models.Searching.Filtering;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Tests.Search.Examine.Integration.Tests.ContentTests.SearchService;

public class ExplicitSegmentSearchTests : SearcherTestBase
{
    private static readonly Guid DocumentWithAllSegmentsKey = Guid.NewGuid();
    private static readonly Guid DocumentWithOnlyNullSegmentKey = Guid.NewGuid();
    private static readonly Guid DocumentWithOnlySegment1Key = Guid.NewGuid();
    private static readonly Guid DocumentWithOverlappingValuesKey = Guid.NewGuid();

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "en-US", "segment-2")]
    [TestCase(false, "en-US", "segment-2")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public async Task ExplicitSegmentSearch_FindsContentInThatSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // "InSegment1Only" only exists in segment-1, "InSegment2Only" only exists in segment-2
        var searchTerm = segment == "segment-1" ? "InSegment1Only" : "InSegment2Only";

        SearchResult results = await Searcher.SearchAsync(indexAlias, searchTerm, null, null, null, culture, segment, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task NullSegmentSearch_FindsContentInNullSegment(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // "InNullSegmentOnly" only exists in the null segment
        SearchResult results = await Searcher.SearchAsync(indexAlias, "InNullSegmentOnly", null, null, null, culture, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "InSegment1Only")]
    [TestCase(false, "en-US", "InSegment1Only")]
    [TestCase(true, "en-US", "InSegment2Only")]
    [TestCase(false, "en-US", "InSegment2Only")]
    public async Task NullSegmentSearch_DoesNotFindSegmentSpecificContent(bool publish, string culture, string segmentSpecificTerm)
    {
        var indexAlias = GetIndexAlias(publish);

        // These terms only exist in specific segments, not in null segment
        // Null segment search should NOT include segment-specific content (no "upward" lookup)
        SearchResult results = await Searcher.SearchAsync(indexAlias, segmentSpecificTerm, null, null, null, culture, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US", "segment-1", "InSegment2Only")]
    [TestCase(false, "en-US", "segment-1", "InSegment2Only")]
    [TestCase(true, "en-US", "segment-2", "InSegment1Only")]
    [TestCase(false, "en-US", "segment-2", "InSegment1Only")]
    public async Task ExplicitSegmentSearch_DoesNotFindContentFromOtherSegment(bool publish, string culture, string segment, string searchTerm)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching segment-1 for segment-2's content (and vice versa) should find nothing
        // (the term doesn't exist in the searched segment OR in null segment)
        SearchResult results = await Searcher.SearchAsync(indexAlias, searchTerm, null, null, null, culture, segment, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task ExplicitSegmentSearch_DocumentWithOnlyThatSegment_IsFound(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Document that ONLY has segment-1 content (no null segment content)
        SearchResult results = await Searcher.SearchAsync(indexAlias, "OnlyInSegment1Document", null, null, null, culture, "segment-1", null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlySegment1Key));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task NullSegmentSearch_DocumentWithOnlyNullSegment_IsFound(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(indexAlias, "OnlyInNullSegmentDocument", null, null, null, culture, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task NullSegmentSearch_DocumentWithOnlyExplicitSegment_NotFound(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Document only has segment-1 content, searching null segment should not find it
        SearchResult results = await Searcher.SearchAsync(indexAlias, "OnlyInSegment1Document", null, null, null, culture, null, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public async Task ExplicitSegmentSearch_NoMatchInSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // "InNullSegmentOnly" exists ONLY in null segment, NOT in segment-1
        // Since segment-1 has no match, should fall back to null segment
        SearchResult results = await Searcher.SearchAsync(indexAlias, "InNullSegmentOnly", null, null, null, culture, segment, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task ExplicitSegmentSearch_DocumentWithOnlyNullSegment_FoundViaFallback(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Document only has null-segment content, searching with segment-1 should find it via fallback
        SearchResult results = await Searcher.SearchAsync(indexAlias, "OnlyInNullSegmentDocument", null, null, null, culture, "segment-1", null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Document with only null-segment content should be found via fallback");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    [TestCase(true, "da-DK")]
    [TestCase(false, "da-DK")]
    public async Task ExplicitSegmentSearch_NonExistentSegment_FallsBackToNullSegment(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Segment "non-existent" doesn't exist, should fall back to null segment
        SearchResult results = await Searcher.SearchAsync(indexAlias, "InNullSegmentOnly", null, null, null, culture, "non-existent-segment", null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Non-existent segment should fall back to null segment");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task ExplicitSegmentSearch_NonExistentSegment_FindsNullSegmentOnlyDocument(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Non-existent segment, document only has null-segment content
        SearchResult results = await Searcher.SearchAsync(indexAlias, "OnlyInNullSegmentDocument", null, null, null, culture, "non-existent-segment", null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Non-existent segment should find null-segment-only document via fallback");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task ExplicitSegmentSearch_TermExistsInBothSegmentAndNullSegment_ReturnsDocumentOnce(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // "SharedTerm" exists in BOTH segment-1 AND null segment of the SAME document
        // Even if the implementation searches both, we should only get 1 document back, not 2
        SearchResult results = await Searcher.SearchAsync(indexAlias, "SharedTerm", null, null, null, culture, segment, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Same document should only appear once, even if term exists in multiple segments");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task ExplicitSegmentSearch_MatchInSegmentAndFallback_ReturnsDocumentOnce(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Search for "SharedTerm" with segment-1
        // - DocumentWithAllSegmentsKey has "SharedTerm" in segment-1 (direct match, no fallback needed)
        // - If fallback were to incorrectly also run, it would find the same doc again in null-segment
        // Result must be exactly 1 document
        SearchResult results = await Searcher.SearchAsync(indexAlias, "SharedTerm", null, null, null, culture, "segment-1", null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1), "Document matched in segment should not be duplicated");
        Assert.That(results.Documents.Count(), Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1", 123)]
    [TestCase(false, "en-US", "segment-2", 123)]
    public async Task ExplicitSegmentSearch_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment, int expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for "Overlapping" which exists in null segment and segment-1 of DocumentWithOverlappingValuesKey
        SearchResult results = await Searcher.SearchAsync(indexAlias, "Overlapping", null, null, null, culture, segment, null, 0, 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    #region Integer Segment Filter Tests

    [TestCase(true, "en-US", "segment-1", 200)]
    [TestCase(false, "en-US", "segment-1", 200)]
    [TestCase(true, "en-US", "segment-2", 300)]
    [TestCase(false, "en-US", "segment-2", 300)]
    [TestCase(true, "da-DK", "segment-1", 200)]
    [TestCase(false, "da-DK", "segment-1", 200)]
    public async Task IntegerExactFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment, int expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [expectedValue], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public async Task IntegerExactFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 100 which only exists in null segment, should fall back
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [100], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task IntegerExactFilter_WithSegment_FindsDocumentWithOnlyNullSegmentViaFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 500 which only exists in null segment of DocumentWithOnlyNullSegmentKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [500], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Document with only null-segment integer value should be found via fallback");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US", "segment-1", 700)]
    [TestCase(false, "en-US", "segment-1", 700)]
    public async Task IntegerExactFilter_WithSegment_FindsDocumentWithOnlyThatSegment(bool publish, string culture, string segment, int expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 700 which only exists in segment-1 of DocumentWithOnlySegment1Key
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [expectedValue], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlySegment1Key));
    }

    [TestCase(true, "en-US", "segment-1", 123)]
    [TestCase(false, "en-US", "segment-2", 123)]
    public async Task IntegerExactFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment, int expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 123 which exists in null segment and segment-1 of DocumentWithOverlappingValuesKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [expectedValue], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task IntegerRangeFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 150-250 should match segment-1's value of 200
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerRangeFilter("count", [new IntegerRangeFilterRange(150, 250)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task IntegerRangeFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 50-150 should match null segment's value of 100 via fallback
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerRangeFilter("count", [new IntegerRangeFilterRange(50, 150)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(2), "Should fall back to null segment when explicit segment has no match in range");
        CollectionAssert.AreEquivalent(new[] { DocumentWithAllSegmentsKey, DocumentWithOverlappingValuesKey }, results.Documents.Select(d => d.Id));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task IntegerRangeFilter_WithSegment_MatchesBothSegmentAndFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 50-250 should match both segment-1's value (200) and null segment's value (100)
        // but for the SAME document, so that document should only count as one result
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerRangeFilter("count", [new IntegerRangeFilterRange(50, 250)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(2), "Same document should only appear once even if both segment and null-segment values match");
        CollectionAssert.AreEquivalent(new[] { DocumentWithAllSegmentsKey, DocumentWithOverlappingValuesKey }, results.Documents.Select(d => d.Id));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task IntegerExactFilter_WithNullSegment_DoesNotFindSegmentSpecificValues(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 200 (segment-1 value) or 300 (segment-2 value) with null segment should find nothing
        // because null segment search should not include segment-specific content (no "upward" lookup)
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerExactFilter("count", [200, 300], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-2")]
    public async Task IntegerRangeFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Should find 123 in null segment and segment-1 of DocumentWithOverlappingValuesKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new IntegerRangeFilter("count", [new IntegerRangeFilterRange(122, 124)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    #endregion

    #region Decimal Segment Filter Tests

    [TestCase(true, "en-US", "segment-1", 200.5)]
    [TestCase(false, "en-US", "segment-1", 200.5)]
    [TestCase(true, "en-US", "segment-2", 300.5)]
    [TestCase(false, "en-US", "segment-2", 300.5)]
    [TestCase(true, "da-DK", "segment-1", 200.5)]
    [TestCase(false, "da-DK", "segment-1", 200.5)]
    public async Task DecimalExactFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment, double expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [(decimal)expectedValue], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public async Task DecimalExactFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 100.5m which only exists in null segment, should fall back
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [100.5m], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DecimalExactFilter_WithSegment_FindsDocumentWithOnlyNullSegmentViaFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 500.5m which only exists in null segment of DocumentWithOnlyNullSegmentKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [500.5m], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Document with only null-segment decimal value should be found via fallback");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US", "segment-1", 700.5)]
    [TestCase(false, "en-US", "segment-1", 700.5)]
    public async Task DecimalExactFilter_WithSegment_FindsDocumentWithOnlyThatSegment(bool publish, string culture, string segment, double expectedValue)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 700.5m which only exists in segment-1 of DocumentWithOnlySegment1Key
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [(decimal)expectedValue], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlySegment1Key));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-2")]
    public async Task DecimalExactFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 1234.5 which exists in null-segment and segment-1 of DocumentWithOverlappingValuesKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [1234.5m], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DecimalRangeFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 150.0-250.0 should match segment-1's value of 200.5m
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalRangeFilter("decimalproperty", [new DecimalRangeFilterRange(150.0m, 250.0m)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DecimalRangeFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 50.0-150.0 should match null segment's value of 100.5m via fallback
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalRangeFilter("decimalproperty", [new DecimalRangeFilterRange(50.0m, 150.0m)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match in range");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DecimalRangeFilter_WithSegment_MatchesBothSegmentAndFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 50.0-250.0 should match both segment-1's value (200.5m) and null segment's value (100.5m)
        // but for the SAME document, so we should get 1 result
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalRangeFilter("decimalproperty", [new DecimalRangeFilterRange(50.0m, 250.0m)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Same document should only appear once even if both segment and null-segment values match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task DecimalExactFilter_WithNullSegment_DoesNotFindSegmentSpecificValues(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 200.5m (segment-1 value) or 300.5m (segment-2 value) with null segment should find nothing
        // because null segment search should not include segment-specific content (no "upward" lookup)
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalExactFilter("decimalproperty", [200.5m, 300.5m], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-2")]
    public async Task DecimalRangeFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 1234.5 which exists in null-segment and segment-1 of DocumentWithOverlappingValuesKey
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DecimalRangeFilter("decimalproperty", [new DecimalRangeFilterRange(1234m, 1235m)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    #endregion

    #region DateTimeOffset Segment Filter Tests

    private static readonly DateTimeOffset BaseDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [TestCase(true, "en-US", "segment-1", 31)] // 2025-02-01 is 31 days after 2025-01-01
    [TestCase(false, "en-US", "segment-1", 31)]
    [TestCase(true, "en-US", "segment-2", 59)] // 2025-03-01 is 59 days after 2025-01-01
    [TestCase(false, "en-US", "segment-2", 59)]
    [TestCase(true, "da-DK", "segment-1", 31)]
    [TestCase(false, "da-DK", "segment-1", 31)]
    public async Task DateTimeOffsetExactFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment, int daysFromBase)
    {
        var indexAlias = GetIndexAlias(publish);
        var expectedDate = BaseDate.AddDays(daysFromBase);

        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [expectedDate], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public async Task DateTimeOffsetExactFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-01-01 which only exists in null segment, should fall back
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [BaseDate], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DateTimeOffsetExactFilter_WithSegment_FindsDocumentWithOnlyNullSegmentViaFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-05-01 which only exists in null segment of DocumentWithOnlyNullSegmentKey
        var targetDate = new DateTimeOffset(2025, 5, 1, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [targetDate], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Document with only null-segment datetime value should be found via fallback");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlyNullSegmentKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DateTimeOffsetExactFilter_WithSegment_FindsDocumentWithOnlyThatSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-07-01 which only exists in segment-1 of DocumentWithOnlySegment1Key
        var targetDate = new DateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [targetDate], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOnlySegment1Key));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-2")]
    public async Task DateTimeOffsetExactFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-08-01 which exists in null-segment and segment-1 of DocumentWithOverlappingValuesKey
        var targetDate = new DateTimeOffset(2025, 8, 1, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [targetDate], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DateTimeOffsetRangeFilter_WithSegment_FindsContentInThatSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 2025-01-15 to 2025-02-15 should match segment-1's value of 2025-02-01
        var rangeStart = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetRangeFilter("datetime", [new DateTimeOffsetRangeFilterRange(rangeStart, rangeEnd)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DateTimeOffsetRangeFilter_WithSegment_FallsBackToNullSegment(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 2024-12-15 to 2025-01-15 should match null segment's value of 2025-01-01 via fallback
        var rangeStart = new DateTimeOffset(2024, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetRangeFilter("datetime", [new DateTimeOffsetRangeFilterRange(rangeStart, rangeEnd)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Should fall back to null segment when explicit segment has no match in range");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    public async Task DateTimeOffsetRangeFilter_WithSegment_MatchesBothSegmentAndFallback(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Range 2024-12-15 to 2025-02-15 should match both segment-1's value (2025-02-01) and null segment's value (2025-01-01)
        // but for the SAME document, so we should get 1 result
        var rangeStart = new DateTimeOffset(2024, 12, 15, 0, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetRangeFilter("datetime", [new DateTimeOffsetRangeFilterRange(rangeStart, rangeEnd)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1), "Same document should only appear once even if both segment and null-segment values match");
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithAllSegmentsKey));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public async Task DateTimeOffsetExactFilter_WithNullSegment_DoesNotFindSegmentSpecificValues(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-02-01 (segment-1 value) or 2025-03-01 (segment-2 value) with null segment should find nothing
        // because null segment search should not include segment-specific content (no "upward" lookup)
        var segment1Date = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var segment2Date = new DateTimeOffset(2025, 3, 1, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetExactFilter("datetime", [segment1Date, segment2Date], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: null,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(0));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-2")]
    public async Task DateTimeOffsetRangeFilter_WithOverlappingValues_FindsSingleDocumentForAllSegments(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);

        // Searching for 2025-08-01 which exists in null-segment and segment-1 of DocumentWithOverlappingValuesKey
        var rangeStart = new DateTimeOffset(2025, 07, 30, 0, 0, 0, TimeSpan.Zero);
        var rangeEnd = new DateTimeOffset(2025, 8, 02, 0, 0, 0, TimeSpan.Zero);
        SearchResult results = await Searcher.SearchAsync(
            indexAlias,
            query: null,
            filters: [new DateTimeOffsetRangeFilter("datetime", [new DateTimeOffsetRangeFilterRange(rangeStart, rangeEnd)], false)],
            facets: null,
            sorters: null,
            culture: culture,
            segment: segment,
            accessContext: null,
            skip: 0,
            take: 100);

        Assert.That(results.Total, Is.EqualTo(1));
        Assert.That(results.Documents.First().Id, Is.EqualTo(DocumentWithOverlappingValuesKey));
    }

    #endregion

    [SetUp]
    public async Task CreateTestDocuments()
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDk, Constants.Security.SuperUserKey);

        DataType decimalDataType = new DataTypeBuilder()
            .WithId(0)
            .WithoutIdentity()
            .WithDatabaseType(ValueStorageType.Decimal)
            .AddEditor()
            .WithAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .Build();
        await DataTypeService.CreateAsync(decimalDataType, Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("segmentTestType")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("segmentedProperty")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .AddPropertyType()
            .WithAlias("count")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(-51)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Integer)
            .Done()
            .AddPropertyType()
            .WithAlias("decimalproperty")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(decimalDataType.Id)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.Decimal)
            .Done()
            .AddPropertyType()
            .WithAlias("datetime")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Constants.DataTypes.DateTime)
            .WithPropertyEditorAlias(Constants.PropertyEditors.Aliases.DateTime)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Constants.Security.SuperUserKey);

        // Document 1: Has distinct values in null-segment, segment-1, and segment-2
        // This allows us to test that searches are isolated to the correct segment
        Content docWithAllSegments = new ContentBuilder()
            .WithKey(DocumentWithAllSegmentsKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithAllSegments")
            .WithCultureName("da-DK", "DokMedAlleSegmenter")
            .Build();

        docWithAllSegments.SetValue("segmentedProperty", "InNullSegmentOnly SharedTerm", "en-US");
        docWithAllSegments.SetValue("segmentedProperty", "InNullSegmentOnly SharedTerm", "da-DK");
        docWithAllSegments.SetValue("count", 100, "en-US");
        docWithAllSegments.SetValue("count", 100, "da-DK");
        docWithAllSegments.SetValue("decimalproperty", 100.5m, "en-US");
        docWithAllSegments.SetValue("decimalproperty", 100.5m, "da-DK");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 1, 1), "en-US");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 1, 1), "da-DK");

        docWithAllSegments.SetValue("segmentedProperty", "InSegment1Only SharedTerm", "en-US", "segment-1");
        docWithAllSegments.SetValue("segmentedProperty", "InSegment1Only SharedTerm", "da-DK", "segment-1");
        docWithAllSegments.SetValue("count", 200, "en-US", "segment-1");
        docWithAllSegments.SetValue("count", 200, "da-DK", "segment-1");
        docWithAllSegments.SetValue("decimalproperty", 200.5m, "en-US", "segment-1");
        docWithAllSegments.SetValue("decimalproperty", 200.5m, "da-DK", "segment-1");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 2, 1), "en-US", "segment-1");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 2, 1), "da-DK", "segment-1");

        docWithAllSegments.SetValue("segmentedProperty", "InSegment2Only", "en-US", "segment-2");
        docWithAllSegments.SetValue("segmentedProperty", "InSegment2Only", "da-DK", "segment-2");
        docWithAllSegments.SetValue("count", 300, "en-US", "segment-2");
        docWithAllSegments.SetValue("count", 300, "da-DK", "segment-2");
        docWithAllSegments.SetValue("decimalproperty", 300.5m, "en-US", "segment-2");
        docWithAllSegments.SetValue("decimalproperty", 300.5m, "da-DK", "segment-2");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 3, 1), "en-US", "segment-2");
        docWithAllSegments.SetValue("datetime", new DateTime(2025, 3, 1), "da-DK", "segment-2");

        // Document 2: Only has null-segment values (tests fallback for documents without segment-specific content)
        Content docWithOnlyNullSegment = new ContentBuilder()
            .WithKey(DocumentWithOnlyNullSegmentKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithOnlyNullSegment")
            .WithCultureName("da-DK", "DokMedKunNulSegment")
            .Build();

        docWithOnlyNullSegment.SetValue("segmentedProperty", "OnlyInNullSegmentDocument", "en-US");
        docWithOnlyNullSegment.SetValue("segmentedProperty", "OnlyInNullSegmentDocument", "da-DK");
        docWithOnlyNullSegment.SetValue("count", 500, "en-US");
        docWithOnlyNullSegment.SetValue("count", 500, "da-DK");
        docWithOnlyNullSegment.SetValue("decimalproperty", 500.5m, "en-US");
        docWithOnlyNullSegment.SetValue("decimalproperty", 500.5m, "da-DK");
        docWithOnlyNullSegment.SetValue("datetime", new DateTime(2025, 5, 1), "en-US");
        docWithOnlyNullSegment.SetValue("datetime", new DateTime(2025, 5, 1), "da-DK");

        // Document 3: Only has segment-1 values (no null-segment fallback available)
        Content docWithOnlySegment1 = new ContentBuilder()
            .WithKey(DocumentWithOnlySegment1Key)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithOnlySegment1")
            .WithCultureName("da-DK", "DokMedKunSegment1")
            .Build();

        docWithOnlySegment1.SetValue("segmentedProperty", "OnlyInSegment1Document", "en-US", "segment-1");
        docWithOnlySegment1.SetValue("segmentedProperty", "OnlyInSegment1Document", "da-DK", "segment-1");
        docWithOnlySegment1.SetValue("count", 700, "en-US", "segment-1");
        docWithOnlySegment1.SetValue("count", 700, "da-DK", "segment-1");
        docWithOnlySegment1.SetValue("decimalproperty", 700.5m, "en-US", "segment-1");
        docWithOnlySegment1.SetValue("decimalproperty", 700.5m, "da-DK", "segment-1");
        docWithOnlySegment1.SetValue("datetime", new DateTime(2025, 7, 1), "en-US", "segment-1");
        docWithOnlySegment1.SetValue("datetime", new DateTime(2025, 7, 1), "da-DK", "segment-1");

        // Document 4: Has overlapping values for null-segment and segment-1
        Content docWithOverlappingValues = new ContentBuilder()
            .WithKey(DocumentWithOverlappingValuesKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithOverlappingValues")
            .Build();

        docWithOverlappingValues.SetValue("segmentedProperty", "Overlapping", "en-US");
        docWithOverlappingValues.SetValue("segmentedProperty", "Overlapping", "en-US", "segment-1");
        docWithOverlappingValues.SetValue("count", 123, "en-US", null);
        docWithOverlappingValues.SetValue("count", 123, "en-US", "segment-1");
        docWithOverlappingValues.SetValue("decimalproperty", 1234.5m, "en-US");
        docWithOverlappingValues.SetValue("decimalproperty", 1234.5m, "en-US", "segment-1");
        docWithOverlappingValues.SetValue("datetime", new DateTime(2025, 8, 1), "en-US");
        docWithOverlappingValues.SetValue("datetime", new DateTime(2025, 8, 1), "en-US", "segment-1");

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(docWithAllSegments);
            ContentService.Publish(docWithAllSegments, ["*"]);

            ContentService.Save(docWithOnlyNullSegment);
            ContentService.Publish(docWithOnlyNullSegment, ["*"]);

            ContentService.Save(docWithOnlySegment1);
            ContentService.Publish(docWithOnlySegment1, ["*"]);

            ContentService.Save(docWithOverlappingValues);
            ContentService.Publish(docWithOverlappingValues, ["*"]);

            return Task.CompletedTask;
        });
    }
}
