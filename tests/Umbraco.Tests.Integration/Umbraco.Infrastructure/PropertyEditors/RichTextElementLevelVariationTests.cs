using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

// NOTE: These tests are in place to ensure that element level variation works for Rich Text. Element level variation
//       is tested more in-depth for Block List (see BlockListElementLevelVariationTests), but since the actual
//       implementation is shared between Block List and Rich Text, we won't repeat all those tests here.
public class RichTextElementLevelVariationTests : BlockEditorElementVariationTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public async Task Can_Publish_Cultures_Independently()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var blockGridDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(blockGridDataType);
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

        var blockGridDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(blockGridDataType);
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
        richTextValue.Blocks.ContentData.RemoveAt(1);
        richTextValue.Blocks.SettingsData.RemoveAt(1);

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

        var blockGridDataType = await CreateRichTextDataType(elementType);
        var contentType = CreateContentType(blockGridDataType);
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
                ]
            }
        };
    }

    private IContent CreateContent(IContentType contentType, RichTextEditorValue richTextValue)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)");

        var content = contentBuilder.Build();

        var propertyValue = JsonSerializer.Serialize(richTextValue);
        content.Properties["blocks"]!.SetValue(propertyValue);

        ContentService.Save(content);
        return content;
    }
}
