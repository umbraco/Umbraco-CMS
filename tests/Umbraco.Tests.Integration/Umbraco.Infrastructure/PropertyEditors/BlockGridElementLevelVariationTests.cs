using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Tests.Common.Builders;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.PropertyEditors;

// NOTE: These tests are in place to ensure that element level variation works for Block Grid. Element level variation
//       is tested more in-depth for Block List (see BlockListElementLevelVariationTests), but since the actual
//       implementation is shared between Block List and Block Grid, we won't repeat all those tests here.
internal class BlockGridElementLevelVariationTests : BlockEditorElementVariationTestBase
{
    private IJsonSerializer JsonSerializer => GetRequiredService<IJsonSerializer>();

    [Test]
    public async Task Can_Publish_Cultures_Independently()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var areaKey = Guid.NewGuid();
        var blockGridDataType = await CreateBlockGridDataType(elementType, areaKey);
        var contentType = CreateContentType(blockGridDataType);
        var blockGridValue = CreateBlockGridValue(elementType, areaKey);
        var content = CreateContent(contentType, blockGridValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues(
            "en-US",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant content value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first content value in English", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The first invariant content value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first content value in English", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The first invariant content value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first content value in English", element3.Value<string>("variantText"));
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant settings value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first settings value in English", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The first invariant settings value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first settings value in English", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The first invariant settings value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first settings value in English", element3.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant content value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first content value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The first invariant content value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first content value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The first invariant content value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first content value in Danish", element3.Value<string>("variantText"));
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The first invariant settings value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first settings value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The first invariant settings value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first settings value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The first invariant settings value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first settings value in Danish", element3.Value<string>("variantText"));
            });

        blockGridValue = JsonSerializer.Deserialize<BlockGridValue>((string)content.Properties["blocks"]!.GetValue()!);
        for (var i = 0; i < 3; i++)
        {
            blockGridValue.ContentData[i].Values[0].Value = $"#{i + 1}: The second invariant content value";
            blockGridValue.ContentData[i].Values[1].Value = $"#{i + 1}: The second content value in English";
            blockGridValue.ContentData[i].Values[2].Value = $"#{i + 1}: The second content value in Danish";
            blockGridValue.SettingsData[i].Values[0].Value = $"#{i + 1}: The second invariant settings value";
            blockGridValue.SettingsData[i].Values[1].Value = $"#{i + 1}: The second settings value in English";
            blockGridValue.SettingsData[i].Values[2].Value = $"#{i + 1}: The second settings value in Danish";
        }

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockGridValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValues(
            "en-US",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The second content value in English", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant content value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The second content value in English", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant content value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The second content value in English", element3.Value<string>("variantText"));
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The second settings value in English", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant settings value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The second settings value in English", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant settings value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The second settings value in English", element3.Value<string>("variantText"));
            });

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first content value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant content value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first content value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant content value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first content value in Danish", element3.Value<string>("variantText"));
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The first settings value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant settings value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The first settings value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant settings value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The first settings value in Danish", element3.Value<string>("variantText"));
            });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues(
            "da-DK",
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant content value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The second content value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant content value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The second content value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant content value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The second content value in Danish", element3.Value<string>("variantText"));
            },
            (element1, element2, element3) =>
            {
                Assert.AreEqual("#1: The second invariant settings value", element1.Value<string>("invariantText"));
                Assert.AreEqual("#1: The second settings value in Danish", element1.Value<string>("variantText"));
                Assert.AreEqual("#2: The second invariant settings value", element2.Value<string>("invariantText"));
                Assert.AreEqual("#2: The second settings value in Danish", element2.Value<string>("variantText"));
                Assert.AreEqual("#3: The second invariant settings value", element3.Value<string>("invariantText"));
                Assert.AreEqual("#3: The second settings value in Danish", element3.Value<string>("variantText"));
            });

        void AssertPropertyValues(
            string culture,
            Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockContentValues,
            Action<IPublishedElement, IPublishedElement, IPublishedElement> validateBlockSettingsValues)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var blocks = publishedContent.Value<BlockGridModel>("blocks");
            Assert.IsNotNull(blocks);
            Assert.AreEqual(2, blocks.Count);
            var area = blocks[0].Areas.FirstOrDefault();
            Assert.IsNotNull(area);
            Assert.AreEqual(1, area.Count);
            Assert.Multiple(() =>
            {
                validateBlockContentValues(blocks[0].Content, area[0].Content, blocks[1].Content);
                validateBlockSettingsValues(blocks[0].Settings, area[0].Settings, blocks[1].Settings);
            });
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Removed_At_Root()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var areaKey = Guid.NewGuid();
        var blockGridDataType = await CreateBlockGridDataType(elementType, areaKey);
        var contentType = CreateContentType(blockGridDataType);
        var blockGridValue = CreateBlockGridValue(elementType, areaKey);
        var content = CreateContent(contentType, blockGridValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 2, blocks => { });
        AssertPropertyValues("da-DK", 2, blocks => { });

        blockGridValue = JsonSerializer.Deserialize<BlockGridValue>((string)content.Properties["blocks"]!.GetValue()!);

        // remove block #3 (second at root level)
        blockGridValue.Layout[blockGridValue.Layout.First().Key] =
        [
            blockGridValue.Layout.First().Value.First(),
        ];
        var contentKey = blockGridValue.ContentData[2].Key;
        blockGridValue.ContentData.RemoveAt(2);
        blockGridValue.SettingsData.RemoveAt(2);
        blockGridValue.Expose.RemoveAll(v => v.ContentKey == contentKey);
        Assert.AreEqual(4, blockGridValue.Expose.Count);

        for (var i = 0; i < 2; i++)
        {
            blockGridValue.ContentData[i].Values[0].Value = $"#{i + 1}: The second invariant content value";
            blockGridValue.ContentData[i].Values[1].Value = $"#{i + 1}: The second content value in English";
            blockGridValue.ContentData[i].Values[2].Value = $"#{i + 1}: The second content value in Danish";
            blockGridValue.SettingsData[i].Values[0].Value = $"#{i + 1}: The second invariant settings value";
            blockGridValue.SettingsData[i].Values[1].Value = $"#{i + 1}: The second settings value in English";
            blockGridValue.SettingsData[i].Values[2].Value = $"#{i + 1}: The second settings value in Danish";
        }

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockGridValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValues("en-US", 1, blocks =>
        {
            var areaItem = blocks[0].Areas.FirstOrDefault()?.FirstOrDefault();
            Assert.IsNotNull(areaItem);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in English", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant content value", areaItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The second content value in English", areaItem.Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second settings value in English", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant settings value", areaItem.Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#2: The second settings value in English", areaItem.Settings.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            var areaItem = blocks[0].Areas.FirstOrDefault()?.FirstOrDefault();
            Assert.IsNotNull(areaItem);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant content value", areaItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The first content value in Danish", areaItem.Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant settings value", areaItem.Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#2: The first settings value in Danish", areaItem.Settings.Value<string>("variantText"));
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues("da-DK", 1, blocks =>
        {
            var areaItem = blocks[0].Areas.FirstOrDefault()?.FirstOrDefault();
            Assert.IsNotNull(areaItem);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant content value", areaItem.Content.Value<string>("invariantText"));
            Assert.AreEqual("#2: The second content value in Danish", areaItem.Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#2: The second invariant settings value", areaItem.Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#2: The second settings value in Danish", areaItem.Settings.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockGridModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockGridModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    [Test]
    public async Task Can_Publish_With_Blocks_Removed_In_Area()
    {
        var elementType = CreateElementType(ContentVariation.Culture);

        var areaKey = Guid.NewGuid();
        var blockGridDataType = await CreateBlockGridDataType(elementType, areaKey);
        var contentType = CreateContentType(blockGridDataType);
        var blockGridValue = CreateBlockGridValue(elementType, areaKey);
        var content = CreateContent(contentType, blockGridValue);

        PublishContent(content, ["en-US", "da-DK"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.IsNotEmpty(blocks[0].Areas);

            // no need to validate the content/settings values here, the same thing is validated in another test
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.IsNotEmpty(blocks[0].Areas);

            // no need to validate the content/settings values here, the same thing is validated in another test
        });

        blockGridValue = JsonSerializer.Deserialize<BlockGridValue>((string)content.Properties["blocks"]!.GetValue()!);

        // remove block #2 (inside the area of the first block at root level)
        ((BlockGridLayoutItem)blockGridValue.Layout[blockGridValue.Layout.First().Key].First()).Areas = [];
        var contentKey = blockGridValue.ContentData[1].Key;
        blockGridValue.ContentData.RemoveAt(1);
        blockGridValue.SettingsData.RemoveAt(1);
        blockGridValue.Expose.RemoveAll(v => v.ContentKey == contentKey);
        Assert.AreEqual(4, blockGridValue.Expose.Count);

        blockGridValue.ContentData[0].Values[0].Value = "#1: The second invariant content value";
        blockGridValue.ContentData[0].Values[1].Value = "#1: The second content value in English";
        blockGridValue.ContentData[0].Values[2].Value = "#1: The second content value in Danish";
        blockGridValue.ContentData[1].Values[0].Value = "#3: The second invariant content value";
        blockGridValue.ContentData[1].Values[1].Value = "#3: The second content value in English";
        blockGridValue.ContentData[1].Values[2].Value = "#3: The second content value in Danish";
        blockGridValue.SettingsData[0].Values[0].Value = "#1: The second invariant settings value";
        blockGridValue.SettingsData[0].Values[1].Value = "#1: The second settings value in English";
        blockGridValue.SettingsData[0].Values[2].Value = "#1: The second settings value in Danish";
        blockGridValue.SettingsData[1].Values[0].Value = "#3: The second invariant settings value";
        blockGridValue.SettingsData[1].Values[1].Value = "#3: The second settings value in English";
        blockGridValue.SettingsData[1].Values[2].Value = "#3: The second settings value in Danish";

        content.Properties["blocks"]!.SetValue(JsonSerializer.Serialize(blockGridValue));
        ContentService.Save(content);
        PublishContent(content, ["en-US"]);

        AssertPropertyValues("en-US", 2, blocks =>
        {
            Assert.IsEmpty(blocks[0].Areas);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in English", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant content value",  blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second content value in English",  blocks[1].Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second settings value in English", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant settings value",  blocks[1].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second settings value in English",  blocks[1].Settings.Value<string>("variantText"));
        });

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.IsEmpty(blocks[0].Areas);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The first content value in Danish", blocks[1].Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The first settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant settings value", blocks[1].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#3: The first settings value in Danish", blocks[1].Settings.Value<string>("variantText"));
        });

        PublishContent(content, ["da-DK"]);

        AssertPropertyValues("da-DK", 2, blocks =>
        {
            Assert.IsEmpty(blocks[0].Areas);

            Assert.AreEqual("#1: The second invariant content value", blocks[0].Content.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second content value in Danish", blocks[0].Content.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant content value", blocks[1].Content.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second content value in Danish", blocks[1].Content.Value<string>("variantText"));

            Assert.AreEqual("#1: The second invariant settings value", blocks[0].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#1: The second settings value in Danish", blocks[0].Settings.Value<string>("variantText"));
            Assert.AreEqual("#3: The second invariant settings value", blocks[1].Settings!.Value<string>("invariantText"));
            Assert.AreEqual("#3: The second settings value in Danish", blocks[1].Settings.Value<string>("variantText"));
        });

        void AssertPropertyValues(string culture, int numberOfExpectedBlocks, Action<BlockGridModel> validateBlocks)
        {
            SetVariationContext(culture, null);
            var publishedContent = GetPublishedContent(content.Key);

            var value = publishedContent.Value<BlockGridModel>("blocks");
            Assert.IsNotNull(value);
            Assert.AreEqual(numberOfExpectedBlocks, value.Count);

            validateBlocks(value);
        }
    }

    private async Task<IDataType> CreateBlockGridDataType(IContentType elementType, Guid areaKey)
        => await CreateBlockEditorDataType(
            Constants.PropertyEditors.Aliases.BlockGrid,
            new BlockGridConfiguration.BlockGridBlockConfiguration[]
            {
                new()
                {
                    ContentElementTypeKey = elementType.Key,
                    SettingsElementTypeKey = elementType.Key,
                    AreaGridColumns = 12,
                    Areas =
                    [
                        new() { Alias = "one", Key = areaKey, ColumnSpan = 12, RowSpan = 1 }
                    ]
                }
            });

    private IContentType CreateContentType(IDataType blockListDataType)
        => CreateContentType(ContentVariation.Culture, blockListDataType);

    private BlockGridValue CreateBlockGridValue(IContentType elementType, Guid areaKey)
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();
        var contentElementKey2 = Guid.NewGuid();
        var settingsElementKey2 = Guid.NewGuid();
        var contentElementKey3 = Guid.NewGuid();
        var settingsElementKey3 = Guid.NewGuid();
        return new BlockGridValue(
        [
            new BlockGridLayoutItem(contentElementKey1, settingsElementKey1)
            {
                ColumnSpan = 12,
                RowSpan = 1,
                Areas =
                [
                    new BlockGridLayoutAreaItem(areaKey)
                    {
                        Items =
                        [
                            new BlockGridLayoutItem(contentElementKey2, settingsElementKey2)
                            {
                                ColumnSpan = 12,
                                RowSpan = 1
                            },
                        ],
                    },
                ],
            },
            new BlockGridLayoutItem(contentElementKey3, settingsElementKey3)
            {
                ColumnSpan = 12,
                RowSpan = 1,
            }
        ])
        {
            ContentData =
            [
                new(contentElementKey1, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new() { Alias = "invariantText", Value = "#1: The first invariant content value" },
                        new() { Alias = "variantText", Value = "#1: The first content value in English", Culture = "en-US" },
                        new() { Alias = "variantText", Value = "#1: The first content value in Danish", Culture = "da-DK" }
                    ]
                },
                new(contentElementKey2, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new() { Alias = "invariantText", Value = "#2: The first invariant content value" },
                        new() { Alias = "variantText", Value = "#2: The first content value in English", Culture = "en-US" },
                        new() { Alias = "variantText", Value = "#2: The first content value in Danish", Culture = "da-DK" }
                    ]
                },
                new(contentElementKey3, elementType.Key, elementType.Alias)
                {
                    Values = [
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
                    Values = [
                        new() { Alias = "invariantText", Value = "#1: The first invariant settings value" },
                        new() { Alias = "variantText", Value = "#1: The first settings value in English", Culture = "en-US" },
                        new() { Alias = "variantText", Value = "#1: The first settings value in Danish", Culture = "da-DK" }
                    ]
                },
                new(settingsElementKey2, elementType.Key, elementType.Alias)
                {
                    Values = [
                        new() { Alias = "invariantText", Value = "#2: The first invariant settings value" },
                        new() { Alias = "variantText", Value = "#2: The first settings value in English", Culture = "en-US" },
                        new() { Alias = "variantText", Value = "#2: The first settings value in Danish", Culture = "da-DK" }
                    ]
                },
                new(settingsElementKey3, elementType.Key, elementType.Alias)
                {
                    Values = [
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
        };
    }

    private IContent CreateContent(IContentType contentType, BlockGridValue blockGridValue)
    {
        var contentBuilder = new ContentBuilder()
            .WithContentType(contentType)
            .WithCultureName("en-US", "Home (en)")
            .WithCultureName("da-DK", "Home (da)");

        var content = contentBuilder.Build();

        var propertyValue = JsonSerializer.Serialize(blockGridValue);
        content.Properties["blocks"]!.SetValue(propertyValue);

        ContentService.Save(content);
        return content;
    }
}
