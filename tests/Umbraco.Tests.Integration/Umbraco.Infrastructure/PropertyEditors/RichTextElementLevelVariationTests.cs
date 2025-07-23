using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

// NOTE: These tests are in place to ensure that element level variation works for Rich Text. Element level variation
//       is tested more in-depth for Block List (see BlockListElementLevelVariationTests), but since the actual
//       implementation is shared between Block List and Rich Text, we won't repeat all those tests here.
internal sealed class RichTextElementLevelVariationTests : BlockEditorElementVariationTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public async Task Can_Publish_Cultures_Independently()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues(
            "en-US",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant content value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first content value in English", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The first invariant content value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first content value in English", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The first invariant content value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first content value in English", element3.Properties["variantText"]);
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant settings value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first settings value in English", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The first invariant settings value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first settings value in English", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The first invariant settings value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first settings value in English", element3.Properties["variantText"]);
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant content value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first content value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The first invariant content value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first content value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The first invariant content value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first content value in Danish", element3.Properties["variantText"]);
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant settings value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first settings value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The first invariant settings value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first settings value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The first invariant settings value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first settings value in Danish", element3.Properties["variantText"]);
            });

        richTextValue = JsonSerializer.Deserialize<RichTextEditorValue>((string)content.Properties["blocks"]!.GetValue()!);
        for (var i = 0; i < 3; i++)
        {
            richTextValue.Blocks.ContentData[i].Values[0].Value = $"#{i + 1}: The second invariant content value";
            richTextValue.Blocks.ContentData[i].Values[1].Value = $"#{i + 1}: The second content value in English";
            richTextValue.Blocks.ContentData[i].Values[2].Value = $"#{i + 1}: The second content value in Danish";
            richTextValue.Blocks.SettingsData[i].Values[0].Value = $"#{i + 1}: The second invariant settings value";
            richTextValue.Blocks.SettingsData[i].Values[1].Value = $"#{i + 1}: The second settings value in English";
            richTextValue.Blocks.SettingsData[i].Values[2].Value = $"#{i + 1}: The second settings value in Danish";
        }

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValues(
            "en-US",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The second content value in English", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant content value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The second content value in English", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant content value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The second content value in English", element3.Properties["variantText"]);
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The second settings value in English", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant settings value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The second settings value in English", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant settings value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The second settings value in English", element3.Properties["variantText"]);
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first content value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant content value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first content value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant content value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first content value in Danish", element3.Properties["variantText"]);
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The first settings value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant settings value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The first settings value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant settings value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The first settings value in Danish", element3.Properties["variantText"]);
            });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The second content value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant content value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The second content value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant content value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The second content value in Danish", element3.Properties["variantText"]);
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Properties["invariantText"]);
                Assert.AreEqual("#1: The second settings value in Danish", element1.Properties["variantText"]);
                Assert.AreEqual("#2: The second invariant settings value", element2.Properties["invariantText"]);
                Assert.AreEqual("#2: The second settings value in Danish", element2.Properties["variantText"]);
                Assert.AreEqual("#3: The second invariant settings value", element3.Properties["invariantText"]);
                Assert.AreEqual("#3: The second settings value in Danish", element3.Properties["variantText"]);
            });

        void AssertPropertyValues(
            string culture,
            Action<IApiElement, IApiElement, IApiElement> validateBlockContentValues,
            Action<IApiElement, IApiElement, IApiElement> validateBlockSettingsValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.IsNotNull(property);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.IsNotNull(propertyValue);

            var blocks = propertyValue.Blocks.ToArray();
            Assert.AreEqual(3, blocks.Length);

            Assert.Multiple(() =>
            {
                validateBlockContentValues(blocks[0].Content, blocks[1].Content, blocks[2].Content);
                validateBlockSettingsValues(blocks[0].Settings, blocks[1].Settings, blocks[2].Settings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Removed()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 3, blocks => { });
        AssertPropertyValues("da-DK", 3, blocks => { });

        richTextValue = JsonSerializer.Deserialize<RichTextEditorValue>((string)content.Properties["blocks"]!.GetValue()!);

        // remove block #2
        var richTextBlockLayout = richTextValue.Blocks!.Layout.First();
        richTextValue.Blocks.Layout[richTextBlockLayout.Key] =
        [
            richTextBlockLayout.Value.First(),
            richTextBlockLayout.Value.Last()
        ];
        var contentKey = richTextValue.Blocks.ContentData[1].Key;
        richTextValue.Blocks.ContentData.RemoveAt(1);
        richTextValue.Blocks.SettingsData.RemoveAt(1);
        richTextValue.Blocks.Expose.RemoveAll(v => v.ContentKey == contentKey);
        Assert.AreEqual(4, richTextValue.Blocks.Expose.Count);

        richTextValue.Blocks.ContentData[0].Values[0].Value = "#1: The second invariant content value";
        richTextValue.Blocks.ContentData[0].Values[1].Value = "#1: The second content value in English";
        richTextValue.Blocks.ContentData[0].Values[2].Value = "#1: The second content value in Danish";
        richTextValue.Blocks.ContentData[1].Values[0].Value = "#3: The second invariant content value";
        richTextValue.Blocks.ContentData[1].Values[1].Value = "#3: The second content value in English";
        richTextValue.Blocks.ContentData[1].Values[2].Value = "#3: The second content value in Danish";
        richTextValue.Blocks.SettingsData[0].Values[0].Value = "#1: The second invariant settings value";
        richTextValue.Blocks.SettingsData[0].Values[1].Value = "#1: The second settings value in English";
        richTextValue.Blocks.SettingsData[0].Values[2].Value = "#1: The second settings value in Danish";
        richTextValue.Blocks.SettingsData[1].Values[0].Value = "#3: The second invariant settings value";
        richTextValue.Blocks.SettingsData[1].Values[1].Value = "#3: The second settings value in English";
        richTextValue.Blocks.SettingsData[1].Values[2].Value = "#3: The second settings value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Properties["invariantText"]);
            Assert.AreEqual("#1: The second content value in English", blocks[0].Content.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Properties["invariantText"]);
            Assert.AreEqual("#3: The second content value in English", blocks[1].Content.Properties["variantText"]);

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#1: The second settings value in English", blocks[0].Settings.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant settings value", blocks[1].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#3: The second settings value in English", blocks[1].Settings.Properties["variantText"]);
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Properties["invariantText"]);
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Properties["invariantText"]);
            Assert.AreEqual("#3: The first content value in Danish", blocks[1].Content.Properties["variantText"]);

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#1: The first settings value in Danish", blocks[0].Settings.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant settings value", blocks[1].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#3: The first settings value in Danish", blocks[1].Settings.Properties["variantText"]);
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Properties["invariantText"]);
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Properties["invariantText"]);
            Assert.AreEqual("#3: The second content value in Danish", blocks[1].Content.Properties["variantText"]);

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#1: The second settings value in Danish", blocks[0].Settings.Properties["variantText"]);
            Assert.AreEqual("#3: The second invariant settings value", blocks[1].Settings!.Properties["invariantText"]);
            Assert.AreEqual("#3: The second settings value in Danish", blocks[1].Settings.Properties["variantText"]);
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<ApiBlockItem[]> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.IsNotNull(property);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.IsNotNull(propertyValue);

            var blocks = propertyValue.Blocks.ToArray();
            Assert.AreEqual(numberOfExpectedBlocks, blocks.Length);

            validateBlocks(blocks);
        }
    }

    [Test]
    public async Task Markup_Follows_Invariance()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValuesForAllCultures(markup =>
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(markup.Contains("<p>Some text.</p>"));
                Assert.IsTrue(markup.Contains("<p>More text.</p>"));
                Assert.IsTrue(markup.Contains("<p>Even more text.</p>"));
                Assert.IsTrue(markup.Contains("<p>The end.</p>"));
            });
        });

        richTextValue = JsonSerializer.Deserialize<RichTextEditorValue>((string)content.Properties["blocks"]!.GetValue()!);
        richTextValue.Markup = richTextValue.Markup
            .Replace("Some text", "Some text updated")
            .Replace("More text", "More text updated")
            .Replace("Even more text", "Even more text updated")
            .Replace("The end", "The end updated");

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValuesForAllCultures(markup =>
        {
            Assert.Multiple(() =>
            {
                Assert.IsFalse(markup.Contains("<p>Some text.</p>"));
                Assert.IsFalse(markup.Contains("<p>More text.</p>"));
                Assert.IsFalse(markup.Contains("<p>Even more text.</p>"));
                Assert.IsFalse(markup.Contains("<p>The end.</p>"));
                Assert.IsTrue(markup.Contains("<p>Some text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>More text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>Even more text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>The end updated.</p>"));
            });
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValuesForAllCultures(markup =>
        {
            Assert.Multiple(() =>
            {
                Assert.IsFalse(markup.Contains("<p>Some text.</p>"));
                Assert.IsFalse(markup.Contains("<p>More text.</p>"));
                Assert.IsFalse(markup.Contains("<p>Even more text.</p>"));
                Assert.IsFalse(markup.Contains("<p>The end.</p>"));
                Assert.IsTrue(markup.Contains("<p>Some text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>More text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>Even more text updated.</p>"));
                Assert.IsTrue(markup.Contains("<p>The end updated.</p>"));
            });
        });

        void AssertPropertyValuesForAllCultures(Action<string> validateMarkup)
        {
            foreach (var culture in new[] { "en-US", "da-DK" })
            {
                SetVariationContext(culture, null);
                var publishedContent = GetPublishedContent(content.Key);
                var property = publishedContent.GetProperty("blocks");
                Assert.IsNotNull(property);

                var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
                Assert.IsNotNull(propertyValue);

                Assert.IsNotEmpty(propertyValue.Markup);
                validateMarkup(propertyValue.Markup);
            }
        }
    }

    [Test]
    public async Task Can_Publish_Without_Blocks_Variant()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = new RichTextEditorValue { Markup = "<p>Markup here</p>", Blocks = null };
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US");
        AssertPropertyValues("da-DK");

        void AssertPropertyValues(string culture)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.IsNotNull(property);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.IsNotNull(propertyValue);
            Assert.AreEqual("<p>Markup here</p>", propertyValue.Markup);
            Assert.IsEmpty(propertyValue.Blocks);
        }
    }

    [Test]
    public async Task Can_Publish_Without_Blocks_Invariant()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Nothing, rteDataType);
        var richTextValue = new RichTextEditorValue { Markup = "<p>Markup here</p>", Blocks = null };
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["*"]);

        AssertPropertyValues("en-US");
        AssertPropertyValues("da-DK");

        void AssertPropertyValues(string culture)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.IsNotNull(property);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.IsNotNull(propertyValue);
            Assert.AreEqual("<p>Markup here</p>", propertyValue.Markup);
            Assert.IsEmpty(propertyValue.Blocks);
        }
    }

    [Test]
    public async Task Can_Index_Cultures_Independently_Invariant_Blocks()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);
        PublishContent(content, ["en-US", "da-DK"]);

        var editor = rteDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: true,
            availableCultures: ["en-US", "da-DK"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            });

        Assert.AreEqual(3, indexValues.Count());
        Assert.NotNull(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)));

        AssertIndexedValues(
            "en-US",
            "Some text.",
            "More text.",
            "Even more text.",
            "The end.",
            "#1: The first invariant content value",
            "#1: The first content value in English",
            "#2: The first invariant content value",
            "#2: The first content value in English",
            "#3: The first invariant content value",
            "#3: The first content value in English");

        AssertIndexedValues(
            "da-DK",
            "Some text.",
            "More text.",
            "Even more text.",
            "The end.",
            "#1: The first invariant content value",
            "#1: The first content value in Danish",
            "#2: The first invariant content value",
            "#2: The first content value in Danish",
            "#3: The first invariant content value",
            "#3: The first content value in Danish");

        void AssertIndexedValues(string culture, params string[] expectedIndexedValues)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture.InvariantEquals(culture));
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.AreEqual(expectedIndexedValues.Length, values.Length);
            Assert.IsTrue(values.ContainsAll(expectedIndexedValues));
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Index_With_Unexposed_Blocks(bool published)
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        richTextValue.Blocks!.Expose.RemoveAll(e => e.Culture == "da-DK");

        var content = CreateContent(contentType, richTextValue);
        PublishContent(content, ["en-US", "da-DK"]);

        var editor = rteDataType.Editor!;
        var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
            content.Properties["blocks"]!,
            culture: null,
            segment: null,
            published: published,
            availableCultures: ["en-US", "da-DK"],
            contentTypeDictionary: new Dictionary<Guid, IContentType>
            {
                { elementType.Key, elementType }, { contentType.Key, contentType }
            });

        Assert.AreEqual(3, indexValues.Count());
        Assert.NotNull(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)));

        if (published)
        {
            AssertIndexedValues(
                "da-DK",
                "Some text.",
                "More text.",
                "Even more text.",
                "The end.");
        }
        else
        {
            AssertIndexedValues(
                "da-DK",
                "Some text.",
                "More text.",
                "Even more text.",
                "The end.",
                "#1: The first invariant content value",
                "#1: The first content value in Danish",
                "#2: The first invariant content value",
                "#2: The first content value in Danish",
                "#3: The first invariant content value",
                "#3: The first content value in Danish");
        }

        AssertIndexedValues(
            "en-US",
            "Some text.",
            "More text.",
            "Even more text.",
            "The end.",
            "#1: The first invariant content value",
            "#1: The first content value in English",
            "#2: The first invariant content value",
            "#2: The first content value in English",
            "#3: The first invariant content value",
            "#3: The first content value in English");

        void AssertIndexedValues(string culture, params string[] expectedIndexedValues)
        {
            var indexValue = indexValues.FirstOrDefault(v => v.Culture.InvariantEquals(culture));
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.AreEqual(expectedIndexedValues.Length, values.Length);
            Assert.IsTrue(values.ContainsAll(expectedIndexedValues));
        }
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Nothing)]
    public async Task Can_Index_Cultures_Independently_Variant_Blocks(ContentVariation elementTypeVariation)
    {
        var elementType = CreateElementType(elementTypeVariation);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(ContentVariation.Culture, rteDataType, ContentVariation.Culture);

        var englishRichTextValue = CreateInvariantRichTextValue("en-US");
        var danishRichTextValue = CreateInvariantRichTextValue("da-DK");

        var content = CreateContent(contentType);
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(englishRichTextValue), "en-US");
        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(danishRichTextValue), "da-DK");
        ContentService.Save(content);

        PublishContent(content, ["en-US", "da-DK"]);

        var editor = rteDataType.Editor!;

        AssertIndexedValues(
            "en-US",
            "Some text for en-US.",
            "More text for en-US.",
            "invariantText value for en-US",
            "variantText value for en-US");

        AssertIndexedValues(
            "da-DK",
            "Some text for da-DK.",
            "More text for da-DK.",
            "invariantText value for da-DK",
            "variantText value for da-DK");

        void AssertIndexedValues(string culture, params string[] expectedIndexedValues)
        {
            var indexValues = editor.PropertyIndexValueFactory.GetIndexValues(
                content.Properties["blocks"]!,
                culture: culture,
                segment: null,
                published: true,
                availableCultures: ["en-US", "da-DK"],
                contentTypeDictionary: new Dictionary<Guid, IContentType>
                {
                    { elementType.Key, elementType }, { contentType.Key, contentType }
                });

            Assert.AreEqual(2, indexValues.Count());
            Assert.NotNull(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)));

            var indexValue = indexValues.FirstOrDefault(v => v.Culture.InvariantEquals(culture) && v.FieldName == "blocks");
            Assert.IsNotNull(indexValue);
            Assert.AreEqual(1, indexValue.Values.Count());
            var indexedValue = indexValue.Values.First() as string;
            Assert.IsNotNull(indexedValue);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.AreEqual(expectedIndexedValues.Length, values.Length);
            Assert.IsTrue(values.ContainsAll(expectedIndexedValues));
        }

        RichTextEditorValue CreateInvariantRichTextValue(string culture)
        {
            var contentElementKey = Guid.NewGuid();
            return new RichTextEditorValue
            {
                Markup = $"""
                          <p>Some text for {culture}.</p>
                          <umb-rte-block data-content-key="{contentElementKey:D}"><!--Umbraco-Block--></umb-rte-block>
                          <p>More text for {culture}.</p>
                          """,
                Blocks = new RichTextBlockValue([
                    new RichTextBlockLayoutItem(contentElementKey)
                ])
                {
                    ContentData =
                    [
                        new(contentElementKey, elementType.Key, elementType.Alias)
                        {
                            Values =
                            [
                                new() { Alias = "invariantText", Value = $"invariantText value for {culture}" },
                                new() { Alias = "variantText", Value = $"variantText value for {culture}" }
                            ]
                        }
                    ],
                    SettingsData = [],
                    Expose =
                    [
                        new(contentElementKey, culture, null),
                    ]
                }
            };
        }
    }

    private async Task<IDataType> CreateRichTextDataType(IContentType elementType)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.RichText,
            new RichTextConfiguration.RichTextBlockConfiguration[]
            {
                new()
                {
                    ContentElementTypeKey = elementType.Key,
                    SettingsElementTypeKey = elementType.Key,
                }
            });

    private IContentType CreateContentType(IDataType blockListDataType)
        => CreateContentType(ContentVariation.Culture, blockListDataType);

    private RichTextEditorValue CreateRichTextValue(IContentType elementType)
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();
        var contentElementKey2 = Guid.NewGuid();
        var settingsElementKey2 = Guid.NewGuid();
        var contentElementKey3 = Guid.NewGuid();
        var settingsElementKey3 = Guid.NewGuid();

        return new RichTextEditorValue
        {
            Markup = $"""
                      <p>Some text.</p>
                      <umb-rte-block data-content-key="{contentElementKey1:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>More text.</p>
                      <umb-rte-block data-content-key="{contentElementKey2:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>Even more text.</p>
                      <umb-rte-block data-content-key="{contentElementKey3:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>The end.</p>
                      """,
            Blocks = new RichTextBlockValue([
                new RichTextBlockLayoutItem(contentElementKey1, settingsElementKey1),
                new RichTextBlockLayoutItem(contentElementKey2, settingsElementKey2),
                new RichTextBlockLayoutItem(contentElementKey3, settingsElementKey3),
            ])
            {
                ContentData =
                [
                    new(contentElementKey1, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
                        ]
                    },
                    new(contentElementKey2, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" }
                        ]
                    },
                    new(contentElementKey3, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#3: The first invariant content value" },
                            new() { Alias = "variantText", Value = "#3: The first content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#3: The first content value in Danish", Culture = "da-DK" }
                        ]
                    },
                ],
                SettingsData =
                [
                    new(settingsElementKey1, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" }
                        ]
                    },
                    new(settingsElementKey2, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" }
                        ]
                    },
                    new(settingsElementKey3, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "#3: The first invariant settings value" },
                            new() { Alias = "variantText", Value = "#3: The first settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "#3: The first settings value in Danish", Culture = "da-DK" }
                        ]
                    },
                ],
                Expose =
                [
                    new (contentElementKey1, "en-US", null),
                    new (contentElementKey1, "da-DK", null),
                    new (contentElementKey2, "en-US", null),
                    new (contentElementKey2, "da-DK", null),
                    new (contentElementKey3, "en-US", null),
                    new (contentElementKey3, "da-DK", null),
                ]
            }
        };
    }

    private IContent CreateContent(IContentType contentType, RichTextEditorValue? richTextValue = null)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType);

        if (contentType.VariesByCulture())
        {
            contentBuilder
                .WithCultureName("en-US", "Home (en)")
                .WithCultureName("da-DK", "Home (da)");
        }
        else
        {
            contentBuilder.WithName("Home");
        }

        var content = contentBuilder.Build();

        if (richTextValue is not null)
        {
            var propertyValue = JsonSerializer.Serialize(richTextValue);
            content.Properties["blocks"]!.SetValue(propertyValue);
        }

        ContentService.Save(content);
        return content;
    }
}
