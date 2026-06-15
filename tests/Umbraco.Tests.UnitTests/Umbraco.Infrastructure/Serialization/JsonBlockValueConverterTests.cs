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

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(blockGridValue);
        var deserialized = serializer.Deserialize<BlockGridValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);

        Assert.That(deserialized.Layout, Has.Count.EqualTo(1));
        Assert.That(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockGrid), Is.True);
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockGrid].OfType<BlockGridLayoutItem>().ToArray();
        Assert.That(layoutItems.Count(), Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(layoutItems[0].ColumnSpan, Is.EqualTo(123));
            Assert.That(layoutItems[0].RowSpan, Is.EqualTo(456));
            Assert.That(layoutItems[0].ContentKey, Is.EqualTo(contentElementKey1));
            Assert.That(layoutItems[0].SettingsKey, Is.EqualTo(settingsElementKey1));

            Assert.That(layoutItems[1].ColumnSpan, Is.EqualTo(789));
            Assert.That(layoutItems[1].RowSpan, Is.EqualTo(123));
            Assert.That(layoutItems[1].ContentKey, Is.EqualTo(contentElementKey2));
            Assert.That(layoutItems[1].SettingsKey, Is.EqualTo(settingsElementKey2));
        });

        Assert.That(layoutItems[0].Areas.Length, Is.EqualTo(1));
        Assert.That(layoutItems[0].Areas[0].Items.Length, Is.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(layoutItems[0].Areas[0].Items[0].ColumnSpan, Is.EqualTo(12));
            Assert.That(layoutItems[0].Areas[0].Items[0].RowSpan, Is.EqualTo(34));
            Assert.That(layoutItems[0].Areas[0].Items[0].ContentKey, Is.EqualTo(contentElementKey3));
            Assert.That(layoutItems[0].Areas[0].Items[0].SettingsKey, Is.EqualTo(settingsElementKey3));
        });

        Assert.That(layoutItems[0].Areas[0].Items[0].Areas.Length, Is.EqualTo(1));
        Assert.That(layoutItems[0].Areas[0].Items[0].Areas[0].Items.Length, Is.EqualTo(1));

        Assert.Multiple(() =>
        {
            Assert.That(layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ColumnSpan, Is.EqualTo(56));
            Assert.That(layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].RowSpan, Is.EqualTo(78));
            Assert.That(layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].ContentKey, Is.EqualTo(contentElementKey4));
            Assert.That(layoutItems[0].Areas[0].Items[0].Areas[0].Items[0].SettingsKey, Is.EqualTo(settingsElementKey4));
        });

        Assert.That(deserialized.ContentData, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.ContentData[0].Key, Is.EqualTo(contentElementKey1));
            Assert.That(deserialized.ContentData[0].ContentTypeKey, Is.EqualTo(elementType1Key));
            Assert.That(deserialized.ContentData[0].ContentTypeAlias, Is.EqualTo(string.Empty)); // explicitly annotated to be ignored by the serializer

            Assert.That(deserialized.ContentData[1].Key, Is.EqualTo(contentElementKey2));
            Assert.That(deserialized.ContentData[1].ContentTypeKey, Is.EqualTo(elementType2Key));
            Assert.That(deserialized.ContentData[1].ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.ContentData[2].Key, Is.EqualTo(contentElementKey3));
            Assert.That(deserialized.ContentData[2].ContentTypeKey, Is.EqualTo(elementType3Key));
            Assert.That(deserialized.ContentData[2].ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.ContentData[2].Key, Is.EqualTo(contentElementKey3));
            Assert.That(deserialized.ContentData[2].ContentTypeKey, Is.EqualTo(elementType3Key));
            Assert.That(deserialized.ContentData[2].ContentTypeAlias, Is.EqualTo(string.Empty));
        });

        Assert.That(deserialized.SettingsData, Has.Count.EqualTo(4));
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.SettingsData[0].Key, Is.EqualTo(settingsElementKey1));
            Assert.That(deserialized.SettingsData[0].ContentTypeKey, Is.EqualTo(elementType3Key));
            Assert.That(deserialized.SettingsData[0].ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.SettingsData[1].Key, Is.EqualTo(settingsElementKey2));
            Assert.That(deserialized.SettingsData[1].ContentTypeKey, Is.EqualTo(elementType4Key));
            Assert.That(deserialized.SettingsData[1].ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.SettingsData[2].Key, Is.EqualTo(settingsElementKey3));
            Assert.That(deserialized.SettingsData[2].ContentTypeKey, Is.EqualTo(elementType1Key));
            Assert.That(deserialized.SettingsData[2].ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.SettingsData[3].Key, Is.EqualTo(settingsElementKey4));
            Assert.That(deserialized.SettingsData[3].ContentTypeKey, Is.EqualTo(elementType2Key));
            Assert.That(deserialized.SettingsData[3].ContentTypeAlias, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Can_Serialize_BlockGrid_Without_Blocks()
    {
        var blockGridValue = new BlockGridValue();
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(blockGridValue);
        var deserialized = serializer.Deserialize<BlockGridValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.Layout, Is.Empty);
            Assert.That(deserialized.ContentData, Is.Empty);
            Assert.That(deserialized.SettingsData, Is.Empty);
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

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);

        Assert.That(deserialized.Layout, Has.Count.EqualTo(1));
        Assert.That(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockList), Is.True);
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockList].OfType<BlockListLayoutItem>().ToArray();
        Assert.That(layoutItems.Count(), Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(layoutItems.First().ContentKey, Is.EqualTo(contentElementKey1));
            Assert.That(layoutItems.First().SettingsKey, Is.EqualTo(settingsElementKey1));

            Assert.That(layoutItems.Last().ContentKey, Is.EqualTo(contentElementKey2));
            Assert.That(layoutItems.Last().SettingsKey, Is.EqualTo(settingsElementKey2));
        });

        Assert.That(deserialized.ContentData, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.ContentData.First().Key, Is.EqualTo(contentElementKey1));
            Assert.That(deserialized.ContentData.First().ContentTypeKey, Is.EqualTo(elementType1Key));
            Assert.That(deserialized.ContentData.First().ContentTypeAlias, Is.EqualTo(string.Empty)); // explicitly annotated to be ignored by the serializer

            Assert.That(deserialized.ContentData.Last().Key, Is.EqualTo(contentElementKey2));
            Assert.That(deserialized.ContentData.Last().ContentTypeKey, Is.EqualTo(elementType2Key));
            Assert.That(deserialized.ContentData.Last().ContentTypeAlias, Is.EqualTo(string.Empty));
        });

        Assert.That(deserialized.SettingsData, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.SettingsData.First().Key, Is.EqualTo(settingsElementKey1));
            Assert.That(deserialized.SettingsData.First().ContentTypeKey, Is.EqualTo(elementType3Key));
            Assert.That(deserialized.SettingsData.First().ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserialized.SettingsData.Last().Key, Is.EqualTo(settingsElementKey2));
            Assert.That(deserialized.SettingsData.Last().ContentTypeKey, Is.EqualTo(elementType4Key));
            Assert.That(deserialized.SettingsData.Last().ContentTypeAlias, Is.EqualTo(string.Empty));
        });
    }

    [Test]
    public void Can_Serialize_BlockList_Without_Blocks()
    {
        var blockListValue = new BlockListValue();
        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.Layout, Is.Empty);
            Assert.That(deserialized.ContentData, Is.Empty);
            Assert.That(deserialized.SettingsData, Is.Empty);
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

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(richTextEditorValue);
        var deserialized = serializer.Deserialize<RichTextEditorValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Markup, Is.EqualTo("<p>This is some markup</p>"));

        var deserializedBlocks = deserialized.Blocks;
        Assert.That(deserializedBlocks, Is.Not.Null);
        Assert.That(deserializedBlocks.Layout, Has.Count.EqualTo(1));
        Assert.That(deserializedBlocks.Layout.ContainsKey(Constants.PropertyEditors.Aliases.RichText), Is.True);
        var layoutItems = deserializedBlocks.Layout[Constants.PropertyEditors.Aliases.RichText].OfType<RichTextBlockLayoutItem>().ToArray();
        Assert.That(layoutItems.Count(), Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(layoutItems.First().ContentKey, Is.EqualTo(contentElementKey1));
            Assert.That(layoutItems.First().SettingsKey, Is.EqualTo(settingsElementKey1));

            Assert.That(layoutItems.Last().ContentKey, Is.EqualTo(contentElementKey2));
            Assert.That(layoutItems.Last().SettingsKey, Is.EqualTo(settingsElementKey2));
        });

        Assert.That(deserializedBlocks.ContentData, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(deserializedBlocks.ContentData.First().Key, Is.EqualTo(contentElementKey1));
            Assert.That(deserializedBlocks.ContentData.First().ContentTypeKey, Is.EqualTo(elementType1Key));
            Assert.That(deserializedBlocks.ContentData.First().ContentTypeAlias, Is.EqualTo(string.Empty)); // explicitly annotated to be ignored by the serializer

            Assert.That(deserializedBlocks.ContentData.Last().Key, Is.EqualTo(contentElementKey2));
            Assert.That(deserializedBlocks.ContentData.Last().ContentTypeKey, Is.EqualTo(elementType2Key));
            Assert.That(deserializedBlocks.ContentData.Last().ContentTypeAlias, Is.EqualTo(string.Empty));
        });

        Assert.That(deserializedBlocks.SettingsData, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(deserializedBlocks.SettingsData.First().Key, Is.EqualTo(settingsElementKey1));
            Assert.That(deserializedBlocks.SettingsData.First().ContentTypeKey, Is.EqualTo(elementType3Key));
            Assert.That(deserializedBlocks.SettingsData.First().ContentTypeAlias, Is.EqualTo(string.Empty));

            Assert.That(deserializedBlocks.SettingsData.Last().Key, Is.EqualTo(settingsElementKey2));
            Assert.That(deserializedBlocks.SettingsData.Last().ContentTypeKey, Is.EqualTo(elementType4Key));
            Assert.That(deserializedBlocks.SettingsData.Last().ContentTypeAlias, Is.EqualTo(string.Empty));
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

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(richTextEditorValue);
        var deserialized = serializer.Deserialize<RichTextEditorValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Markup, Is.EqualTo("<p>This is some markup</p>"));
        Assert.That(deserialized.Blocks, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(deserialized.Blocks.Layout, Is.Empty);
            Assert.That(deserialized.Blocks.ContentData, Is.Empty);
            Assert.That(deserialized.Blocks.SettingsData, Is.Empty);
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

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var serialized = serializer.Serialize(blockListValue);
        var deserialized = serializer.Deserialize<BlockListValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);

        Assert.That(deserialized.Layout, Has.Count.EqualTo(1));
        Assert.That(deserialized.Layout.ContainsKey(Constants.PropertyEditors.Aliases.BlockList), Is.True);
        var layoutItems = deserialized.Layout[Constants.PropertyEditors.Aliases.BlockList].OfType<BlockListLayoutItem>().ToArray();
        Assert.That(layoutItems.Count(), Is.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(layoutItems.First().ContentKey, Is.EqualTo(contentElementKey1));
            Assert.That(layoutItems.First().SettingsKey, Is.EqualTo(settingsElementKey1));
        });
    }

    [Test]
    public void Try_Deserialize_Unknown_Block_Layout_With_Nested_Array()
    {
        var json = """
        {
            "layout": {
                "Umbraco.BlockGrid": [{
                        "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D",
                        "rowSpan": 1,
                        "areas": [],
                        "columnSpan": 12
                    }
                ],
                "Umbraco.BlockList": [{
                        "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D"
                    }
                ],
                "Some.Custom.BlockEditor": [{
                        "contentUdi": "umb://element/1304E1DDAC87439684FE8A399231CB3D"
                    }
                ]
            }
        }
""";

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        Assert.DoesNotThrow(() => serializer.Deserialize<BlockListValue>(json));
    }

    /// <summary>
    /// Test case that verifies the fix for https://github.com/umbraco/Umbraco-CMS/issues/20409.
    /// </summary>
    [Test]
    public void Can_Deserialize_BlockGrid_With_Blocks_Using_Values_As_Property_Alias()
    {
        // Create a serialized BlockGridValue in Umbraco 13 format that has a block with a property alias "values".
        var serialized = @"{
   ""layout"":{
      ""Umbraco.BlockList"":[
         {
            ""contentUdi"":""umb://element/6ad18441631140d48515ea0fc5b00425""
         }
      ]
   },
   ""contentData"":[
      {
         ""contentTypeKey"":""a1d1123c-289b-4a05-b33f-9f06cb723da1"",
         ""udi"":""umb://element/6ad18441631140d48515ea0fc5b00425"",
         ""text"":""Text"",
         ""values"":""Values""
      }
   ],
   ""settingsData"":[
   ]
}";

        var serializer = new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory());
        var deserialized = serializer.Deserialize<BlockGridValue>(serialized);

        Assert.That(deserialized, Is.Not.Null);

        Assert.That(deserialized.ContentData, Has.Count.EqualTo(1));
        Assert.That(deserialized.ContentData[0].RawPropertyValues, Has.Count.EqualTo(2));
        Assert.That(deserialized.ContentData[0].RawPropertyValues["text"], Is.EqualTo("Text"));
        Assert.That(deserialized.ContentData[0].RawPropertyValues["values"], Is.EqualTo("Values"));
    }
}
