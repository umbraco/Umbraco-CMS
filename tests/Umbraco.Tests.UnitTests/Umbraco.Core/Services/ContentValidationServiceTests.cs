// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.ContentEditing;

/// <summary>
/// Contains unit tests for the <see cref="ContentValidationService"/> class within the content editing domain.
/// </summary>
[TestFixture]
public class ContentValidationServiceTests
{
    // Concrete implementation for testing the abstract base class.
    private class TestContentEditingModel : ContentEditingModelBase
    {
    }

    /// <summary>
    /// Tests that GetPopulatedSegmentCultures returns an empty dictionary when there are no segments in the variants.
    /// </summary>
    [Test]
    public void GetPopulatedSegmentCultures_ReturnsEmptyDictionary_WhenNoSegmentsInVariants()
    {
        var model = new TestContentEditingModel
        {
            Variants =
            [
                new VariantModel { Name = "English", Culture = "en-US", Segment = null },
                new VariantModel { Name = "Danish", Culture = "da-DK", Segment = null }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Hello", Culture = "en-US", Segment = null }
            ],
        };

        var result = ContentValidationService.GetPopulatedSegmentCultures(model, ["en-US", "da-DK"]);

        Assert.That(result, Is.Empty);
    }

    /// <summary>
    /// Tests that GetPopulatedSegmentCultures returns a segment with the correct cultures when properties exist for that segment.
    /// </summary>
    [Test]
    public void GetPopulatedSegmentCultures_ReturnsSegmentWithCultures_WhenPropertiesExistForSegment()
    {
        var model = new TestContentEditingModel
        {
            Variants =
            [
                new VariantModel { Name = "English Default", Culture = "en-US", Segment = null },
                new VariantModel { Name = "English Segment 1", Culture = "en-US", Segment = "segment-1" },
                new VariantModel { Name = "Danish Segment 1", Culture = "da-DK", Segment = "segment-1" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Hello", Culture = "en-US", Segment = null },
                new PropertyValueModel { Alias = "title", Value = "Hello Segment 1", Culture = "en-US", Segment = "segment-1" },
                new PropertyValueModel { Alias = "title", Value = "Hej Segment 1", Culture = "da-DK", Segment = "segment-1" }
            ],
        };

        var result = ContentValidationService.GetPopulatedSegmentCultures(model, ["en-US", "da-DK"]);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.ContainsKey("segment-1"), Is.True);
        Assert.That(result["segment-1"], Does.Contain("en-US"));
        Assert.That(result["segment-1"], Does.Contain("da-DK"));
    }

    /// <summary>
    /// Tests that GetPopulatedSegmentCultures returns only cultures that have properties
    /// when some segment-culture combinations do not have any properties.
    /// </summary>
    [Test]
    public void GetPopulatedSegmentCultures_ReturnsOnlyCulturesWithProperties_WhenSomeSegmentCultureCombinationsHaveNoProperties()
    {
        var model = new TestContentEditingModel
        {
            Variants =
            [
                new VariantModel { Name = "English Segment 1", Culture = "en-US", Segment = "segment-1" },
                new VariantModel { Name = "Danish Segment 1", Culture = "da-DK", Segment = "segment-1" }
            ],
            Properties =
            [
                // Only en-US has a property for the mobile segment
                new PropertyValueModel { Alias = "title", Value = "Hello Segment 1", Culture = "en-US", Segment = "segment-1" }
            ],
        };

        var result = ContentValidationService.GetPopulatedSegmentCultures(model, ["en-US", "da-DK"]);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result.ContainsKey("segment-1"), Is.True);
        Assert.That(result["segment-1"], Does.Contain("en-US"));
        Assert.That(result["segment-1"], Does.Not.Contain("da-DK"));
    }

    /// <summary>
    /// Tests that GetPopulatedSegmentCultures excludes cultures not specified in the cultures parameter.
    /// </summary>
    [Test]
    public void GetPopulatedSegmentCultures_ExcludesCulturesNotInCulturesParameter()
    {
        var model = new TestContentEditingModel
        {
            Variants =
            [
                new VariantModel { Name = "English Segment 1", Culture = "en-US", Segment = "segment-1" },
                new VariantModel { Name = "Danish Segment 1", Culture = "da-DK", Segment = "segment-1" },
                new VariantModel { Name = "German Segment 1", Culture = "de-DE", Segment = "segment-1" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Hello Segment 1", Culture = "en-US", Segment = "segment-1" },
                new PropertyValueModel { Alias = "title", Value = "Hej Segment 1", Culture = "da-DK", Segment = "segment-1" },
                new PropertyValueModel { Alias = "title", Value = "Hallo Segment 1", Culture = "de-DE", Segment = "segment-1" }
            ],
        };

        // Only validating en-US and da-DK, not de-DE
        var result = ContentValidationService.GetPopulatedSegmentCultures(model, ["en-US", "da-DK"]);

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result["segment-1"], Does.Contain("en-US"));
        Assert.That(result["segment-1"], Does.Contain("da-DK"));
        Assert.That(result["segment-1"], Does.Not.Contain("de-DE"));
    }

    /// <summary>
    /// Tests that GetPopulatedSegmentCultures correctly handles multiple segments with different cultures.
    /// </summary>
    [Test]
    public void GetPopulatedSegmentCultures_HandlesMultipleSegments()
    {
        var model = new TestContentEditingModel
        {
            Variants =
            [
                new VariantModel { Name = "English Segment 1", Culture = "en-US", Segment = "segment-1" },
                new VariantModel { Name = "English Segment 2", Culture = "en-US", Segment = "segment-2" },
                new VariantModel { Name = "Danish Segment 1", Culture = "da-DK", Segment = "segment-1" }
            ],
            Properties =
            [
                new PropertyValueModel { Alias = "title", Value = "Hello Segment 1", Culture = "en-US", Segment = "segment-1" },
                new PropertyValueModel { Alias = "title", Value = "Hello Segment 2", Culture = "en-US", Segment = "segment-2" },
                new PropertyValueModel { Alias = "title", Value = "Hej Segment 1", Culture = "da-DK", Segment = "segment-1" }
            ],
        };

        var result = ContentValidationService.GetPopulatedSegmentCultures(model, ["en-US", "da-DK"]);

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.ContainsKey("segment-1"), Is.True);
        Assert.That(result.ContainsKey("segment-2"), Is.True);
        Assert.That(result["segment-1"], Does.Contain("en-US"));
        Assert.That(result["segment-1"], Does.Contain("da-DK"));
        Assert.That(result["segment-2"], Does.Contain("en-US"));
        Assert.That(result["segment-2"], Does.Not.Contain("da-DK"));
    }
}
