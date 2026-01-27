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
    public void ExtractCultures_SingleCultureWithSingleQuotes_ReturnsCulture()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title' && @.culture == 'en-US')]";

        // Act
        var cultures = _extractor.ExtractCultures(path);

        // Assert
        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }

    [Test]
    public void ExtractCultures_SingleCultureWithDoubleQuotes_ReturnsCulture()
    {
        // Arrange
        var path = """$.values[?(@.alias == "title" && @.culture == "en-US")]""";

        // Act
        var cultures = _extractor.ExtractCultures(path);

        // Assert
        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }

    [Test]
    public void ExtractCultures_MultipleCultures_ReturnsAllCultures()
    {
        // Arrange
        var path1 = "$.values[?(@.culture == 'en-US')]";
        var path2 = "$.values[?(@.culture == 'da-DK')]";

        // Act
        var cultures1 = _extractor.ExtractCultures(path1);
        var cultures2 = _extractor.ExtractCultures(path2);

        // Assert
        Assert.That(cultures1, Does.Contain("en-US"));
        Assert.That(cultures2, Does.Contain("da-DK"));
    }

    [Test]
    public void ExtractCultures_NoCulture_ReturnsEmpty()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title')]";

        // Act
        var cultures = _extractor.ExtractCultures(path);

        // Assert
        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractCultures_EmptyString_ReturnsEmpty()
    {
        // Act
        var cultures = _extractor.ExtractCultures(string.Empty);

        // Assert
        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractSegments_SingleSegment_ReturnsSegment()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'price' && @.segment == 'premium')]";

        // Act
        var segments = _extractor.ExtractSegments(path);

        // Assert
        Assert.That(segments, Has.Count.EqualTo(1));
        Assert.That(segments, Does.Contain("premium"));
    }

    [Test]
    public void ExtractSegments_NoSegment_ReturnsEmpty()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title')]";

        // Act
        var segments = _extractor.ExtractSegments(path);

        // Assert
        Assert.That(segments, Is.Empty);
    }

    [Test]
    public void ContainsInvariantCultureFilter_NullCulture_ReturnsTrue()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title' && @.culture == null)]";

        // Act
        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        // Assert
        Assert.That(containsInvariant, Is.True);
    }

    [Test]
    public void ContainsInvariantCultureFilter_CultureValue_ReturnsFalse()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title' && @.culture == 'en-US')]";

        // Act
        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        // Assert
        Assert.That(containsInvariant, Is.False);
    }

    [Test]
    public void ContainsInvariantCultureFilter_NoCultureFilter_ReturnsFalse()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'title')]";

        // Act
        var containsInvariant = _extractor.ContainsInvariantCultureFilter(path);

        // Assert
        Assert.That(containsInvariant, Is.False);
    }

    [Test]
    public void ContainsNullSegmentFilter_NullSegment_ReturnsTrue()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'price' && @.segment == null)]";

        // Act
        var containsNullSegment = _extractor.ContainsNullSegmentFilter(path);

        // Assert
        Assert.That(containsNullSegment, Is.True);
    }

    [Test]
    public void ContainsNullSegmentFilter_SegmentValue_ReturnsFalse()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'price' && @.segment == 'premium')]";

        // Act
        var containsNullSegment = _extractor.ContainsNullSegmentFilter(path);

        // Assert
        Assert.That(containsNullSegment, Is.False);
    }

    [Test]
    public void ExtractCulturesFromOperations_MultipleOperations_ReturnsAllUniqueCultures()
    {
        // Arrange
        var operations = new[]
        {
            "$.values[?(@.culture == 'en-US')]",
            "$.values[?(@.culture == 'da-DK')]",
            "$.values[?(@.culture == 'en-US')]" // Duplicate
        };

        // Act
        var cultures = _extractor.ExtractCulturesFromOperations(operations);

        // Assert
        Assert.That(cultures, Has.Count.EqualTo(2));
        Assert.That(cultures, Does.Contain("en-US"));
        Assert.That(cultures, Does.Contain("da-DK"));
    }

    [Test]
    public void ExtractCulturesFromOperations_EmptyCollection_ReturnsEmpty()
    {
        // Act
        var cultures = _extractor.ExtractCulturesFromOperations(Array.Empty<string>());

        // Assert
        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void AnyOperationTargetsInvariantCulture_ContainsInvariant_ReturnsTrue()
    {
        // Arrange
        var operations = new[]
        {
            "$.values[?(@.culture == 'en-US')]",
            "$.values[?(@.culture == null)]"
        };

        // Act
        var targetsInvariant = _extractor.AnyOperationTargetsInvariantCulture(operations);

        // Assert
        Assert.That(targetsInvariant, Is.True);
    }

    [Test]
    public void AnyOperationTargetsInvariantCulture_NoInvariant_ReturnsFalse()
    {
        // Arrange
        var operations = new[]
        {
            "$.values[?(@.culture == 'en-US')]",
            "$.values[?(@.culture == 'da-DK')]"
        };

        // Act
        var targetsInvariant = _extractor.AnyOperationTargetsInvariantCulture(operations);

        // Assert
        Assert.That(targetsInvariant, Is.False);
    }

    [Test]
    public void ExtractCultures_CaseInsensitive_ReturnsUniqueCultures()
    {
        // Arrange
        var path = "$.values[?(@.culture == 'en-US' || @.culture == 'EN-US')]";

        // Act
        var cultures = _extractor.ExtractCultures(path);

        // Assert - Should return 1 unique culture (case-insensitive comparison)
        Assert.That(cultures, Has.Count.EqualTo(1));
    }

    [Test]
    public void ExtractCultures_ComplexNestedPath_ExtractsCulture()
    {
        // Arrange
        var path = "$.values[?(@.alias == 'contentBlocks')].value.contentData[?(@.key == 'guid')].values[?(@.alias == 'headline' && @.culture == 'en-US')]";

        // Act
        var cultures = _extractor.ExtractCultures(path);

        // Assert
        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }
}
