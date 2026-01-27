using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class DocumentUrlServiceTests
{
    [Test]
    public void ConvertToCacheModel_Converts_Single_Document_With_Single_Segment_To_Expected_Cache_Model()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.AreEqual(1, cacheModels[0].Key.LanguageId);
        Assert.IsFalse(cacheModels[0].Key.IsDraft);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNull(cacheModels[0].Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Converts_Multiple_Documents_With_Single_Segment_To_Expected_Cache_Model()
    {
        var documentKey1 = Guid.NewGuid();
        var documentKey2 = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey1,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
            new()
            {
                DocumentKey = documentKey2,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment-2",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(2, cacheModels.Count);

        var model1 = cacheModels.First(m => m.Key.DocumentKey == documentKey1);
        var model2 = cacheModels.First(m => m.Key.DocumentKey == documentKey2);

        Assert.AreEqual(1, model1.Key.LanguageId);
        Assert.AreEqual(1, model2.Key.LanguageId);
        Assert.AreEqual("test-segment", model1.Cache.PrimarySegment);
        Assert.AreEqual("test-segment-2", model2.Cache.PrimarySegment);
        Assert.IsNull(model1.Cache.AlternateSegments);
        Assert.IsNull(model2.Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Converts_Single_Document_With_Multiple_Segments_To_Expected_Cache_Model()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = false,
                LanguageId = 1,
                UrlSegment = "test-segment-2",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.AreEqual(1, cacheModels[0].Key.LanguageId);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNotNull(cacheModels[0].Cache.AlternateSegments);
        Assert.AreEqual(1, cacheModels[0].Cache.AlternateSegments!.Length);
        Assert.AreEqual("test-segment-2", cacheModels[0].Cache.AlternateSegments[0]);
    }

    [Test]
    public void ConvertToCacheModel_Performance_Test()
    {
        const int NumberOfSegments = 1;
        var segments = Enumerable.Range(0, NumberOfSegments)
            .Select((x, i) => new PublishedDocumentUrlSegment
            {
                DocumentKey = Guid.NewGuid(),
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = $"test-segment-{x + 1}",
            });
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(NumberOfSegments, cacheModels.Count);

        // Benchmarking (for NumberOfSegments = 50000):
        //  - Initial implementation (15.4): ~28s
        //  - Previous implementation (versions 15.5 through 17.0, optimized algorithm): ~75ms
        //  - Current implementation (17.1+, refactored for memory optimization, same performance as previous): ~75ms
    }

    [Test]
    public void ConvertToCacheModel_Handles_Null_LanguageId_For_Invariant_Content()
    {
        var documentKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = documentKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = null, // Invariant content uses NULL
                UrlSegment = "test-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(documentKey, cacheModels[0].Key.DocumentKey);
        Assert.IsNull(cacheModels[0].Key.LanguageId, "Invariant content should have NULL LanguageId");
        Assert.IsFalse(cacheModels[0].Key.IsDraft);
        Assert.AreEqual("test-segment", cacheModels[0].Cache.PrimarySegment);
        Assert.IsNull(cacheModels[0].Cache.AlternateSegments);
    }

    [Test]
    public void ConvertToCacheModel_Handles_Mixed_Invariant_And_Variant_Content()
    {
        var invariantDocKey = Guid.NewGuid();
        var variantDocKey = Guid.NewGuid();
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = invariantDocKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = null, // Invariant content
                UrlSegment = "invariant-segment",
            },
            new()
            {
                DocumentKey = variantDocKey,
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1, // Variant content
                UrlSegment = "variant-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(2, cacheModels.Count);

        var invariantModel = cacheModels.First(m => m.Key.DocumentKey == invariantDocKey);
        var variantModel = cacheModels.First(m => m.Key.DocumentKey == variantDocKey);

        Assert.IsNull(invariantModel.Key.LanguageId, "Invariant content should have NULL LanguageId");
        Assert.AreEqual(1, variantModel.Key.LanguageId, "Variant content should have specific LanguageId");
        Assert.AreEqual("invariant-segment", invariantModel.Cache.PrimarySegment);
        Assert.AreEqual("variant-segment", variantModel.Cache.PrimarySegment);
    }
}
