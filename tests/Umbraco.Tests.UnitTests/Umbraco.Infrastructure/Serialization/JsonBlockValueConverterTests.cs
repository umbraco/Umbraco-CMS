using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class JsonBlockValueConverterTests
{
    [Test]
    public void Can_Serialize_BlockGrid_With_Blocks()
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();
        var contentElementKey2 = Guid.NewGuid();
        var settingsElementKey2 = Guid.NewGuid();
        var contentElementKey3 = Guid.NewGuid();
        var settingsElementKey3 = Guid.NewGuid();
        var contentElementKey4 = Guid.NewGuid();
        var settingsElementKey4 = Guid.NewGuid();

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var blockGridValue = new BlockGridValue(
        [
            new BlockGridLayoutItem(contentElementKey1, settingsElementKey1)
            {
                ColumnSpan = 123,
                RowSpan = 456,
                Areas =
                [
                    new BlockGridLayoutAreaItem(Guid.NewGuid())
                    {
                        Items =
                        [
                            new BlockGridLayoutItem(contentElementKey3, settingsElementKey3)
                            {
                                ColumnSpan = 12,
                                RowSpan = 34,
                                Areas =
                                [
                                    new BlockGridLayoutAreaItem(Guid.NewGuid())
                                    {
                                        Items =
                                        [
                                            new BlockGridLayoutItem(contentElementKey4, settingsElementKey4)
                                            {
                                                ColumnSpan = 56,
                                                RowSpan = 78,
                                            },
                                        ],
                                    },
                                ],
                            },
                        ],
                    },
                ],
            },
            new BlockGridLayoutItem(contentElementKey2, settingsElementKey2)
            {
                ColumnSpan = 789,
                RowSpan = 123,
            }
        ])
        {
            ContentData =
            [
                new(contentElementKey1, elementType1Key, "elementType1"),
                new(contentElementKey2, elementType2Key, "elementType2"),
                new(contentElementKey3, elementType3Key, "elementType3"),
                new(contentElementKey4, elementType4Key, "elementType4"),
            ],
            SettingsData =
            [
                new(settingsElementKey1, elementType3Key, "elementType3"),
                new(settingsElementKey2, elementType4Key, "elementType4"),
                new(settingsElementKey3, elementType1Key, "elementType1"),
                new(settingsElementKey4, elementType2Key, "elementType2")
            ]
        };

        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(blockGridValue);
        var deserialized = serializer.Deserialize<BlockGridValue>(serialized);

        Assert.IsNotNull(deserialized);

        Assert.AreEqual(1, deserialized.Layout.Count);
        Assert.IsTrue(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockGrid));
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockGrid].OfType<BlockGridLayoutItem>().ToArray();
        Assert.AreEqual(2, layoutItems.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(123, layoutItems[0].ColumnSpan);
            Assert.AreEqual(456, layoutItems[0].RowSpan);
            Assert.AreEqual(contentElementKey1, layoutItems[0].ContentKey);
            Assert.AreEqual(settingsElementKey1, layoutItems[0].SettingsKey);

            Assert.AreEqual(789, layoutItems[1].ColumnSpan);
            Assert.AreEqual(123, layoutItems[1].RowSpan);
            Assert.AreEqual(contentElementKey2, layoutItems[1].ContentKey);
            Assert.AreEqual(settingsElementKey2, layoutItems[1].SettingsKey);
        });

        Assert.AreEqual(1, layoutItems[0].Areas.Length);
        Assert.AreEqual(1, layoutItems[0].Areas[0].Items.Length);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(12, layoutItems[0].Areas[0].Items[0].ColumnSpan);
            Assert.AreEqual(34, layoutItems[0].Areas[0].Items[0].RowSpan);
            Assert.AreEqual(contentElementKey3, layoutItems[0].Areas[0].Items[0].ContentKey);
            Assert.AreEqual(settingsElementKey3, layoutItems[0].Areas[0].Items[0].SettingsKey);
        });

        Assert.AreEqual(1, layoutItems[0].Areas[0].Items[0].Areas.Length);
        Assert.AreEqual(1, layoutItems[0].Areas[0].Items[0].Areas[0].Items.Length);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(56, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ColumnSpan);
            Assert.AreEqual(78, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].RowSpan);
            Assert.AreEqual(contentElementKey4, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ContentKey);
            Assert.AreEqual(settingsElementKey4, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].SettingsKey);
        });

        Assert.AreEqual(4, deserialized.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, deserialized.ContentData[0].Key);
            Assert.AreEqual(elementType1Key, deserialized.ContentData[0].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[0].ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementKey2, deserialized.ContentData[1].Key);
            Assert.AreEqual(elementType2Key, deserialized.ContentData[1].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[1].ContentTypeAlias);

            Assert.AreEqual(contentElementKey3, deserialized.ContentData[2].Key);
            Assert.AreEqual(elementType3Key, deserialized.ContentData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[2].ContentTypeAlias);

            Assert.AreEqual(contentElementKey3, deserialized.ContentData[2].Key);
            Assert.AreEqual(elementType3Key, deserialized.ContentData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[2].ContentTypeAlias);
        });

        Assert.AreEqual(4, deserialized.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementKey1, deserialized.SettingsData[0].Key);
            Assert.AreEqual(elementType3Key, deserialized.SettingsData[0].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[0].ContentTypeAlias);

            Assert.AreEqual(settingsElementKey2, deserialized.SettingsData[1].Key);
            Assert.AreEqual(elementType4Key, deserialized.SettingsData[1].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[1].ContentTypeAlias);

            Assert.AreEqual(settingsElementKey3, deserialized.SettingsData[2].Key);
            Assert.AreEqual(elementType1Key, deserialized.SettingsData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[2].ContentTypeAlias);

            Assert.AreEqual(settingsElementKey4, deserialized.SettingsData[3].Key);
            Assert.AreEqual(elementType2Key, deserialized.SettingsData[3].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[3].ContentTypeAlias);
        });
    }

    [Test]
    public void Can_Serialize_BlockGrid_Without_Blocks()
    {
        var blockGridValue = new BlockGridValue();
        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(blockGridValue);
        var deserialized = serializer.Deserialize<BlockGridValue>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.Multiple(() =>
        {
            Assert.IsEmpty(deserialized.Layout);
            Assert.IsEmpty(deserialized.ContentData);
            Assert.IsEmpty(deserialized.SettingsData);
        });
    }

    [Test]
    public void Can_Serialize_BlockList_With_Blocks()
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();
        var contentElementKey2 = Guid.NewGuid();
        var settingsElementKey2 = Guid.NewGuid();

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentElementKey1, settingsElementKey1),
            new BlockListLayoutItem(contentElementKey2, settingsElementKey2),
        ])
        {
            ContentData =
            [
                new(contentElementKey1, elementType1Key, "elementType1"),
                new(contentElementKey2, elementType2Key, "elementType2")
            ],
            SettingsData =
            [
                new(settingsElementKey1, elementType3Key, "elementType3"),
                new(settingsElementKey2, elementType4Key, "elementType4")
            ]
        };

        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.IsNotNull(deserialized);

        Assert.AreEqual(1, deserialized.Layout.Count);
        Assert.IsTrue(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockList));
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockList].OfType<BlockListLayoutItem>().ToArray();
        Assert.AreEqual(2, layoutItems.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, layoutItems.First().ContentKey);
            Assert.AreEqual(settingsElementKey1, layoutItems.First().SettingsKey);

            Assert.AreEqual(contentElementKey2, layoutItems.Last().ContentKey);
            Assert.AreEqual(settingsElementKey2, layoutItems.Last().SettingsKey);
        });

        Assert.AreEqual(2, deserialized.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, deserialized.ContentData.First().Key);
            Assert.AreEqual(elementType1Key, deserialized.ContentData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData.First().ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementKey2, deserialized.ContentData.Last().Key);
            Assert.AreEqual(elementType2Key, deserialized.ContentData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData.Last().ContentTypeAlias);
        });

        Assert.AreEqual(2, deserialized.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementKey1, deserialized.SettingsData.First().Key);
            Assert.AreEqual(elementType3Key, deserialized.SettingsData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData.First().ContentTypeAlias);

            Assert.AreEqual(settingsElementKey2, deserialized.SettingsData.Last().Key);
            Assert.AreEqual(elementType4Key, deserialized.SettingsData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData.Last().ContentTypeAlias);
        });
    }

    [Test]
    public void Can_Serialize_BlockList_Without_Blocks()
    {
        var blockListValue = new BlockListValue();
        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.Multiple(() =>
        {
            Assert.IsEmpty(deserialized.Layout);
            Assert.IsEmpty(deserialized.ContentData);
            Assert.IsEmpty(deserialized.SettingsData);
        });
    }

    [Test]
    public void Can_Serialize_Richtext_With_Blocks()
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();
        var contentElementKey2 = Guid.NewGuid();
        var settingsElementKey2 = Guid.NewGuid();

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var richTextBlockValue = new RichTextBlockValue(
        [
            new RichTextBlockLayoutItem(contentElementKey1, settingsElementKey1),
            new RichTextBlockLayoutItem(contentElementKey2, settingsElementKey2),
        ])
        {
            ContentData =
            [
                new(contentElementKey1, elementType1Key, "elementType1"),
                new(contentElementKey2, elementType2Key, "elementType2")
            ],
            SettingsData =
            [
                new(settingsElementKey1, elementType3Key, "elementType3"),
                new(settingsElementKey2, elementType4Key, "elementType4")
            ]
        };

        var richTextEditorValue = new RichTextEditorValue
        {
            Blocks = richTextBlockValue,
            Markup = "<p>This is some markup</p>"
        };

        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(richTextEditorValue);
        var deserialized = serializer.Deserialize<RichTextEditorValue>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("<p>This is some markup</p>", deserialized.Markup);

        var deserializedBlocks = deserialized.Blocks;
        Assert.IsNotNull(deserializedBlocks);
        Assert.AreEqual(1, deserializedBlocks.Layout.Count);
        Assert.IsTrue(deserializedBlocks.Layout.ContainsKey(Constants.PropertyEditors.Aliases.RichText));
        var layoutItems = deserializedBlocks.Layout[Constants.PropertyEditors.Aliases.RichText].OfType<RichTextBlockLayoutItem>().ToArray();
        Assert.AreEqual(2, layoutItems.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, layoutItems.First().ContentKey);
            Assert.AreEqual(settingsElementKey1, layoutItems.First().SettingsKey);

            Assert.AreEqual(contentElementKey2, layoutItems.Last().ContentKey);
            Assert.AreEqual(settingsElementKey2, layoutItems.Last().SettingsKey);
        });

        Assert.AreEqual(2, deserializedBlocks.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, deserializedBlocks.ContentData.First().Key);
            Assert.AreEqual(elementType1Key, deserializedBlocks.ContentData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.ContentData.First().ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementKey2, deserializedBlocks.ContentData.Last().Key);
            Assert.AreEqual(elementType2Key, deserializedBlocks.ContentData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.ContentData.Last().ContentTypeAlias);
        });

        Assert.AreEqual(2, deserializedBlocks.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementKey1, deserializedBlocks.SettingsData.First().Key);
            Assert.AreEqual(elementType3Key, deserializedBlocks.SettingsData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.SettingsData.First().ContentTypeAlias);

            Assert.AreEqual(settingsElementKey2, deserializedBlocks.SettingsData.Last().Key);
            Assert.AreEqual(elementType4Key, deserializedBlocks.SettingsData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.SettingsData.Last().ContentTypeAlias);
        });
    }

    [Test]
    public void Can_Serialize_Richtext_Without_Blocks()
    {
        var richTextEditorValue = new RichTextEditorValue
        {
            Blocks = new RichTextBlockValue(),
            Markup = "<p>This is some markup</p>"
        };

        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(richTextEditorValue);
        var deserialized = serializer.Deserialize<RichTextEditorValue>(serialized);

        Assert.IsNotNull(deserialized);
        Assert.AreEqual("<p>This is some markup</p>", deserialized.Markup);
        Assert.IsNotNull(deserialized.Blocks);
        Assert.Multiple(() =>
        {
            Assert.IsEmpty(deserialized.Blocks.Layout);
            Assert.IsEmpty(deserialized.Blocks.ContentData);
            Assert.IsEmpty(deserialized.Blocks.SettingsData);
        });
    }

    [Test]
    public void Ignores_Other_Layouts()
    {
        var contentElementKey1 = Guid.NewGuid();
        var settingsElementKey1 = Guid.NewGuid();

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();

        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentElementKey1, settingsElementKey1),
        ])
        {
            Layout =
            {
                [Constants.PropertyEditors.Aliases.RichText] =
                [
                    new RichTextBlockLayoutItem(contentElementKey1, settingsElementKey1)
                ],
                [Constants.PropertyEditors.Aliases.BlockGrid] =
                [
                    new BlockGridLayoutItem(contentElementKey1, settingsElementKey1),
                ],
                ["Some.Custom.Block.Editor"] =
                [
                    new BlockListLayoutItem(contentElementKey1, settingsElementKey1),
                ]
            },
            ContentData =
            [
                new(contentElementKey1, elementType1Key, "elementType1"),
            ],
            SettingsData =
            [
                new(settingsElementKey1, elementType2Key, "elementType2")
            ]
        };

        var serializer = new SystemTextJsonSerializer();
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.IsNotNull(deserialized);

        Assert.AreEqual(1, deserialized.Layout.Count);
        Assert.IsTrue(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockList));
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockList].OfType<BlockListLayoutItem>().ToArray();
        Assert.AreEqual(1, layoutItems.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementKey1, layoutItems.First().ContentKey);
            Assert.AreEqual(settingsElementKey1, layoutItems.First().SettingsKey);
        });
    }
}
