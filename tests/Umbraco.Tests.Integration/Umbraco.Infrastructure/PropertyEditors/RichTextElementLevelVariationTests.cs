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
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues(
            "en-US",
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The first invariant content value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first content value in English"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The first invariant content value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first content value in English"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The first invariant content value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first content value in English"));
            },
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The first invariant settings value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first settings value in English"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The first invariant settings value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first settings value in English"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The first invariant settings value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The first invariant content value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first content value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The first invariant content value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first content value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The first invariant content value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first content value in Danish"));
            },
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The first invariant settings value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first settings value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The first invariant settings value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first settings value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The first invariant settings value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first settings value in Danish"));
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
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The second content value in English"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant content value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The second content value in English"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The second content value in English"));
            },
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The second settings value in English"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant settings value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The second settings value in English"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The second settings value in English"));
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first content value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant content value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first content value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first content value in Danish"));
            },
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The first settings value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant settings value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The first settings value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The first settings value in Danish"));
            });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The second content value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant content value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The second content value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The second content value in Danish"));
            },
            (element1, element2, element3) =>
            {
                Assert.That(element1.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
                Assert.That(element1.Properties["variantText"], Is.EqualTo("#1: The second settings value in Danish"));
                Assert.That(element2.Properties["invariantText"], Is.EqualTo("#2: The second invariant settings value"));
                Assert.That(element2.Properties["variantText"], Is.EqualTo("#2: The second settings value in Danish"));
                Assert.That(element3.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
                Assert.That(element3.Properties["variantText"], Is.EqualTo("#3: The second settings value in Danish"));
            });

        void AssertPropertyValues(
            string culture,
            Action<IApiElement, IApiElement, IApiElement> validateBlockContentValues,
            Action<IApiElement, IApiElement, IApiElement> validateBlockSettingsValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.That(property, Is.Not.Null);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.That(propertyValue, Is.Not.Null);

            var blocks = propertyValue.Blocks.ToArray();
            Assert.That(blocks, Has.Length.EqualTo(3));

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
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
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
        Assert.That(richTextValue.Blocks.Expose, Has.Count.EqualTo(4));

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
            Assert.That(blocks[0].Content.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Properties["variantText"], Is.EqualTo("#1: The second content value in English"));
            Assert.That(blocks[1].Content.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Properties["variantText"], Is.EqualTo("#3: The second content value in English"));

            Assert.That(blocks[0].Settings!.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
            Assert.That(blocks[0].Settings.Properties["variantText"], Is.EqualTo("#1: The second settings value in English"));
            Assert.That(blocks[1].Settings!.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
            Assert.That(blocks[1].Settings.Properties["variantText"], Is.EqualTo("#3: The second settings value in English"));
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.That(blocks[0].Content.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Properties["variantText"], Is.EqualTo("#1: The first content value in Danish"));
            Assert.That(blocks[1].Content.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Properties["variantText"], Is.EqualTo("#3: The first content value in Danish"));

            Assert.That(blocks[0].Settings!.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
            Assert.That(blocks[0].Settings.Properties["variantText"], Is.EqualTo("#1: The first settings value in Danish"));
            Assert.That(blocks[1].Settings!.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
            Assert.That(blocks[1].Settings.Properties["variantText"], Is.EqualTo("#3: The first settings value in Danish"));
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.That(blocks[0].Content.Properties["invariantText"], Is.EqualTo("#1: The second invariant content value"));
            Assert.That(blocks[0].Content.Properties["variantText"], Is.EqualTo("#1: The second content value in Danish"));
            Assert.That(blocks[1].Content.Properties["invariantText"], Is.EqualTo("#3: The second invariant content value"));
            Assert.That(blocks[1].Content.Properties["variantText"], Is.EqualTo("#3: The second content value in Danish"));

            Assert.That(blocks[0].Settings!.Properties["invariantText"], Is.EqualTo("#1: The second invariant settings value"));
            Assert.That(blocks[0].Settings.Properties["variantText"], Is.EqualTo("#1: The second settings value in Danish"));
            Assert.That(blocks[1].Settings!.Properties["invariantText"], Is.EqualTo("#3: The second invariant settings value"));
            Assert.That(blocks[1].Settings.Properties["variantText"], Is.EqualTo("#3: The second settings value in Danish"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<ApiBlockItem[]> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.That(property, Is.Not.Null);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.That(propertyValue, Is.Not.Null);

            var blocks = propertyValue.Blocks.ToArray();
            Assert.That(blocks, Has.Length.EqualTo(numberOfExpectedBlocks));

            validateBlocks(blocks);
        }
    }

    [Test]
    public async Task Markup_Follows_Invariance()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
        var richTextValue = CreateRichTextValue(elementType);
        var content = CreateContent(contentType, richTextValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValuesForAllCultures(markup =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Contain("<p>Some text.</p>"));
                Assert.That(markup, Does.Contain("<p>More text.</p>"));
                Assert.That(markup, Does.Contain("<p>Even more text.</p>"));
                Assert.That(markup, Does.Contain("<p>The end.</p>"));
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
                Assert.That(markup, Does.Not.Contain("<p>Some text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>More text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>Even more text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>The end.</p>"));
                Assert.That(markup, Does.Contain("<p>Some text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>More text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>Even more text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>The end updated.</p>"));
            });
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValuesForAllCultures(markup =>
        {
            Assert.Multiple(() =>
            {
                Assert.That(markup, Does.Not.Contain("<p>Some text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>More text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>Even more text.</p>"));
                Assert.That(markup, Does.Not.Contain("<p>The end.</p>"));
                Assert.That(markup, Does.Contain("<p>Some text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>More text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>Even more text updated.</p>"));
                Assert.That(markup, Does.Contain("<p>The end updated.</p>"));
            });
        });

        void AssertPropertyValuesForAllCultures(Action<string> validateMarkup)
        {
            foreach (var culture in new[] { "en-US", "da-DK" })
            {
                SetVariationContext(culture, null);
                var publishedContent = GetPublishedContent(content.Key);
                var property = publishedContent.GetProperty("blocks");
                Assert.That(property, Is.Not.Null);

                var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
                Assert.That(propertyValue, Is.Not.Null);

                Assert.That(propertyValue.Markup, Is.Not.Empty);
                validateMarkup(propertyValue.Markup);
            }
        }
    }

    [Test]
    public async Task Can_Publish_Without_Blocks_Variant()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
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
            Assert.That(property, Is.Not.Null);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.That(propertyValue, Is.Not.Null);
            Assert.That(propertyValue.Markup, Is.EqualTo("<p>Markup here</p>"));
            Assert.That(propertyValue.Blocks, Is.Empty);
        }
    }

    [Test]
    public async Task Can_Publish_Without_Blocks_Invariant()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Nothing, rteDataType);
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
            Assert.That(property, Is.Not.Null);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.That(propertyValue, Is.Not.Null);
            Assert.That(propertyValue.Markup, Is.EqualTo("<p>Markup here</p>"));
            Assert.That(propertyValue.Blocks, Is.Empty);
        }
    }

    [Test]
    public async Task Can_Index_Cultures_Independently_Invariant_Blocks()
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
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

        Assert.That(indexValues.Count(), Is.EqualTo(3));
        Assert.That(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)), Is.Not.Null);

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
            Assert.That(indexValue, Is.Not.Null);
            Assert.That(indexValue.Values.Count(), Is.EqualTo(1));
            var indexedValue = indexValue.Values.First() as string;
            Assert.That(indexedValue, Is.Not.Null);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.That(values, Has.Length.EqualTo(expectedIndexedValues.Length));
            Assert.That(values.ContainsAll(expectedIndexedValues), Is.True);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public async Task Can_Index_With_Unexposed_Blocks(bool published)
    {
        var elementType = await CreateElementType(ContentVariation.Culture);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);
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

        Assert.That(indexValues.Count(), Is.EqualTo(3));
        Assert.That(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)), Is.Not.Null);

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
            Assert.That(indexValue, Is.Not.Null);
            Assert.That(indexValue.Values.Count(), Is.EqualTo(1));
            var indexedValue = indexValue.Values.First() as string;
            Assert.That(indexedValue, Is.Not.Null);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.That(values, Has.Length.EqualTo(expectedIndexedValues.Length));
            Assert.That(values.ContainsAll(expectedIndexedValues), Is.True);
        }
    }

    [TestCase(ContentVariation.Culture)]
    [TestCase(ContentVariation.Nothing)]
    public async Task Can_Index_Cultures_Independently_Variant_Blocks(ContentVariation elementTypeVariation)
    {
        var elementType = await CreateElementType(elementTypeVariation);

        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(ContentVariation.Culture, rteDataType, ContentVariation.Culture);

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

            Assert.That(indexValues.Count(), Is.EqualTo(2));
            Assert.That(indexValues.FirstOrDefault(value => value.FieldName.StartsWith(UmbracoExamineFieldNames.RawFieldPrefix)), Is.Not.Null);

            var indexValue = indexValues.FirstOrDefault(v => v.Culture.InvariantEquals(culture) && v.FieldName == "blocks");
            Assert.That(indexValue, Is.Not.Null);
            Assert.That(indexValue.Values.Count(), Is.EqualTo(1));
            var indexedValue = indexValue.Values.First() as string;
            Assert.That(indexedValue, Is.Not.Null);
            var values = indexedValue.Split(Environment.NewLine).Select(s => s.Trim()).Where(s => s.IsNullOrWhiteSpace() is false).ToArray();
            Assert.That(values, Has.Length.EqualTo(expectedIndexedValues.Length));
            Assert.That(values.ContainsAll(expectedIndexedValues), Is.True);
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

    private async Task<IContentType> CreateContentType(IDataType blockListDataType)
        => await CreateContentType(ContentVariation.Culture, blockListDataType);

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

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Publishing_After_Changing_Element_Property_From_Variant_To_Invariant_Does_Not_Keep_Old_Culture_Specific_Values(bool republishEnglish, bool republishDanish)
    {
        // 1. Create element type WITH culture variation
        var elementType = await CreateElementType(ContentVariation.Culture);
        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);

        // 2. Create a simple rich text value with a single block for clarity
        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();
        var richTextValue = new RichTextEditorValue
        {
            Markup = $"""
                      <p>Some text.</p>
                      <umb-rte-block data-content-key="{contentElementKey:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>More text.</p>
                      """,
            Blocks = new RichTextBlockValue([
                new RichTextBlockLayoutItem(contentElementKey, settingsElementKey)
            ])
            {
                ContentData =
                [
                    new(contentElementKey, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "The invariant content value" },
                            new() { Alias = "variantText", Value = "The content value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The content value in Danish", Culture = "da-DK" }
                        ]
                    }
                ],
                SettingsData =
                [
                    new(settingsElementKey, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "The invariant settings value" },
                            new() { Alias = "variantText", Value = "The settings value in English", Culture = "en-US" },
                            new() { Alias = "variantText", Value = "The settings value in Danish", Culture = "da-DK" }
                        ]
                    }
                ],
                Expose =
                [
                    new(contentElementKey, "en-US", null),
                    new(contentElementKey, "da-DK", null)
                ]
            }
        };

        var content = CreateContent(contentType, richTextValue);
        PublishContent(content, ["en-US", "da-DK"]);

        // 3. Change element property type to invariant (remove culture variation)
        foreach (var propertyType in elementType.PropertyTypes.Where(pt => pt.Alias == "variantText"))
        {
            propertyType.Variations = ContentVariation.Nothing;
        }

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // 4. Update the content values to be invariant
        richTextValue = JsonSerializer.Deserialize<RichTextEditorValue>((string)content.Properties["blocks"]!.GetValue()!)!;

        richTextValue.Blocks!.ContentData[0].Values.RemoveAll(value => value.Alias == "variantText");
        richTextValue.Blocks.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The invariant content value",
            Culture = null
        });

        richTextValue.Blocks.SettingsData[0].Values.RemoveAll(value => value.Alias == "variantText");
        richTextValue.Blocks.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The invariant settings value",
            Culture = null
        });

        richTextValue.Blocks.Expose = richTextValue.Blocks.Expose
            .Select(e => new BlockItemVariation(e.ContentKey, null, null))
            .DistinctBy(e => e.ContentKey)
            .ToList();

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);

        // 5. Publish selected cultures
        string[] culturesToPublish = republishEnglish && republishDanish
            ? ["en-US", "da-DK"]
            : republishEnglish
                ? ["en-US"]
                : republishDanish
                    ? ["da-DK"]
                    : throw new ArgumentException("Can't proceed without republishing at least one culture");
        PublishContent(content, culturesToPublish);

        // 6. Verify published JSON doesn't contain old culture-specific values
        content = ContentService.GetById(content.Key)!;
        var publishedValue = (string?)content.Properties["blocks"]!.GetValue(null, null, published: true);
        Assert.That(publishedValue, Is.Not.Null, "Published value should not be null");

        var publishedRichTextValue = JsonSerializer.Deserialize<RichTextEditorValue>(publishedValue);
        Assert.That(publishedRichTextValue?.Blocks, Is.Not.Null);

        // Verify ContentData entries are not duplicated
        Assert.That(publishedRichTextValue.Blocks.ContentData, Has.Count.EqualTo(1), "Should have exactly 1 content data entry");
        Assert.That(publishedRichTextValue.Blocks.SettingsData, Has.Count.EqualTo(1), "Should have exactly 1 settings data entry");

        var aliasGroups = publishedRichTextValue.Blocks.ContentData[0].Values.GroupBy(v => v.Alias);
        foreach (var group in aliasGroups)
        {
            Assert.That(
                group.Count(), Is.EqualTo(1),
                $"Property '{group.Key}' has multiple values. Values: {string.Join(", ", group.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");
        }

        aliasGroups = publishedRichTextValue.Blocks.SettingsData[0].Values.GroupBy(v => v.Alias);
        foreach (var group in aliasGroups)
        {
            Assert.That(
                group.Count(), Is.EqualTo(1),
                $"Property '{group.Key}' has multiple values. Values: {string.Join(", ", group.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");
        }

        // Verify Expose entries are not duplicated
        Assert.That(publishedRichTextValue.Blocks.Expose, Has.Count.EqualTo(1));
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public async Task Publishing_After_Changing_Element_Property_From_Invariant_To_Variant_Does_Not_Keep_Old_Invariant_Values(bool republishEnglish, bool republishDanish)
    {
        // 1. Create variant element type WITHOUT variant properties
        var elementType = await CreateElementType(ContentVariation.Culture);
        elementType.PropertyTypes.First(p => p.Alias == "variantText").Variations = ContentVariation.Nothing;
        await ContentTypeService.UpdateAsync(elementType, Constants.Security.SuperUserKey);
        var rteDataType = await CreateRichTextDataType(elementType);
        var contentType = await CreateContentType(rteDataType);

        // 2. Create a simple rich text value with a single block
        var contentElementKey = Guid.NewGuid();
        var settingsElementKey = Guid.NewGuid();
        var richTextValue = new RichTextEditorValue
        {
            Markup = $"""
                      <p>Some text.</p>
                      <umb-rte-block data-content-key="{contentElementKey:D}"><!--Umbraco-Block--></umb-rte-block>
                      <p>More text.</p>
                      """,
            Blocks = new RichTextBlockValue([
                new RichTextBlockLayoutItem(contentElementKey, settingsElementKey)
            ])
            {
                ContentData =
                [
                    new(contentElementKey, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "The invariant content value" },
                            new() { Alias = "variantText", Value = "The original invariant value for content" }
                        ]
                    }
                ],
                SettingsData =
                [
                    new(settingsElementKey, elementType.Key, elementType.Alias)
                    {
                        Values =
                        [
                            new() { Alias = "invariantText", Value = "The invariant settings value" },
                            new() { Alias = "variantText", Value = "The original invariant value for settings" }
                        ]
                    }
                ],
                Expose =
                [
                    new(contentElementKey, "en-US", null),
                    new(contentElementKey, "da-DK", null),
                ]
            }
        };

        var content = CreateContent(contentType, richTextValue);
        PublishContent(content, ["en-US", "da-DK"]);

        // Verify initial state - both cultures should see the same invariant value
        AssertPropertyValues(
            "en-US",
            "The invariant content value",
            "The original invariant value for content",
            "The invariant settings value",
            "The original invariant value for settings");

        AssertPropertyValues(
            "da-DK",
            "The invariant content value",
            "The original invariant value for content",
            "The invariant settings value",
            "The original invariant value for settings");

        // 3. Change element type to variant (add culture variation)
        elementType.Variations = ContentVariation.Culture;
        foreach (var propertyType in elementType.PropertyTypes.Where(pt => pt.Alias == "variantText"))
        {
            propertyType.Variations = ContentVariation.Culture;
        }

        await ContentTypeService.CreateAsync(elementType, Constants.Security.SuperUserKey);

        // 4. Update the content values to have culture-specific values
        richTextValue = JsonSerializer.Deserialize<RichTextEditorValue>((string)content.Properties["blocks"]!.GetValue()!)!;
        richTextValue.Blocks!.ContentData[0].Values.RemoveAll(value => value.Alias == "variantText");
        richTextValue.Blocks.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The content value in English",
            Culture = "en-US"
        });
        richTextValue.Blocks.ContentData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The content value in Danish",
            Culture = "da-DK"
        });

        richTextValue.Blocks.SettingsData[0].Values.RemoveAll(value => value.Alias == "variantText");
        richTextValue.Blocks.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The settings value in English",
            Culture = "en-US"
        });
        richTextValue.Blocks.SettingsData[0].Values.Add(new BlockPropertyValue
        {
            Alias = "variantText",
            Value = "The settings value in Danish",
            Culture = "da-DK"
        });

        richTextValue.Blocks.Expose =
        [
            new BlockItemVariation(contentElementKey, "en-US", null),
            new BlockItemVariation(contentElementKey, "da-DK", null)
        ];

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(richTextValue));
        ContentService.Save(content);

        // 5. Publish selected cultures
        string[] culturesToPublish = republishEnglish && republishDanish
            ? ["en-US", "da-DK"]
            : republishEnglish
                ? ["en-US"]
                : republishDanish
                    ? ["da-DK"]
                    : throw new ArgumentException("Can't proceed without republishing at least one culture");
        PublishContent(content, culturesToPublish);

        // 6. Verify published JSON doesn't contain old invariant values for variantText
        content = ContentService.GetById(content.Key)!;
        var publishedValue = (string?)content.Properties["blocks"]!.GetValue(null, null, published: true);
        Assert.That(publishedValue, Is.Not.Null, "Published value should not be null");

        var publishedRichTextValue = JsonSerializer.Deserialize<RichTextEditorValue>(publishedValue);
        Assert.That(publishedRichTextValue?.Blocks, Is.Not.Null);

        // Verify ContentData entries are not duplicated
        Assert.That(publishedRichTextValue.Blocks.ContentData, Has.Count.EqualTo(1), "Should have exactly 1 content data entry");
        Assert.That(publishedRichTextValue.Blocks.SettingsData, Has.Count.EqualTo(1), "Should have exactly 1 settings data entry");

        var variantTextValues = publishedRichTextValue.Blocks.ContentData[0].Values.Where(v => v.Alias == "variantText").ToList();
        Assert.That(
            variantTextValues.Any(v => v.Culture is null), Is.False,
            $"variantText property should not have invariant values after changing to variant. Values: {string.Join(", ", variantTextValues.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");

        variantTextValues = publishedRichTextValue.Blocks.SettingsData[0].Values.Where(v => v.Alias == "variantText").ToList();
        Assert.That(
            variantTextValues.Any(v => v.Culture is null), Is.False,
            $"variantText property should not have invariant values after changing to variant. Values: {string.Join(", ", variantTextValues.Select(v => $"Culture={v.Culture ?? "null"}:Value={v.Value}"))}");

        // Verify Expose entries are not duplicated
        var exposeGroups = publishedRichTextValue.Blocks.Expose.GroupBy(e => (e.ContentKey, e.Culture, e.Segment));
        Assert.That(
            exposeGroups.All(g => g.Count() == 1), Is.True,
            $"Duplicate Expose entries found. Expose: {string.Join(", ", publishedRichTextValue.Blocks.Expose.Select(e => $"{e.ContentKey}:{e.Culture}:{e.Segment}"))}");

        void AssertPropertyValues(
            string culture,
            string expectedInvariantContentValue,
            string expectedVariantContentValue,
            string expectedInvariantSettingsValue,
            string expectedVariantSettingsValue)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);
            var property = publishedContent.GetProperty("blocks");
            Assert.That(property, Is.Not.Null);

            var propertyValue = property.GetDeliveryApiValue(false, culture) as RichTextModel;
            Assert.That(propertyValue, Is.Not.Null);

            var blocks = propertyValue.Blocks.ToArray();
            Assert.That(blocks, Has.Length.EqualTo(1));

            var apiBlockItem = blocks.First();
            Assert.Multiple(() =>
            {
                Assert.That(apiBlockItem.Content.Properties["invariantText"], Is.EqualTo(expectedInvariantContentValue));
                Assert.That(apiBlockItem.Content.Properties["variantText"], Is.EqualTo(expectedVariantContentValue));
            });

            Assert.Multiple(() =>
            {
                Assert.That(apiBlockItem.Settings!.Properties["invariantText"], Is.EqualTo(expectedInvariantSettingsValue));
                Assert.That(apiBlockItem.Settings.Properties["variantText"], Is.EqualTo(expectedVariantSettingsValue));
            });
        }
    }
}
