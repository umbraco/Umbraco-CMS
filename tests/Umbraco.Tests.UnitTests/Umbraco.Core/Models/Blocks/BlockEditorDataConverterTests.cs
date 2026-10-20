using NUnit.Framework;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models.Blocks;

[TestFixture]
public class BlockEditorDataConverterTests
{
    private static readonly Guid _contentKey = new("1304e1dd-ac87-4396-84fe-8a399231cb3d");
    private static readonly Guid _settingsKey = new("90859e4c-0000-4a05-b33f-9f06cb723da1");

    // A legacy (v13) Block List value with one block referencing both a content and a settings element.
    private const string LegacyJsonWithContentAndSettings = """
    {
        "layout": {
            "Umbraco.BlockList": [
                {
                    "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                    "settingsUdi": "umb://element/90859e4c00004a05b33f9f06cb723da1"
                }
            ]
        },
        "contentData": [
            {
                "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                "text": "Hello world"
            }
        ],
        "settingsData": [
            {
                "contentTypeKey": "b2e2234d-390c-4b16-c44f-af17dc834eb2",
                "udi": "umb://element/90859e4c00004a05b33f9f06cb723da1",
                "cssClasses": "text-xl"
            }
        ]
    }
    """;

    private static BlockListEditorDataConverter CreateConverter()
        => new(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

    private static BlockGridEditorDataConverter CreateGridConverter()
        => new(new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()));

    /// <summary>
    /// Regression test for https://github.com/umbraco/Umbraco-CMS/issues/23392.
    /// Legacy (v13) settings values must be migrated into the new <see cref="BlockItemData.Values"/> array
    /// even when the block's content element has no property values to migrate.
    /// </summary>
    [Test]
    public void Can_Migrate_Settings_Values_When_Content_Has_No_Property_Values()
    {
        // Legacy Block List data where the content element has no property values (e.g. a structural
        // element type), but the settings element carries flat cssClasses/customCSS values.
        const string json = """
        {
            "layout": {
                "Umbraco.BlockList": [
                    {
                        "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                        "settingsUdi": "umb://element/90859e4c00004a05b33f9f06cb723da1"
                    }
                ]
            },
            "contentData": [
                {
                    "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                    "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d"
                }
            ],
            "settingsData": [
                {
                    "contentTypeKey": "b2e2234d-390c-4b16-c44f-af17dc834eb2",
                    "udi": "umb://element/90859e4c00004a05b33f9f06cb723da1",
                    "cssClasses": "text-xl",
                    "customCSS": null
                }
            ]
        }
        """;

        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(json);

        Assert.AreEqual(1, result.BlockValue.SettingsData.Count);

        BlockItemData settings = result.BlockValue.SettingsData[0];

        // The flat legacy values must have been relocated into the Values array...
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, settings.Values.Count);

            BlockPropertyValue? cssClasses = settings.Values.SingleOrDefault(v => v.Alias == "cssClasses");
            Assert.IsNotNull(cssClasses);
            Assert.AreEqual("text-xl", cssClasses!.Value?.ToString());

            BlockPropertyValue? customCss = settings.Values.SingleOrDefault(v => v.Alias == "customCSS");
            Assert.IsNotNull(customCss);
            Assert.IsNull(customCss!.Value);

            // ...and cleared from the raw property values so they are not written back to the DB.
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsEmpty(settings.RawPropertyValues);
#pragma warning restore CS0618 // Type or member is obsolete

            // ...and the content block must still be exposed - AmendExpose is the other half of the migration.
            Assert.AreEqual(1, result.BlockValue.Expose.Count);
            Assert.AreEqual(_contentKey, result.BlockValue.Expose[0].ContentKey);
        });
    }

    /// <summary>
    /// The reported scenario in https://github.com/umbraco/Umbraco-CMS/issues/23392 is Block Grid.
    /// The conversion logic is shared in the base <see cref="BlockEditorDataConverter{TValue, TLayout}"/>,
    /// but this covers the Block Grid layout shape (nested areas) end-to-end.
    /// </summary>
    [Test]
    public void Can_Migrate_Block_Grid_Settings_Values_When_Content_Has_No_Property_Values()
    {
        const string json = """
        {
            "layout": {
                "Umbraco.BlockGrid": [
                    {
                        "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                        "settingsUdi": "umb://element/90859e4c00004a05b33f9f06cb723da1",
                        "areas": [],
                        "columnSpan": 12,
                        "rowSpan": 1
                    }
                ]
            },
            "contentData": [
                {
                    "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                    "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d"
                }
            ],
            "settingsData": [
                {
                    "contentTypeKey": "b2e2234d-390c-4b16-c44f-af17dc834eb2",
                    "udi": "umb://element/90859e4c00004a05b33f9f06cb723da1",
                    "cssClasses": "text-xl",
                    "customCSS": null
                }
            ]
        }
        """;

        BlockGridEditorDataConverter converter = CreateGridConverter();
        BlockEditorData<BlockGridValue, BlockGridLayoutItem> result = converter.Deserialize(json);

        Assert.AreEqual(1, result.BlockValue.SettingsData.Count);

        BlockItemData settings = result.BlockValue.SettingsData[0];
        Assert.Multiple(() =>
        {
            Assert.AreEqual(2, settings.Values.Count);

            BlockPropertyValue? cssClasses = settings.Values.SingleOrDefault(v => v.Alias == "cssClasses");
            Assert.IsNotNull(cssClasses);
            Assert.AreEqual("text-xl", cssClasses!.Value?.ToString());

            BlockPropertyValue? customCss = settings.Values.SingleOrDefault(v => v.Alias == "customCSS");
            Assert.IsNotNull(customCss);
            Assert.IsNull(customCss!.Value);

#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsEmpty(settings.RawPropertyValues);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.AreEqual(1, result.BlockValue.Expose.Count);
            Assert.AreEqual(_contentKey, result.BlockValue.Expose[0].ContentKey);
        });
    }

    [Test]
    public void Can_Migrate_Content_Values_When_Only_Content_Has_Property_Values()
    {
        const string json = """
        {
            "layout": {
                "Umbraco.BlockList": [
                    {
                        "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d"
                    }
                ]
            },
            "contentData": [
                {
                    "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                    "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d",
                    "text": "Hello world"
                }
            ],
            "settingsData": []
        }
        """;

        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(json);

        Assert.AreEqual(1, result.BlockValue.ContentData.Count);

        BlockItemData content = result.BlockValue.ContentData[0];
        Assert.Multiple(() =>
        {
            BlockPropertyValue? text = content.Values.SingleOrDefault(v => v.Alias == "text");
            Assert.IsNotNull(text);
            Assert.AreEqual("Hello world", text!.Value?.ToString());
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsEmpty(content.RawPropertyValues);
#pragma warning restore CS0618 // Type or member is obsolete
        });
    }

    [Test]
    public void Can_Populate_References_From_Layout()
    {
        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(LegacyJsonWithContentAndSettings);

        Assert.AreEqual(1, result.References.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, result.References[0].ContentKey);
            Assert.AreEqual(_settingsKey, result.References[0].SettingsKey);
        });
    }

    [Test]
    public void Can_Resolve_Keys_From_Legacy_Udi()
    {
        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(LegacyJsonWithContentAndSettings);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, result.BlockValue.ContentData[0].Key);
            Assert.AreEqual(_settingsKey, result.BlockValue.SettingsData[0].Key);
        });
    }

    [Test]
    public void Can_Amend_Expose_When_Migrating_Legacy_Data()
    {
        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(LegacyJsonWithContentAndSettings);

        // migrating legacy (pre-block-level-variance) data should expose every content block invariantly
        Assert.AreEqual(1, result.BlockValue.Expose.Count);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(_contentKey, result.BlockValue.Expose[0].ContentKey);
            Assert.IsNull(result.BlockValue.Expose[0].Culture);
            Assert.IsNull(result.BlockValue.Expose[0].Segment);
        });
    }

    [Test]
    public void Cannot_Reconvert_New_Format_Data()
    {
        // Data already in the new format: values are populated and expose is explicitly set (here with a
        // culture, which AmendExpose would overwrite to null if it wrongly ran on already-migrated data).
        var json = $$"""
        {
            "layout": {
                "Umbraco.BlockList": [
                    {
                        "contentKey": "{{_contentKey}}",
                        "settingsKey": "{{_settingsKey}}"
                    }
                ]
            },
            "contentData": [
                {
                    "key": "{{_contentKey}}",
                    "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                    "values": [ { "alias": "text", "value": "Hello world" } ]
                }
            ],
            "settingsData": [
                {
                    "key": "{{_settingsKey}}",
                    "contentTypeKey": "b2e2234d-390c-4b16-c44f-af17dc834eb2",
                    "values": [ { "alias": "cssClasses", "value": "text-xl" } ]
                }
            ],
            "expose": [ { "contentKey": "{{_contentKey}}", "culture": "en-US", "segment": null } ]
        }
        """;

        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(json);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(1, result.BlockValue.ContentData[0].Values.Count);
            Assert.AreEqual(1, result.BlockValue.SettingsData[0].Values.Count);

            // expose must be left exactly as deserialized - not regenerated by AmendExpose
            Assert.AreEqual(1, result.BlockValue.Expose.Count);
            Assert.AreEqual("en-US", result.BlockValue.Expose[0].Culture);
        });
    }

    [Test]
    public void Cannot_Build_References_When_Layout_Is_Missing()
    {
        // Layout only contains a foreign block editor alias, so there's nothing for the Block List converter.
        const string json = """
        {
            "layout": {
                "Umbraco.BlockGrid": [
                    { "contentUdi": "umb://element/1304e1ddac87439684fe8a399231cb3d" }
                ]
            },
            "contentData": [
                {
                    "contentTypeKey": "a1d1123c-289b-4a05-b33f-9f06cb723da1",
                    "udi": "umb://element/1304e1ddac87439684fe8a399231cb3d"
                }
            ],
            "settingsData": []
        }
        """;

        BlockListEditorDataConverter converter = CreateConverter();
        BlockEditorData<BlockListValue, BlockListLayoutItem> result = converter.Deserialize(json);

        Assert.Multiple(() =>
        {
            Assert.IsEmpty(result.References);
            Assert.IsEmpty(result.BlockValue.ContentData);
        });
    }

    [Test]
    public void Can_TryDeserialize_Valid_Json()
    {
        BlockListEditorDataConverter converter = CreateConverter();

        var success = converter.TryDeserialize(LegacyJsonWithContentAndSettings, out BlockEditorData<BlockListValue, BlockListLayoutItem>? result);

        Assert.IsTrue(success);
        Assert.IsNotNull(result);
    }

    [Test]
    public void Cannot_TryDeserialize_Invalid_Json()
    {
        BlockListEditorDataConverter converter = CreateConverter();

        var success = converter.TryDeserialize("this is not json", out BlockEditorData<BlockListValue, BlockListLayoutItem>? result);

        Assert.IsFalse(success);
        Assert.IsNull(result);
    }
}
