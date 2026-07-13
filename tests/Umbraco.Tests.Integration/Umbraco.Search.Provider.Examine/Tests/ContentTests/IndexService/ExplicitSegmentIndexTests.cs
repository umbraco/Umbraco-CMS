using Examine;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Extensions;
using Umbraco.Cms.Search.Provider.Examine.Helpers;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Constants = Umbraco.Cms.Search.Provider.Examine.Constants;
using CoreConstants = Umbraco.Cms.Search.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Search.Provider.Examine.Tests.ContentTests.IndexService;

/// <summary>
/// Tests that verify the indexing structure for segment-based content.
/// Previously: a new document was indexed for every segment and every culture.
/// Now: every culture will have every segment property on a single document.
/// </summary>
public class ExplicitSegmentIndexTests : IndexTestBase
{
    private static readonly Guid DocumentWithAllSegmentsKey = Guid.NewGuid();
    private static readonly Guid DocumentWithOnlyNullSegmentKey = Guid.NewGuid();
    private static readonly Guid DocumentWithOnlySegment1Key = Guid.NewGuid();
    private static readonly Guid DocumentWithSpecialCharacterSegmentsKey = Guid.NewGuid();

    /// <summary>
    /// Segments with special characters that could potentially cause issues with indexing or field naming.
    /// </summary>
    private static readonly string[] SpecialCharacterSegments =
    [
        "segment-{{curly}}",      // Curly braces (common in templating)
        "segment with spaces",    // Spaces
        "segment_underscore",     // Underscores (used as delimiter in field names)
        "segment.dot.notation",   // Dots
        "segment:colon",          // Colons
        "segment[brackets]",      // Square brackets
        "segment+plus",           // Plus signs
        "segment/slash",          // Forward slashes
        "segment-å",              // Danish character
        "segment-@@",             // @ sign
        "segment-漢字",            // Japanese kanji
    ];

    /// <summary>
    /// Expected field name format for segment-specific properties.
    /// Format: Field_{propertyAlias}_{fieldValues}_{segment}
    /// </summary>
    private static string SegmentFieldName(string propertyAlias, string segment, string fieldValues)
        => $"Field_{propertyAlias}_{fieldValues}_{segment}";

    /// <summary>
    /// Helper method to get documents from the index and filter by document key.
    /// Uses .All() query and filters in memory because Examine field queries
    /// have issues with certain keyword field values.
    /// </summary>
    private ISearchResult? GetDocumentByKeyAndCulture(IIndex index, Guid key, string culture)
    {
        var idFieldName = FieldNameHelper.FieldName(CoreConstants.FieldNames.Id, Constants.FieldValues.Keywords);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();

        return results.FirstOrDefault(doc =>
            doc.Values.TryGetValue(idFieldName, out var docKey) &&
            doc.Values.TryGetValue(Constants.SystemFields.Culture, out var docCulture) &&
            docKey == key.AsKeyword() &&
            docCulture == culture);
    }

    /// <summary>
    /// Helper method to get all documents for a specific key (all cultures).
    /// </summary>
    private IEnumerable<ISearchResult> GetDocumentsByKey(IIndex index, Guid key)
    {
        var idFieldName = FieldNameHelper.FieldName(CoreConstants.FieldNames.Id, Constants.FieldValues.Keywords);
        ISearchResults results = index.Searcher.CreateQuery().All().Execute();

        return results.Where(doc =>
            doc.Values.TryGetValue(idFieldName, out var docKey) &&
            docKey == key.AsKeyword());
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    [TestCase(true, "da-DK")]
    [TestCase(false, "da-DK")]
    public void DocumentWithMultipleSegments_IndexesAsSingleDocumentPerCulture(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        // Get all documents for this key
        var documents = GetDocumentsByKey(index, DocumentWithAllSegmentsKey)
            .Where(d => d.Values.TryGetValue(Constants.SystemFields.Culture, out var c) && c == culture)
            .ToList();

        // Should be exactly 1 document per culture (not 3 for null + segment-1 + segment-2)
        Assert.That(documents.Count, Is.EqualTo(1), $"Expected exactly 1 indexed document for culture {culture}, not one per segment");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    [TestCase(true, "da-DK")]
    [TestCase(false, "da-DK")]
    public void DocumentWithMultipleSegments_ContainsNullSegmentProperty(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Null segment property should be indexed without segment suffix
        var nullSegmentFieldName = FieldNameHelper.FieldName("segmentedProperty", Constants.FieldValues.Texts);
        Assert.That(document!.Values.ContainsKey(nullSegmentFieldName), Is.True, $"Document should contain null segment property field: {nullSegmentFieldName}");
        Assert.That(document.Values[nullSegmentFieldName], Does.Contain("InNullSegmentOnly"));
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "en-US", "segment-2")]
    [TestCase(false, "en-US", "segment-2")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public void DocumentWithMultipleSegments_ContainsExplicitSegmentProperty(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Segment-specific property should be indexed with segment suffix
        var segmentFieldName = SegmentFieldName("segmentedProperty", segment, Constants.FieldValues.Texts);
        Assert.That(document!.Values.ContainsKey(segmentFieldName), Is.True, $"Document should contain segment-specific property field: {segmentFieldName}");

        var expectedTerm = segment == "segment-1" ? "InSegment1Only" : "InSegment2Only";
        Assert.That(document.Values[segmentFieldName], Does.Contain(expectedTerm));
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void DocumentWithMultipleSegments_ContainsAllSegmentPropertiesOnSameDocument(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // All segment properties should be on the same document
        var nullSegmentFieldName = FieldNameHelper.FieldName("segmentedProperty", Constants.FieldValues.Texts);
        var segment1FieldName = SegmentFieldName("segmentedProperty", "segment-1", Constants.FieldValues.Texts);
        var segment2FieldName = SegmentFieldName("segmentedProperty", "segment-2", Constants.FieldValues.Texts);

        Assert.Multiple(() =>
        {
            Assert.That(document!.Values.ContainsKey(nullSegmentFieldName), Is.True, "Document should contain null segment property");
            Assert.That(document.Values.ContainsKey(segment1FieldName), Is.True, "Document should contain segment-1 property");
            Assert.That(document.Values.ContainsKey(segment2FieldName), Is.True, "Document should contain segment-2 property");
        });
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void DocumentWithOnlyNullSegment_IndexesWithOnlyNullSegmentProperty(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithOnlyNullSegmentKey, culture);
        Assert.That(document, Is.Not.Null);

        var nullSegmentFieldName = FieldNameHelper.FieldName("segmentedProperty", Constants.FieldValues.Texts);
        Assert.That(document!.Values.ContainsKey(nullSegmentFieldName), Is.True, "Document should contain null segment property");
        Assert.That(document.Values[nullSegmentFieldName], Does.Contain("OnlyInNullSegmentDocument"));

        // Should NOT have segment-specific properties
        var segment1FieldName = SegmentFieldName("segmentedProperty", "segment-1", Constants.FieldValues.Texts);
        Assert.That(document.Values.ContainsKey(segment1FieldName), Is.False, "Document should NOT contain segment-1 property when only null segment has values");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void DocumentWithOnlyExplicitSegment_IndexesWithOnlyThatSegmentProperty(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithOnlySegment1Key, culture);
        Assert.That(document, Is.Not.Null);

        // Should have segment-1 property
        var segment1FieldName = SegmentFieldName("segmentedProperty", "segment-1", Constants.FieldValues.Texts);
        Assert.That(document!.Values.ContainsKey(segment1FieldName), Is.True, "Document should contain segment-1 property");
        Assert.That(document.Values[segment1FieldName], Does.Contain("OnlyInSegment1Document"));

        // Should NOT have null segment property (since no value was set for null segment)
        var nullSegmentFieldName = FieldNameHelper.FieldName("segmentedProperty", Constants.FieldValues.Texts);
        Assert.That(document.Values.ContainsKey(nullSegmentFieldName), Is.False, "Document should NOT contain null segment property when only explicit segment has values");
    }

    [TestCase(true)]
    [TestCase(false)]
    public void TotalIndexedDocuments_MatchesCultureCount_NotCultureTimesSegmentCount(bool publish)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        // Get all documents for our test keys
        var allDocs = GetDocumentsByKey(index, DocumentWithAllSegmentsKey)
            .Concat(GetDocumentsByKey(index, DocumentWithOnlyNullSegmentKey))
            .Concat(GetDocumentsByKey(index, DocumentWithOnlySegment1Key))
            .Concat(GetDocumentsByKey(index, DocumentWithSpecialCharacterSegmentsKey))
            .ToList();

        // 4 documents * 2 cultures = 8 total indexed documents
        // NOT 4 documents * 2 cultures * N segments = many more documents
        Assert.That(allDocs.Count, Is.EqualTo(8), "Total indexed documents should be (document count * culture count), not (document count * culture count * segment count)");
    }

    /// <summary>
    /// Expected field name format for segment-specific aggregated texts.
    /// Format: Sys_aggregated_texts_{segment}
    /// </summary>
    private static string SegmentAggregatedTextsFieldName(string segment)
        => $"{Constants.SystemFields.AggregatedTexts}_{segment}";

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    [TestCase(true, "da-DK")]
    [TestCase(false, "da-DK")]
    public void AggregatedTexts_ContainsNullSegmentAggregatedTexts(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Null segment aggregated texts should be in the standard field
        Assert.That(document!.Values.ContainsKey(Constants.SystemFields.AggregatedTexts), Is.True,
            "Document should contain null segment aggregated texts field");
        Assert.That(document.Values[Constants.SystemFields.AggregatedTexts], Does.Contain("InNullSegmentOnly"),
            "Null segment aggregated texts should contain the null segment property value");
    }

    [TestCase(true, "en-US", "segment-1")]
    [TestCase(false, "en-US", "segment-1")]
    [TestCase(true, "en-US", "segment-2")]
    [TestCase(false, "en-US", "segment-2")]
    [TestCase(true, "da-DK", "segment-1")]
    [TestCase(false, "da-DK", "segment-1")]
    public void AggregatedTexts_ContainsExplicitSegmentAggregatedTexts(bool publish, string culture, string segment)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Segment-specific aggregated texts should be in a segment-suffixed field
        var segmentAggregatedFieldName = SegmentAggregatedTextsFieldName(segment);
        Assert.That(document!.Values.ContainsKey(segmentAggregatedFieldName), Is.True,
            $"Document should contain segment-specific aggregated texts field: {segmentAggregatedFieldName}");

        var expectedTerm = segment == "segment-1" ? "InSegment1Only" : "InSegment2Only";
        Assert.That(document.Values[segmentAggregatedFieldName], Does.Contain(expectedTerm),
            $"Segment aggregated texts should contain the {segment} property value");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void AggregatedTexts_NullSegmentDoesNotContainExplicitSegmentValues(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Null segment aggregated texts should NOT contain values from explicit segments
        var nullSegmentAggregated = document!.Values[Constants.SystemFields.AggregatedTexts];
        Assert.That(nullSegmentAggregated, Does.Not.Contain("InSegment1Only"),
            "Null segment aggregated texts should NOT contain segment-1 values");
        Assert.That(nullSegmentAggregated, Does.Not.Contain("InSegment2Only"),
            "Null segment aggregated texts should NOT contain segment-2 values");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void AggregatedTexts_ExplicitSegmentDoesNotContainOtherSegmentValues(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithAllSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        // Segment-1 aggregated texts should NOT contain values from null segment or segment-2
        var segment1AggregatedFieldName = SegmentAggregatedTextsFieldName("segment-1");
        var segment1Aggregated = document!.Values[segment1AggregatedFieldName];
        Assert.That(segment1Aggregated, Does.Not.Contain("InNullSegmentOnly"),
            "Segment-1 aggregated texts should NOT contain null segment values");
        Assert.That(segment1Aggregated, Does.Not.Contain("InSegment2Only"),
            "Segment-1 aggregated texts should NOT contain segment-2 values");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void AggregatedTexts_DocumentWithOnlyNullSegment_HasOnlyNullSegmentAggregatedTexts(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithOnlyNullSegmentKey, culture);
        Assert.That(document, Is.Not.Null);

        // Should have null segment aggregated texts
        Assert.That(document!.Values.ContainsKey(Constants.SystemFields.AggregatedTexts), Is.True,
            "Document should contain null segment aggregated texts");
        Assert.That(document.Values[Constants.SystemFields.AggregatedTexts], Does.Contain("OnlyInNullSegmentDocument"));

        // Should NOT have segment-specific aggregated texts
        var segment1AggregatedFieldName = SegmentAggregatedTextsFieldName("segment-1");
        Assert.That(document.Values.ContainsKey(segment1AggregatedFieldName), Is.False,
            "Document should NOT contain segment-1 aggregated texts when only null segment has values");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void AggregatedTexts_DocumentWithOnlyExplicitSegment_HasOnlyThatSegmentAggregatedTexts(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithOnlySegment1Key, culture);
        Assert.That(document, Is.Not.Null);

        // Should have segment-1 aggregated texts
        var segment1AggregatedFieldName = SegmentAggregatedTextsFieldName("segment-1");
        Assert.That(document!.Values.ContainsKey(segment1AggregatedFieldName), Is.True,
            "Document should contain segment-1 aggregated texts");
        Assert.That(document.Values[segment1AggregatedFieldName], Does.Contain("OnlyInSegment1Document"));

        // Should NOT have null segment aggregated texts (since no value was set for null segment)
        Assert.That(document.Values.ContainsKey(Constants.SystemFields.AggregatedTexts), Is.False,
            "Document should NOT contain null segment aggregated texts when only explicit segment has values");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void DocumentWithSpecialCharacterSegments_IndexesAsSingleDocumentPerCulture(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        var documents = GetDocumentsByKey(index, DocumentWithSpecialCharacterSegmentsKey)
            .Where(d => d.Values.TryGetValue(Constants.SystemFields.Culture, out var c) && c == culture)
            .ToList();

        Assert.That(documents.Count, Is.EqualTo(1), $"Expected exactly 1 indexed document for culture {culture} with special character segments");
    }

    [TestCase(true, "en-US")]
    [TestCase(false, "en-US")]
    public void DocumentWithSpecialCharacterSegments_ContainsAllSegmentPropertiesOnSameDocument(bool publish, string culture)
    {
        var indexAlias = GetIndexAlias(publish);
        IIndex index = GetIndex(indexAlias);

        ISearchResult? document = GetDocumentByKeyAndCulture(index, DocumentWithSpecialCharacterSegmentsKey, culture);
        Assert.That(document, Is.Not.Null);

        Assert.Multiple(() =>
        {
            foreach (var segment in SpecialCharacterSegments)
            {
                var segmentFieldName = SegmentFieldName("segmentedProperty", segment, Constants.FieldValues.Texts);
                Assert.That(document!.Values.ContainsKey(segmentFieldName), Is.True, $"Document should contain property for segment '{segment}'");
            }
        });
    }

    [SetUp]
    public async Task CreateTestDocuments()
    {
        ILanguage langDk = new LanguageBuilder()
            .WithCultureInfo("da-DK")
            .Build();
        await LanguageService.CreateAsync(langDk, Cms.Core.Constants.Security.SuperUserKey);

        IContentType contentType = new ContentTypeBuilder()
            .WithAlias("segmentTestType")
            .WithContentVariation(ContentVariation.CultureAndSegment)
            .AddPropertyType()
            .WithAlias("segmentedProperty")
            .WithVariations(ContentVariation.CultureAndSegment)
            .WithDataTypeId(Cms.Core.Constants.DataTypes.Textbox)
            .WithPropertyEditorAlias(Cms.Core.Constants.PropertyEditors.Aliases.TextBox)
            .Done()
            .Build();
        await ContentTypeService.CreateAsync(contentType, Cms.Core.Constants.Security.SuperUserKey);

        // Document 1: Has distinct values in null-segment, segment-1, and segment-2
        Content docWithAllSegments = new ContentBuilder()
            .WithKey(DocumentWithAllSegmentsKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithAllSegments")
            .WithCultureName("da-DK", "DokMedAlleSegmenter")
            .Build();

        // Null segment values
        docWithAllSegments.SetValue("segmentedProperty", "InNullSegmentOnly SharedTerm", "en-US");
        docWithAllSegments.SetValue("segmentedProperty", "InNullSegmentOnly SharedTerm", "da-DK");

        // Segment-1 values
        docWithAllSegments.SetValue("segmentedProperty", "InSegment1Only SharedTerm", "en-US", "segment-1");
        docWithAllSegments.SetValue("segmentedProperty", "InSegment1Only SharedTerm", "da-DK", "segment-1");

        // Segment-2 values
        docWithAllSegments.SetValue("segmentedProperty", "InSegment2Only", "en-US", "segment-2");
        docWithAllSegments.SetValue("segmentedProperty", "InSegment2Only", "da-DK", "segment-2");

        // Document 2: Only has null-segment values
        Content docWithOnlyNullSegment = new ContentBuilder()
            .WithKey(DocumentWithOnlyNullSegmentKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithOnlyNullSegment")
            .WithCultureName("da-DK", "DokMedKunNulSegment")
            .Build();

        docWithOnlyNullSegment.SetValue("segmentedProperty", "OnlyInNullSegmentDocument", "en-US");
        docWithOnlyNullSegment.SetValue("segmentedProperty", "OnlyInNullSegmentDocument", "da-DK");

        // Document 3: Only has segment-1 values (no null-segment values)
        Content docWithOnlySegment1 = new ContentBuilder()
            .WithKey(DocumentWithOnlySegment1Key)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithOnlySegment1")
            .WithCultureName("da-DK", "DokMedKunSegment1")
            .Build();

        docWithOnlySegment1.SetValue("segmentedProperty", "OnlyInSegment1Document", "en-US", "segment-1");
        docWithOnlySegment1.SetValue("segmentedProperty", "OnlyInSegment1Document", "da-DK", "segment-1");

        // Document 4: Has values in segments with special characters
        Content docWithSpecialCharacterSegments = new ContentBuilder()
            .WithKey(DocumentWithSpecialCharacterSegmentsKey)
            .WithContentType(contentType)
            .WithCultureName("en-US", "DocWithSpecialCharacterSegments")
            .WithCultureName("da-DK", "DokMedSpecielTegnSegmenter")
            .Build();

        // Set values for each special character segment
        foreach (var segment in SpecialCharacterSegments)
        {
            // Use a sanitized version of the segment name as part of the value to verify correct retrieval
            var sanitizedSegment = segment.Replace(" ", "_");
            docWithSpecialCharacterSegments.SetValue("segmentedProperty", $"ValueFor_{sanitizedSegment}", "en-US", segment);
            docWithSpecialCharacterSegments.SetValue("segmentedProperty", $"ValueFor_{sanitizedSegment}", "da-DK", segment);
        }

        await WaitForIndexing(GetIndexAlias(true), () =>
        {
            ContentService.Save(docWithAllSegments);
            ContentService.Publish(docWithAllSegments, ["*"]);

            ContentService.Save(docWithOnlyNullSegment);
            ContentService.Publish(docWithOnlyNullSegment, ["*"]);

            ContentService.Save(docWithOnlySegment1);
            ContentService.Publish(docWithOnlySegment1, ["*"]);

            ContentService.Save(docWithSpecialCharacterSegments);
            ContentService.Publish(docWithSpecialCharacterSegments, ["*"]);

            return Task.CompletedTask;
        });
    }
}
