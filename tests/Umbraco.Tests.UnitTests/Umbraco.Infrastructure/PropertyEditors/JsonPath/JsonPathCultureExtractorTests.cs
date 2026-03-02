using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.JsonPath;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PropertyEditors.JsonPath;

[TestFixture]
public class JsonPathCultureExtractorTests
{
    private JsonPathCultureExtractor _extractor = null!;

    [SetUp]
    public void SetUp()
    {
        _extractor = new JsonPathCultureExtractor();
    }

    [Test]
    public void ExtractCultures_SingleCulture_ReturnsCulture()
    {
        var path = "/values[alias=title,culture=en-US]/value";

        var cultures = _extractor.ExtractCultures(path);

        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }

    [Test]
    public void ExtractCultures_MultiplePaths_ReturnAllCultures()
    {
        var path1 = "/values[culture=en-US]/value";
        var path2 = "/values[culture=da-DK]/value";

        var cultures1 = _extractor.ExtractCultures(path1);
        var cultures2 = _extractor.ExtractCultures(path2);

        Assert.That(cultures1, Does.Contain("en-US"));
        Assert.That(cultures2, Does.Contain("da-DK"));
    }

    [Test]
    public void ExtractCultures_NoCulture_ReturnsEmpty()
    {
        var path = "/values[alias=title]/value";

        var cultures = _extractor.ExtractCultures(path);

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractCultures_EmptyString_ReturnsEmpty()
    {
        var cultures = _extractor.ExtractCultures(string.Empty);

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractSegments_SingleSegment_ReturnsSegment()
    {
        var path = "/values[alias=price,segment=premium]/value";

        var segments = _extractor.ExtractSegments(path);

        Assert.That(segments, Has.Count.EqualTo(1));
        Assert.That(segments, Does.Contain("premium"));
    }

    [Test]
    public void ExtractSegments_NoSegment_ReturnsEmpty()
    {
        var path = "/values[alias=title]/value";

        var segments = _extractor.ExtractSegments(path);

        Assert.That(segments, Is.Empty);
    }

    [Test]
    public void ContainsInvariantCultureFilter_NullCulture_ReturnsTrue()
    {
        var path = "/values[alias=title,culture=null]/value";

        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        Assert.That(containsInvariant, Is.True);
    }

    [Test]
    public void ContainsInvariantCultureFilter_CultureValue_ReturnsFalse()
    {
        var path = "/values[alias=title,culture=en-US]/value";

        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        Assert.That(containsInvariant, Is.False);
    }

    [Test]
    public void ContainsInvariantCultureFilter_NoCultureFilter_ReturnsFalse()
    {
        var path = "/values[alias=title]/value";

        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        Assert.That(containsInvariant, Is.False);
    }

    [Test]
    public void ContainsNullSegmentFilter_NullSegment_ReturnsTrue()
    {
        var path = "/values[alias=price,segment=null]/value";

        var containsNullSegment = _extractor.ContainsNullSegmentFilter(path);

        Assert.That(containsNullSegment, Is.True);
    }

    [Test]
    public void ContainsNullSegmentFilter_SegmentValue_ReturnsFalse()
    {
        var path = "/values[alias=price,segment=premium]/value";

        var containsNullSegment = _extractor.ContainsNullSegmentFilter(path);

        Assert.That(containsNullSegment, Is.False);
    }

    [Test]
    public void ExtractCulturesFromOperations_MultipleOperations_ReturnsAllUniqueCultures()
    {
        var operations = new[]
        {
            "/values[culture=en-US]/value",
            "/values[culture=da-DK]/value",
            "/values[culture=en-US]/value" // Duplicate
        };

        var cultures = _extractor.ExtractCulturesFromOperations(operations);

        Assert.That(cultures, Has.Count.EqualTo(2));
        Assert.That(cultures, Does.Contain("en-US"));
        Assert.That(cultures, Does.Contain("da-DK"));
    }

    [Test]
    public void ExtractCulturesFromOperations_EmptyCollection_ReturnsEmpty()
    {
        var cultures = _extractor.ExtractCulturesFromOperations(Array.Empty<string>());

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void AnyOperationTargetsInvariantCulture_ContainsInvariant_ReturnsTrue()
    {
        var operations = new[]
        {
            "/values[culture=en-US]/value",
            "/values[culture=null]/value"
        };

        var targetsInvariant = _extractor.AnyOperationTargetsInvariantCulture(operations);

        Assert.That(targetsInvariant, Is.True);
    }

    [Test]
    public void AnyOperationTargetsInvariantCulture_NoInvariant_ReturnsFalse()
    {
        var operations = new[]
        {
            "/values[culture=en-US]/value",
            "/values[culture=da-DK]/value"
        };

        var targetsInvariant = _extractor.AnyOperationTargetsInvariantCulture(operations);

        Assert.That(targetsInvariant, Is.False);
    }

    [Test]
    public void ExtractCultures_ComplexNestedPath_ExtractsCulture()
    {
        var path = "/values[alias=contentBlocks]/value/contentData[key=some-guid]/values[alias=headline,culture=en-US]/value";

        var cultures = _extractor.ExtractCultures(path);

        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }
}
