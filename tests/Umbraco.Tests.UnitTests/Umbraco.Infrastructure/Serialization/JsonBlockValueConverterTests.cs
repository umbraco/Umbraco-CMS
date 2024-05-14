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
        var contentElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentElementUdi3 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi3 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentElementUdi4 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi4 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var blockGridValue = new BlockGridValue(
        [
            new BlockGridLayoutItem(contentElementUdi1, settingsElementUdi1)
            {
                ColumnSpan = 123,
                RowSpan = 456,
                Areas =
                [
                    new BlockGridLayoutAreaItem(Guid.NewGuid())
                    {
                        Items =
                        [
                            new BlockGridLayoutItem(contentElementUdi3, settingsElementUdi3)
                            {
                                ColumnSpan = 12,
                                RowSpan = 34,
                                Areas =
                                [
                                    new BlockGridLayoutAreaItem(Guid.NewGuid())
                                    {
                                        Items =
                                        [
                                            new BlockGridLayoutItem(contentElementUdi4, settingsElementUdi4)
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
            new BlockGridLayoutItem(contentElementUdi2, settingsElementUdi2)
            {
                ColumnSpan = 789,
                RowSpan = 123,
            }
        ])
        {
            ContentData =
            [
                new(contentElementUdi1, elementType1Key, "elementType1"),
                new(contentElementUdi2, elementType2Key, "elementType2"),
                new(contentElementUdi3, elementType3Key, "elementType3"),
                new(contentElementUdi4, elementType4Key, "elementType4"),
            ],
            SettingsData =
            [
                new(settingsElementUdi1, elementType3Key, "elementType3"),
                new(settingsElementUdi2, elementType4Key, "elementType4"),
                new(settingsElementUdi3, elementType1Key, "elementType1"),
                new(settingsElementUdi4, elementType2Key, "elementType2")
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
            Assert.AreEqual(contentElementUdi1, layoutItems[0].ContentUdi);
            Assert.AreEqual(settingsElementUdi1, layoutItems[0].SettingsUdi);

            Assert.AreEqual(789, layoutItems[1].ColumnSpan);
            Assert.AreEqual(123, layoutItems[1].RowSpan);
            Assert.AreEqual(contentElementUdi2, layoutItems[1].ContentUdi);
            Assert.AreEqual(settingsElementUdi2, layoutItems[1].SettingsUdi);
        });

        Assert.AreEqual(1, layoutItems[0].Areas.Length);
        Assert.AreEqual(1, layoutItems[0].Areas[0].Items.Length);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(12, layoutItems[0].Areas[0].Items[0].ColumnSpan);
            Assert.AreEqual(34, layoutItems[0].Areas[0].Items[0].RowSpan);
            Assert.AreEqual(contentElementUdi3, layoutItems[0].Areas[0].Items[0].ContentUdi);
            Assert.AreEqual(settingsElementUdi3, layoutItems[0].Areas[0].Items[0].SettingsUdi);
        });

        Assert.AreEqual(1, layoutItems[0].Areas[0].Items[0].Areas.Length);
        Assert.AreEqual(1, layoutItems[0].Areas[0].Items[0].Areas[0].Items.Length);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(56, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ColumnSpan);
            Assert.AreEqual(78, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].RowSpan);
            Assert.AreEqual(contentElementUdi4, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ContentUdi);
            Assert.AreEqual(settingsElementUdi4, layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].SettingsUdi);
        });

        Assert.AreEqual(4, deserialized.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementUdi1, deserialized.ContentData[0].Udi);
            Assert.AreEqual(elementType1Key, deserialized.ContentData[0].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[0].ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementUdi2, deserialized.ContentData[1].Udi);
            Assert.AreEqual(elementType2Key, deserialized.ContentData[1].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[1].ContentTypeAlias);

            Assert.AreEqual(contentElementUdi3, deserialized.ContentData[2].Udi);
            Assert.AreEqual(elementType3Key, deserialized.ContentData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[2].ContentTypeAlias);

            Assert.AreEqual(contentElementUdi3, deserialized.ContentData[2].Udi);
            Assert.AreEqual(elementType3Key, deserialized.ContentData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData[2].ContentTypeAlias);
        });

        Assert.AreEqual(4, deserialized.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementUdi1, deserialized.SettingsData[0].Udi);
            Assert.AreEqual(elementType3Key, deserialized.SettingsData[0].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[0].ContentTypeAlias);

            Assert.AreEqual(settingsElementUdi2, deserialized.SettingsData[1].Udi);
            Assert.AreEqual(elementType4Key, deserialized.SettingsData[1].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[1].ContentTypeAlias);

            Assert.AreEqual(settingsElementUdi3, deserialized.SettingsData[2].Udi);
            Assert.AreEqual(elementType1Key, deserialized.SettingsData[2].ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData[2].ContentTypeAlias);

            Assert.AreEqual(settingsElementUdi4, deserialized.SettingsData[3].Udi);
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
        var contentElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentElementUdi1, settingsElementUdi1),
            new BlockListLayoutItem(contentElementUdi2, settingsElementUdi2),
        ])
        {
            ContentData =
            [
                new(contentElementUdi1, elementType1Key, "elementType1"),
                new(contentElementUdi2, elementType2Key, "elementType2")
            ],
            SettingsData =
            [
                new(settingsElementUdi1, elementType3Key, "elementType3"),
                new(settingsElementUdi2, elementType4Key, "elementType4")
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
            Assert.AreEqual(contentElementUdi1, layoutItems.First().ContentUdi);
            Assert.AreEqual(settingsElementUdi1, layoutItems.First().SettingsUdi);

            Assert.AreEqual(contentElementUdi2, layoutItems.Last().ContentUdi);
            Assert.AreEqual(settingsElementUdi2, layoutItems.Last().SettingsUdi);
        });

        Assert.AreEqual(2, deserialized.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementUdi1, deserialized.ContentData.First().Udi);
            Assert.AreEqual(elementType1Key, deserialized.ContentData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData.First().ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementUdi2, deserialized.ContentData.Last().Udi);
            Assert.AreEqual(elementType2Key, deserialized.ContentData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.ContentData.Last().ContentTypeAlias);
        });

        Assert.AreEqual(2, deserialized.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementUdi1, deserialized.SettingsData.First().Udi);
            Assert.AreEqual(elementType3Key, deserialized.SettingsData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserialized.SettingsData.First().ContentTypeAlias);

            Assert.AreEqual(settingsElementUdi2, deserialized.SettingsData.Last().Udi);
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
        var contentElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var contentElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi2 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();
        var elementType3Key = Guid.NewGuid();
        var elementType4Key = Guid.NewGuid();

        var richTextBlockValue = new RichTextBlockValue(
        [
            new RichTextBlockLayoutItem(contentElementUdi1, settingsElementUdi1),
            new RichTextBlockLayoutItem(contentElementUdi2, settingsElementUdi2),
        ])
        {
            ContentData =
            [
                new(contentElementUdi1, elementType1Key, "elementType1"),
                new(contentElementUdi2, elementType2Key, "elementType2")
            ],
            SettingsData =
            [
                new(settingsElementUdi1, elementType3Key, "elementType3"),
                new(settingsElementUdi2, elementType4Key, "elementType4")
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
        Assert.IsTrue(deserializedBlocks.Layout.ContainsKey(Constants.PropertyEditors.Aliases.TinyMce));
        var layoutItems = deserializedBlocks.Layout[Constants.PropertyEditors.Aliases.TinyMce].OfType<RichTextBlockLayoutItem>().ToArray();
        Assert.AreEqual(2, layoutItems.Count());
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementUdi1, layoutItems.First().ContentUdi);
            Assert.AreEqual(settingsElementUdi1, layoutItems.First().SettingsUdi);

            Assert.AreEqual(contentElementUdi2, layoutItems.Last().ContentUdi);
            Assert.AreEqual(settingsElementUdi2, layoutItems.Last().SettingsUdi);
        });

        Assert.AreEqual(2, deserializedBlocks.ContentData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(contentElementUdi1, deserializedBlocks.ContentData.First().Udi);
            Assert.AreEqual(elementType1Key, deserializedBlocks.ContentData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.ContentData.First().ContentTypeAlias); // explicitly annotated to be ignored by the serializer

            Assert.AreEqual(contentElementUdi2, deserializedBlocks.ContentData.Last().Udi);
            Assert.AreEqual(elementType2Key, deserializedBlocks.ContentData.Last().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.ContentData.Last().ContentTypeAlias);
        });

        Assert.AreEqual(2, deserializedBlocks.SettingsData.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(settingsElementUdi1, deserializedBlocks.SettingsData.First().Udi);
            Assert.AreEqual(elementType3Key, deserializedBlocks.SettingsData.First().ContentTypeKey);
            Assert.AreEqual(string.Empty, deserializedBlocks.SettingsData.First().ContentTypeAlias);

            Assert.AreEqual(settingsElementUdi2, deserializedBlocks.SettingsData.Last().Udi);
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
        var contentElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());
        var settingsElementUdi1 = Udi.Create(Constants.UdiEntityType.Element, Guid.NewGuid());

        var elementType1Key = Guid.NewGuid();
        var elementType2Key = Guid.NewGuid();

        var blockListValue = new BlockListValue(
        [
            new BlockListLayoutItem(contentElementUdi1, settingsElementUdi1),
        ])
        {
            Layout =
            {
                [Constants.PropertyEditors.Aliases.TinyMce] =
                [
                    new RichTextBlockLayoutItem(contentElementUdi1, settingsElementUdi1)
                ],
                [Constants.PropertyEditors.Aliases.BlockGrid] =
                [
                    new BlockGridLayoutItem(contentElementUdi1, settingsElementUdi1),
                ],
                ["Some.Custom.Block.Editor"] =
                [
                    new BlockListLayoutItem(contentElementUdi1, settingsElementUdi1),
                ]
            },
            ContentData =
            [
                new(contentElementUdi1, elementType1Key, "elementType1"),
            ],
            SettingsData =
            [
                new(settingsElementUdi1, elementType2Key, "elementType2")
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
            Assert.AreEqual(contentElementUdi1, layoutItems.First().ContentUdi);
            Assert.AreEqual(settingsElementUdi1, layoutItems.First().SettingsUdi);
        });
    }
}
