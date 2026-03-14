// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Contains unit tests for the <see cref="BlockListPropertyValueConverter"/> class, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class BlockListPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<BlockListConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockList;

    private BlockListPropertyValueConverter CreateConverter()
    {
        var publishedModelFactory = new NoopPublishedModelFactory();
        var blockVarianceHandler = new BlockEditorVarianceHandler(Mock.Of<ILanguageService>(), Mock.Of<IContentTypeService>());
        var editor = new BlockListPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(GetPublishedContentTypeCache(), Mock.Of<ICacheManager>(), publishedModelFactory, Mock.Of<IVariationContextAccessor>(), blockVarianceHandler),
            Mock.Of<IContentTypeService>(),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            new BlockListPropertyValueConstructorCache(),
            Mock.Of<IVariationContextAccessor>(),
            blockVarianceHandler);
        return editor;
    }

    private BlockListConfiguration ConfigForMany() => new()
    {
        Blocks = new[]
        {
            new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = ContentKey1,
                SettingsElementTypeKey = SettingKey2,
            },
            new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = ContentKey2,
                SettingsElementTypeKey = SettingKey1,
            },
        },
    };

    private BlockListConfiguration ConfigForSingle() => new()
    {
        Blocks = new[] { new BlockListConfiguration.BlockConfiguration { ContentElementTypeKey = ContentKey1 } },
    };

    private BlockListConfiguration ConfigForSingleBlockMode() => new()
    {
        Blocks = new[] { new BlockListConfiguration.BlockConfiguration { ContentElementTypeKey = ContentKey1 } },
        ValidationLimit = new() { Min = 1, Max = 1 },
        UseSingleBlockMode = true,
    };

    /// <summary>
    /// Tests whether the converter correctly identifies applicable property editors.
    /// </summary>
    [Test]
    public void IsConverter_For()
    {
        var editor = CreateConverter();
        Assert.IsTrue(editor.IsConverter(
            Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList)));
        Assert.IsFalse(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x =>
            x.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent)));
    }

    /// <summary>
    /// Verifies that the <c>GetPropertyValueType</c> method of the block list property value converter
    /// returns <see cref="BlockListModel"/> as the value type when the property type is configured
    /// to support multiple block items.
    /// </summary>
    [Test]
    public void Get_Value_Type_Multiple()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.AreEqual(typeof(BlockListModel), valueType);
    }

    /// <summary>
    /// Tests that the GetPropertyValueType method returns the correct type for a single block list configuration.
    /// </summary>
    [Test]
    public void Get_Value_TypeSingle()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.AreEqual(typeof(BlockListModel), valueType);
    }

    /// <summary>
    /// Tests that the GetPropertyValueType method returns the correct type when in single block mode.
    /// </summary>
    [Test]
    public void Get_Value_TypeSingleBlockMode()
    {
        var editor = CreateConverter();
        var config = ConfigForSingleBlockMode();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        Assert.AreEqual(typeof(BlockListItem), valueType);
    }

    /// <summary>
    /// Tests that the BlockListPropertyValueConverter correctly converts null and empty JSON values
    /// to an empty BlockListModel instance.
    /// </summary>
    [Test]
    public void Convert_Null_Empty()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        string json = null;
        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        json = string.Empty;
        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);
    }

    /// <summary>
    /// Tests that the <see cref="BlockListPropertyValueConverter"/> correctly returns an empty <see cref="BlockListModel"/>
    /// when provided with various valid but empty or invalid JSON inputs, including empty objects, missing data, or mismatched keys.
    /// Ensures robustness against different empty or incomplete JSON scenarios.
    /// </summary>
    [Test]
    public void Convert_Valid_Empty_Json()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = "{}";
        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        json = @"{
""layout"": {},
""data"": []}";
        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Even though there is a layout, there is no data, so the conversion will result in zero elements in total
        json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""e7dba547-615b-4e9a-b4ab-2a7674845bc9""
            }
        ]
    },
    ""contentData"": [],
    ""expose"": [
        {
            ""contentKey"": ""e7dba547-615b-4e9a-b4ab-2a7674845bc9""
        }
    ]
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Even though there is a layout and data, the data is invalid (missing required keys) so the conversion will result in zero elements in total
        json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""e7dba547-615b-4e9a-b4ab-2a7674845bc9""
            }
        ]
    },
    ""contentData"": [
        {
            ""key"": ""e7dba547-615b-4e9a-b4ab-2a7674845bc9""
        }
    ],
    ""expose"": [
        {
            ""contentKey"": ""e7dba547-615b-4e9a-b4ab-2a7674845bc9""
        }
    ]
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Everthing is ok except the udi reference in the layout doesn't match the data so it will be empty
        json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": """ + ContentKey1 + @""",
            ""key"": ""1304E1DD-0000-4396-84FE-8A399231CB3D""
        }
    ],
    ""expose"": [
        {
            ""contentKey"": ""1304E1DD-0000-4396-84FE-8A399231CB3D""
        }
    ]
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);
    }

    /// <summary>
    /// Tests that the BlockListPropertyValueConverter correctly converts a valid JSON string into a BlockListModel.
    /// </summary>
    [Test]
    public void Convert_Valid_Json()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": """ + ContentKey1 + @""",
            ""key"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        }
    ],
    ""expose"": [
        {
            ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        }
    ]
}";
        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(1, converted.Count);
        var item0 = converted[0].Content;
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Key);
        Assert.AreEqual("Test1", item0.ContentType.Alias);
        Assert.IsNull(converted[0].Settings);
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), converted[0].ContentKey);
    }

    /// <summary>
    /// Tests that data can be correctly retrieved and converted from a layout item in a BlockList property.
    /// </summary>
    [Test]
    public void Get_Data_From_Layout_Item()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D"",
                ""settingsKey"": ""1F613E26-CE27-4898-908A-561437AF5100""
            },
            {
                ""contentKey"": ""0A4A416E-547D-464F-ABCC-6F345C17809A"",
                ""settingsKey"": ""63027539-B0DB-45E7-B704-59762D4E83DD""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": """ + ContentKey1 + @""",
            ""key"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        },
        {
            ""contentTypeKey"": """ + ContentKey2 + @""",
            ""key"": ""E05A0347-0442-4AB3-A520-E048E6197E79""
        },
        {
            ""contentTypeKey"": """ + ContentKey2 + @""",
            ""key"": ""0A4A416E-547D-464F-ABCC-6F345C17809A""
        }
    ],
    ""settingsData"": [
        {
            ""contentTypeKey"": """ + SettingKey1 + @""",
            ""key"": ""63027539-B0DB-45E7-B704-59762D4E83DD""
        },
        {
            ""contentTypeKey"": """ + SettingKey2 + @""",
            ""key"": ""1F613E26-CE27-4898-908A-561437AF5100""
        },
        {
            ""contentTypeKey"": """ + SettingKey2 + @""",
            ""key"": ""BCF4BA3D-A40C-496C-93EC-58FAC85F18B9""
        }
    ],
    ""expose"": [
        {
            ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        },
        {
            ""contentKey"": ""E05A0347-0442-4AB3-A520-E048E6197E79""
        },
        {
            ""contentKey"": ""0A4A416E-547D-464F-ABCC-6F345C17809A""
        }
    ]}";

        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(2, converted.Count);

        var item0 = converted[0];
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Content.Key);
        Assert.AreEqual("Test1", item0.Content.ContentType.Alias);
        Assert.AreEqual(Guid.Parse("1F613E26-CE27-4898-908A-561437AF5100"), item0.Settings!.Key);
        Assert.AreEqual("Setting2", item0.Settings.ContentType.Alias);

        var item1 = converted[1];
        Assert.AreEqual(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A"), item1.Content.Key);
        Assert.AreEqual("Test2", item1.Content.ContentType.Alias);
        Assert.AreEqual(Guid.Parse("63027539-B0DB-45E7-B704-59762D4E83DD"), item1.Settings!.Key);
        Assert.AreEqual("Setting1", item1.Settings.ContentType.Alias);
    }

    /// <summary>
    /// Tests that a data item is removed if it is removed from the configuration.
    /// </summary>
    [Test]
    public void Data_Item_Removed_If_Removed_FromConfig()
    {
        var editor = CreateConverter();

        // The data below expects that ContentKey1 + ContentKey2 + SettingsKey1 + SettingsKey2 exist but only ContentKey2 exists so
        // the data should all be filtered.
        var config = new BlockListConfiguration
        {
            Blocks = new[]
            {
                new BlockListConfiguration.BlockConfiguration
                {
                    ContentElementTypeKey = ContentKey2,
                    SettingsElementTypeKey = null,
                },
            },
        };

        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D"",
                ""settingsKey"": ""1F613E26-CE27-4898-908A-561437AF5100""
            },
            {
                ""contentKey"": ""0A4A416E-547D-464F-ABCC-6F345C17809A"",
                ""settingsKey"": ""63027539-B0DB-45E7-B704-59762D4E83DD""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": """ + ContentKey1 + @""",
            ""key"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        },
        {
            ""contentTypeKey"": """ + ContentKey2 + @""",
            ""key"": ""E05A0347-0442-4AB3-A520-E048E6197E79""
        },
        {
            ""contentTypeKey"": """ + ContentKey2 + @""",
            ""key"": ""0A4A416E-547D-464F-ABCC-6F345C17809A""
        }
    ],
    ""settingsData"": [
        {
            ""contentTypeKey"": """ + SettingKey1 + @""",
            ""key"": ""63027539-B0DB-45E7-B704-59762D4E83DD""
        },
        {
            ""contentTypeKey"": """ + SettingKey2 + @""",
            ""key"": ""1F613E26-CE27-4898-908A-561437AF5100""
        },
        {
            ""contentTypeKey"": """ + SettingKey2 + @""",
            ""key"": ""BCF4BA3D-A40C-496C-93EC-58FAC85F18B9""
        }
    ],
    ""expose"": [
        {
            ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        },
        {
            ""contentKey"": ""E05A0347-0442-4AB3-A520-E048E6197E79""
        },
        {
            ""contentKey"": ""0A4A416E-547D-464F-ABCC-6F345C17809A""
        }
    ]
}";

        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(1, converted.Count);

        var item0 = converted[0];
        Assert.AreEqual(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A"), item0.Content.Key);
        Assert.AreEqual("Test2", item0.Content.ContentType.Alias);
        Assert.IsNull(item0.Settings);
    }
}
