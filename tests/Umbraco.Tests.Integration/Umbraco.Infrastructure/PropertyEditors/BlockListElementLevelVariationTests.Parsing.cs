using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

internal partial class BlockListElementLevelVariationTests
{
    [TestCase("en-US", "The culture variant content value in English", "The culture variant settings value in English")]
    [TestCase("da-DK", "The culture variant content value in Danish", "The culture variant settings value in Danish")]
    public async Task Can_Parse_Element_Level_Culture_Variations(string culture, string expectedVariantContentValue, string expectedVariantSettingsValue)
    {
        var publishedContent = await CreatePublishedContent(
            ContentVariation.Culture,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The culture variant content value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The culture variant content value in Danish", Culture = "da-DK" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The culture variant settings value in English", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The culture variant settings value in Danish", Culture = "da-DK" },
            });

        SetVariationContext(culture, null);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantContentValue, variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantSettingsValue, variantProperty.GetValue());
        });
    }

    [TestCase("segment1", "The segment variant content value for Segment1", "The segment variant settings value for Segment1")]
    [TestCase("segment2", "The segment variant content value for Segment2", "The segment variant settings value for Segment2")]
    public async Task Can_Parse_Element_Level_Segment_Variations(string segment, string expectedVariantContentValue, string expectedVariantSettingsValue)
    {
        var publishedContent = await CreatePublishedContent(
            ContentVariation.Segment,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The segment variant content value for Segment1", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The segment variant content value for Segment2", Segment = "segment2" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The segment variant settings value for Segment1", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The segment variant settings value for Segment2", Segment = "segment2" },
            });

        SetVariationContext(null, segment);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantContentValue, variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantSettingsValue, variantProperty.GetValue());
        });
    }

    [TestCase(
        "en-US",
        "segment1",
        "The variant content value in English for Segment1",
        "The variant settings value in English for Segment1")]
    [TestCase(
        "en-US",
        "segment2",
        "The variant content value in English for Segment2",
        "The variant settings value in English for Segment2")]
    [TestCase(
        "da-DK",
        "segment1",
        "The variant content value in Danish for Segment1",
        "The variant settings value in Danish for Segment1")]
    [TestCase(
        "da-DK",
        "segment2",
        "The variant content value in Danish for Segment2",
        "The variant settings value in Danish for Segment2")]
    public async Task Can_Parse_Element_Level_Culture_And_Segment_Variations(
        string culture,
        string segment,
        string expectedVariantContentValue,
        string expectedVariantSettingsValue)
    {
        var publishedContent = await CreatePublishedContent(
            ContentVariation.CultureAndSegment,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The variant content value in English for Segment1", Culture = "en-US", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The variant content value in English for Segment2", Culture = "en-US", Segment = "segment2" },
                new() { Alias = "variantText", Value = "The variant content value in Danish for Segment1", Culture = "da-DK", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The variant content value in Danish for Segment2", Culture = "da-DK", Segment = "segment2" }
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The variant settings value in English for Segment1", Culture = "en-US", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The variant settings value in English for Segment2", Culture = "en-US", Segment = "segment2" },
                new() { Alias = "variantText", Value = "The variant settings value in Danish for Segment1", Culture = "da-DK", Segment = "segment1" },
                new() { Alias = "variantText", Value = "The variant settings value in Danish for Segment2", Culture = "da-DK", Segment = "segment2" }
            });

        SetVariationContext(culture, segment);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantContentValue, variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(expectedVariantSettingsValue, variantProperty.GetValue());
        });
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public async Task Can_Be_Invariant(string culture)
    {
        var publishedContent = await CreatePublishedContent(
            ContentVariation.Nothing,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "Another invariant content value" }
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "Another invariant settings value" }
            });

        SetVariationContext(culture, null);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual("Another invariant content value", variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual("Another invariant settings value", variantProperty.GetValue());
        });
    }

    [TestCase("en-US", true)]
    [TestCase("da-DK", false)]
    public async Task Can_Become_Variant_After_Publish(string culture, bool expectExposedBlocks)
    {
        var elementType = CreateElementType(ContentVariation.Nothing);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "Another invariant content value" }
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "Another invariant settings value" }
            },
            true);

        // the content and element types are created invariant; now make them culture variant, and enable culture variance on the "variantText" property
        elementType.Variations = ContentVariation.Culture;
        elementType.PropertyTypes.First(pt => pt.Alias == "variantText").Variations = ContentVariation.Culture;
        ContentTypeService.Save(elementType);

        contentType.Variations = ContentVariation.Culture;
        ContentTypeService.Save(contentType);

        RefreshContentTypeCache(elementType, contentType);

        // to re-publish the content in both cultures we need to set the culture names
        content = ContentService.GetById(content.Key)!;
        content.SetCultureName("Home (en)", "en-US");
        content.SetCultureName("Home (da)", "da-DK");
        ContentService.Save(content);
        PublishContent(content, contentType);

        var publishedContent = GetPublishedContent(content.Key);

        SetVariationContext(culture, null);

        // the "blocks" property is invariant (at content level), and the block data currently stored is also invariant.
        // however, the content and element types both vary by culture at this point, so the blocks should be parsed
        // accordingly. this means that the block is exposed only in the default culture, and the "variantText" property
        // should perform a fallback to the default language (which is en-US).
        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(expectExposedBlocks ? 1 : 0, value.Count);

        if (expectExposedBlocks)
        {
            var blockListItem = value.First();
            Assert.AreEqual(2, blockListItem.Content.Properties.Count());
            Assert.Multiple(() =>
            {
                var invariantProperty = blockListItem.Content.Properties.First();
                Assert.IsFalse(invariantProperty.PropertyType.VariesByCulture());
                Assert.AreEqual("invariantText", invariantProperty.Alias);
                Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

                var variantProperty = blockListItem.Content.Properties.Last();
                Assert.IsTrue(variantProperty.PropertyType.VariesByCulture());
                Assert.AreEqual("variantText", variantProperty.Alias);
                Assert.AreEqual("Another invariant content value", variantProperty.GetValue());
            });

            Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
            Assert.Multiple(() =>
            {
                var invariantProperty = blockListItem.Settings.Properties.First();
                Assert.AreEqual("invariantText", invariantProperty.Alias);
                Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

                var variantProperty = blockListItem.Settings.Properties.Last();
                Assert.AreEqual("variantText", variantProperty.Alias);
                Assert.AreEqual("Another invariant settings value", variantProperty.GetValue());
            });
        }
    }

    [TestCase("en-US", "en-US", ContentVariation.Nothing)]
    [TestCase("en-US", "en-US", ContentVariation.Culture)]
    [TestCase("en-US", "da-DK", ContentVariation.Nothing)]
    [TestCase("en-US", "da-DK", ContentVariation.Culture)]
    [TestCase("da-DK", "en-US", ContentVariation.Nothing)]
    [TestCase("da-DK", "en-US", ContentVariation.Culture)]
    [TestCase("da-DK", "da-DK", ContentVariation.Nothing)]
    [TestCase("da-DK", "da-DK", ContentVariation.Culture)]
    public async Task Can_Become_Invariant_After_Publish(string requestCulture, string defaultCulture, ContentVariation elementVariationAfterPublish)
    {
        var language = await LanguageService.GetAsync(defaultCulture);
        language!.IsDefault = true;
        await LanguageService.UpdateAsync(language, Constants.Security.SuperUserKey);

        var elementType = CreateElementType(ContentVariation.Culture);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant content value" },
                new() { Alias = "variantText", Value = "The en-US content value", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The da-DK content value", Culture = "da-DK" },
            },
            new List<BlockPropertyValue>
            {
                new() { Alias = "invariantText", Value = "The invariant settings value" },
                new() { Alias = "variantText", Value = "The en-US settings value", Culture = "en-US" },
                new() { Alias = "variantText", Value = "The da-DK settings value", Culture = "da-DK" },
            },
            true);

        // the content and element types are created as variant; now update the element type according to the test case
        elementType.Variations = elementVariationAfterPublish;
        elementType.PropertyTypes.First(pt => pt.Alias == "variantText").Variations = elementVariationAfterPublish;
        ContentTypeService.Save(elementType);

        // ...and make the content type invariant
        contentType.Variations = ContentVariation.Nothing;
        ContentTypeService.Save(contentType);

        RefreshContentTypeCache(elementType, contentType);

        // to re-publish the content we need to set the invariant name
        content = ContentService.GetById(content.Key)!;
        content.Name = "Home";
        ContentService.Save(content);
        PublishContent(content, contentType);

        var publishedContent = GetPublishedContent(content.Key);

        SetVariationContext(requestCulture, null);

        // the "blocks" property is invariant (at content level), but the block data currently stored is variant because the
        // content type was originally variant. however, as the content type has changed to invariant, we expect no variance
        // in the rendered block output, despite the variance of the element (which may or may not vary by culture, depending
        // on the test case). this means that the "variantText" property should now always output the value set for the
        // default language (which is also depending on the test case).
        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant content value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual($"The {defaultCulture} content value", variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("The invariant settings value", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual($"The {defaultCulture} settings value", variantProperty.GetValue());
        });
    }

    [TestCase(ContentVariation.Nothing, "en-US", "English")]
    [TestCase(ContentVariation.Nothing, "da-DK", "Danish")]
    [TestCase(ContentVariation.Culture, "en-US", "English")]
    [TestCase(ContentVariation.Culture, "da-DK", "Danish")]
    [TestCase(ContentVariation.CultureAndSegment, "en-US", "English")]
    [TestCase(ContentVariation.CultureAndSegment, "da-DK", "Danish")]
    [TestCase(ContentVariation.Segment, "en-US", "English")]
    [TestCase(ContentVariation.Segment, "da-DK", "Danish")]
    public async Task Can_Handle_Both_Content_And_Element_Level_Variation(ContentVariation elementVariation, string culture, string expectedStartsWith)
    {
        var elementType = CreateElementType(elementVariation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, blockListDataType, ContentVariation.Culture);

        var content = CreateContent(
            contentType,
            elementType,
            new []
            {
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "English invariant content value" },
                        new() { Alias = "variantText", Value = "English variant content value", Culture = elementVariation.VariesByCulture() ? "en-US" : null }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "English invariant settings value" },
                        new() { Alias = "variantText", Value = "English variant settings value", Culture = elementVariation.VariesByCulture() ? "en-US" : null }
                    },
                    "en-US",
                    null),
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "Danish invariant content value" },
                        new() { Alias = "variantText", Value = "Danish variant content value", Culture = elementVariation.VariesByCulture() ? "da-DK" : null }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "Danish invariant settings value" },
                        new() { Alias = "variantText", Value = "Danish variant settings value", Culture = elementVariation.VariesByCulture() ? "da-DK" : null }
                    },
                    "da-DK",
                    null)
            },
            true);

        SetVariationContext(culture, null);

        var publishedContent = GetPublishedContent(content.Key);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.IsTrue(invariantProperty.GetValue()!.ToString()!.StartsWith(expectedStartsWith));

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.IsTrue(variantProperty.GetValue()!.ToString()!.StartsWith(expectedStartsWith));
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.IsTrue(invariantProperty.GetValue()!.ToString()!.StartsWith(expectedStartsWith));

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.IsTrue(variantProperty.GetValue()!.ToString()!.StartsWith(expectedStartsWith));
        });
    }

    [TestCase(ContentVariation.Culture, "en-US", "Segment1")]
    [TestCase(ContentVariation.Culture, "en-US", "Segment2")]
    [TestCase(ContentVariation.Culture, "da-DK", "Segment1")]
    [TestCase(ContentVariation.Culture, "da-DK", "Segment2")]
    [TestCase(ContentVariation.CultureAndSegment, "en-US", "Segment1")]
    [TestCase(ContentVariation.CultureAndSegment, "en-US", "Segment2")]
    [TestCase(ContentVariation.CultureAndSegment, "da-DK", "Segment1")]
    [TestCase(ContentVariation.CultureAndSegment, "da-DK", "Segment2")]
    [TestCase(ContentVariation.Segment, "en-US", "Segment1")]
    [TestCase(ContentVariation.Segment, "en-US", "Segment2")]
    [TestCase(ContentVariation.Segment, "da-DK", "Segment1")]
    [TestCase(ContentVariation.Segment, "da-DK", "Segment2")]
    public async Task Can_Handle_Variant_Element_For_Invariant_Content(ContentVariation elementVariation, string culture, string segment)
    {
        var elementType = CreateElementType(elementVariation);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, blockListDataType);

        var content = CreateContent(
            contentType,
            elementType,
            new []
            {
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant content text" },
                        new() { Alias = "variantText", Value = "This is also invariant content text" }
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant settings text" },
                        new() { Alias = "variantText", Value = "This is also invariant settings text" }
                    },
                    null,
                    null)
            },
            true);

        SetVariationContext(culture, segment);

        var publishedContent = GetPublishedContent(content.Key);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("This is invariant content text", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual("This is also invariant content text", variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual("This is invariant settings text", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual("This is also invariant settings text", variantProperty.GetValue());
        });
    }

    [TestCase("en-US", null)]
    [TestCase("en-US", "segment1")]
    [TestCase("en-US", "segment2")]
    [TestCase("da-DK", null)]
    [TestCase("da-DK", "segment1")]
    [TestCase("da-DK", "segment2")]
    public async Task Can_Combine_Element_Level_Segment_Variation_With_Document_Level_Language_Variation(string culture, string? segment)
    {
        var elementType = CreateElementType(ContentVariation.Segment);
        var blockListDataType = await CreateBlockListDataType(elementType);
        var contentType = CreateContentType(ContentVariation.CultureAndSegment, blockListDataType, ContentVariation.Culture);

        var content = CreateContent(
            contentType,
            elementType,
            new []
            {
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant content text for en-US" },
                        new() { Alias = "variantText", Value = "This is the default segment content text for en-US", Segment = null },
                        new() { Alias = "variantText", Value = "This is the segment1 segment content text for en-US", Segment = "segment1" },
                        new() { Alias = "variantText", Value = "This is the segment2 segment content text for en-US", Segment = "segment2" },
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant settings text for en-US" },
                        new() { Alias = "variantText", Value = "This is the default segment settings text for en-US", Segment = null },
                        new() { Alias = "variantText", Value = "This is the segment1 segment settings text for en-US", Segment = "segment1" },
                        new() { Alias = "variantText", Value = "This is the segment2 segment settings text for en-US", Segment = "segment2" }
                    },
                    "en-US",
                    null),
                new BlockProperty(
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant content text for da-DK" },
                        new() { Alias = "variantText", Value = "This is the default segment content text for da-DK", Segment = null },
                        new() { Alias = "variantText", Value = "This is the segment1 segment content text for da-DK", Segment = "segment1" },
                        new() { Alias = "variantText", Value = "This is the segment2 segment content text for da-DK", Segment = "segment2" },
                    },
                    new List<BlockPropertyValue>
                    {
                        new() { Alias = "invariantText", Value = "This is invariant settings text for da-DK" },
                        new() { Alias = "variantText", Value = "This is the default segment settings text for da-DK", Segment = null },
                        new() { Alias = "variantText", Value = "This is the segment1 segment settings text for da-DK", Segment = "segment1" },
                        new() { Alias = "variantText", Value = "This is the segment2 segment settings text for da-DK", Segment = "segment2" }
                    },
                    "da-DK",
                    null)
            },
            true);

        SetVariationContext(culture, segment);

        var publishedContent = GetPublishedContent(content.Key);

        var value = publishedContent.GetProperty("blocks")!.GetValue() as BlockListModel;
        Assert.IsNotNull(value);
        Assert.AreEqual(1, value.Count);

        var blockListItem = value.First();
        Assert.AreEqual(2, blockListItem.Content.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Content.Properties.First();
            Assert.AreEqual(ContentVariation.Nothing, invariantProperty.PropertyType.Variations);
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual($"This is invariant content text for {culture}", invariantProperty.GetValue());

            var variantProperty = blockListItem.Content.Properties.Last();
            Assert.AreEqual(ContentVariation.Segment, variantProperty.PropertyType.Variations);
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(
                segment is null ? $"This is the default segment content text for {culture}" : $"This is the {segment} segment content text for {culture}",
                variantProperty.GetValue());
        });

        Assert.AreEqual(2, blockListItem.Settings.Properties.Count());
        Assert.Multiple(() =>
        {
            var invariantProperty = blockListItem.Settings.Properties.First();
            Assert.AreEqual(ContentVariation.Nothing, invariantProperty.PropertyType.Variations);
            Assert.AreEqual("invariantText", invariantProperty.Alias);
            Assert.AreEqual($"This is invariant settings text for {culture}", invariantProperty.GetValue());

            var variantProperty = blockListItem.Settings.Properties.Last();
            Assert.AreEqual(ContentVariation.Segment, variantProperty.PropertyType.Variations);
            Assert.AreEqual("variantText", variantProperty.Alias);
            Assert.AreEqual(
                segment is null ? $"This is the default segment settings text for {culture}" : $"This is the {segment} segment settings text for {culture}",
                variantProperty.GetValue());
        });
    }
}
