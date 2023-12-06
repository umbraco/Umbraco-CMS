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
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class BlockListPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<BlockListConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockList;

    private BlockListPropertyValueConverter CreateConverter()
    {
        var publishedSnapshotAccessor = GetPublishedSnapshotAccessor();
        var publishedModelFactory = new NoopPublishedModelFactory();
        var editor = new BlockListPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(publishedSnapshotAccessor, publishedModelFactory),
            Mock.Of<IContentTypeService>(),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new BlockListPropertyValueConstructorCache());
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

    [Test]
    public void IsConverter_For()
    {
        var editor = CreateConverter();
        Assert.IsTrue(editor.IsConverter(
            Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList)));
        Assert.IsFalse(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x =>
            x.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent)));
    }

    [Test]
    public void Get_Value_Type_Multiple()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();

        var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.AreEqual(typeof(BlockListModel), valueType);
    }

    [Test]
    public void Get_Value_TypeSingle()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();

        var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.AreEqual(typeof(BlockListModel), valueType);
    }

    [Test]
    public void Get_Value_TypeSingleBlockMode()
    {
        var editor = CreateConverter();
        var config = ConfigForSingleBlockMode();

        var dataType = new PublishedDataType(1, "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        Assert.AreEqual(typeof(BlockListItem), valueType);
    }

    [Test]
    public void Convert_Null_Empty()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = Mock.Of<IPublishedElement>();

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

    [Test]
    public void Convert_Valid_Empty_Json()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = Mock.Of<IPublishedElement>();

        var json = "{}";
        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        json = @"{
layout: {},
data: []}";
        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Even though there is a layout, there is no data, so the conversion will result in zero elements in total
        json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
            }
        ]
    },
    contentData: []
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Even though there is a layout and data, the data is invalid (missing required keys) so the conversion will result in zero elements in total
        json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
            }
        ]
    },
        contentData: [
        {
            'udi': 'umb://element/e7dba547615b4e9ab4ab2a7674845bc9'
        }
    ]
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);

        // Everthing is ok except the udi reference in the layout doesn't match the data so it will be empty
        json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
            }
        ]
    },
        contentData: [
        {
            'contentTypeKey': '" + ContentKey1 + @"',
            'key': '1304E1DD-0000-4396-84FE-8A399231CB3D'
        }
    ]
}";

        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(0, converted.Count);
    }

    [Test]
    public void Convert_Valid_Json()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = Mock.Of<IPublishedElement>();

        var json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
            }
        ]
    },
        contentData: [
        {
            'contentTypeKey': '" + ContentKey1 + @"',
            'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
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
        Assert.AreEqual(UdiParser.Parse("umb://element/1304E1DDAC87439684FE8A399231CB3D"), converted[0].ContentUdi);
    }

    [Test]
    public void Get_Data_From_Layout_Item()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();
        var propertyType = GetPropertyType(config);
        var publishedElement = Mock.Of<IPublishedElement>();

        var json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D',
                'settingsUdi': 'umb://element/1F613E26CE274898908A561437AF5100'
            },
            {
                'contentUdi': 'umb://element/0A4A416E547D464FABCC6F345C17809A',
                'settingsUdi': 'umb://element/63027539B0DB45E7B70459762D4E83DD'
            }
        ]
    },
    contentData: [
        {
            'contentTypeKey': '" + ContentKey1 + @"',
            'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
        },
        {
            'contentTypeKey': '" + ContentKey2 + @"',
            'udi': 'umb://element/E05A034704424AB3A520E048E6197E79'
        },
        {
            'contentTypeKey': '" + ContentKey2 + @"',
            'udi': 'umb://element/0A4A416E547D464FABCC6F345C17809A'
        }
    ],
    settingsData: [
        {
            'contentTypeKey': '" + SettingKey1 + @"',
            'udi': 'umb://element/63027539B0DB45E7B70459762D4E83DD'
        },
        {
            'contentTypeKey': '" + SettingKey2 + @"',
            'udi': 'umb://element/1F613E26CE274898908A561437AF5100'
        },
        {
            'contentTypeKey': '" + SettingKey2 + @"',
            'udi': 'umb://element/BCF4BA3DA40C496C93EC58FAC85F18B9'
        }
    ],
}";

        var converted =
            editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as
                BlockListModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(2, converted.Count);

        var item0 = converted[0];
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Content.Key);
        Assert.AreEqual("Test1", item0.Content.ContentType.Alias);
        Assert.AreEqual(Guid.Parse("1F613E26CE274898908A561437AF5100"), item0.Settings.Key);
        Assert.AreEqual("Setting2", item0.Settings.ContentType.Alias);

        var item1 = converted[1];
        Assert.AreEqual(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A"), item1.Content.Key);
        Assert.AreEqual("Test2", item1.Content.ContentType.Alias);
        Assert.AreEqual(Guid.Parse("63027539B0DB45E7B70459762D4E83DD"), item1.Settings.Key);
        Assert.AreEqual("Setting1", item1.Settings.ContentType.Alias);
    }

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
        var publishedElement = Mock.Of<IPublishedElement>();

        var json = @"
{
    layout: {
        '" + Constants.PropertyEditors.Aliases.BlockList + @"': [
            {
                'contentUdi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D',
                'settingsUdi': 'umb://element/1F613E26CE274898908A561437AF5100'
            },
            {
                'contentUdi': 'umb://element/0A4A416E547D464FABCC6F345C17809A',
                'settingsUdi': 'umb://element/63027539B0DB45E7B70459762D4E83DD'
            }
        ]
    },
    contentData: [
        {
            'contentTypeKey': '" + ContentKey1 + @"',
            'udi': 'umb://element/1304E1DDAC87439684FE8A399231CB3D'
        },
        {
            'contentTypeKey': '" + ContentKey2 + @"',
            'udi': 'umb://element/E05A034704424AB3A520E048E6197E79'
        },
        {
            'contentTypeKey': '" + ContentKey2 + @"',
            'udi': 'umb://element/0A4A416E547D464FABCC6F345C17809A'
        }
    ],
    settingsData: [
        {
            'contentTypeKey': '" + SettingKey1 + @"',
            'udi': 'umb://element/63027539B0DB45E7B70459762D4E83DD'
        },
        {
            'contentTypeKey': '" + SettingKey2 + @"',
            'udi': 'umb://element/1F613E26CE274898908A561437AF5100'
        },
        {
            'contentTypeKey': '" + SettingKey2 + @"',
            'udi': 'umb://element/BCF4BA3DA40C496C93EC58FAC85F18B9'
        }
    ],
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
