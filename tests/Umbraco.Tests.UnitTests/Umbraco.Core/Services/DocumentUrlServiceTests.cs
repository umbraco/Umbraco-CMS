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
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = Guid.NewGuid(),
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(1, cacheModels.Count);
        Assert.AreEqual(segments[0].DocumentKey, cacheModels[0].DocumentKey);
        Assert.AreEqual(1, cacheModels[0].LanguageId);
        Assert.AreEqual(1, cacheModels[0].UrlSegments.Count);
        Assert.AreEqual("test-segment", cacheModels[0].UrlSegments[0].Segment);
        Assert.IsTrue(cacheModels[0].UrlSegments[0].IsPrimary);
    }

    [Test]
    public void ConvertToCacheModel_Converts_Multiple_Documents_With_Single_Segment_To_Expected_Cache_Model()
    {
        var segments = new List<PublishedDocumentUrlSegment>
        {
            new()
            {
                DocumentKey = Guid.NewGuid(),
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment",
            },
            new()
            {
                DocumentKey = Guid.NewGuid(),
                IsDraft = false,
                IsPrimary = true,
                LanguageId = 1,
                UrlSegment = "test-segment-2",
            },
        };
        var cacheModels = DocumentUrlService.ConvertToCacheModel(segments).ToList();

        Assert.AreEqual(2, cacheModels.Count);
        Assert.AreEqual(segments[0].DocumentKey, cacheModels[0].DocumentKey);
        Assert.AreEqual(segments[1].DocumentKey, cacheModels[1].DocumentKey);
        Assert.AreEqual(1, cacheModels[0].LanguageId);
        Assert.AreEqual(1, cacheModels[1].LanguageId);
        Assert.AreEqual(1, cacheModels[0].UrlSegments.Count);
        Assert.AreEqual("test-segment", cacheModels[0].UrlSegments[0].Segment);
        Assert.AreEqual(1, cacheModels[1].UrlSegments.Count);
        Assert.AreEqual("test-segment-2", cacheModels[1].UrlSegments[0].Segment);
        Assert.IsTrue(cacheModels[0].UrlSegments[0].IsPrimary);
        Assert.IsTrue(cacheModels[1].UrlSegments[0].IsPrimary);
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
        Assert.AreEqual(documentKey, cacheModels[0].DocumentKey);
        Assert.AreEqual(1, cacheModels[0].LanguageId);
        Assert.AreEqual(2, cacheModels[0].UrlSegments.Count);
        Assert.AreEqual("test-segment", cacheModels[0].UrlSegments[0].Segment);
        Assert.AreEqual("test-segment-2", cacheModels[0].UrlSegments[1].Segment);
        Assert.IsTrue(cacheModels[0].UrlSegments[0].IsPrimary);
        Assert.IsFalse(cacheModels[0].UrlSegments[1].IsPrimary);
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
        //  - Current implementation: ~100ms
    }
}
