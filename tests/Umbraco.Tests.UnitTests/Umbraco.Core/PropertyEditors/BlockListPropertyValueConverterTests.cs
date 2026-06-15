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

[TestFixture]
public class BlockListPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<BlockListConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockList;

    private BlockListPropertyValueConverter CreateConverter()
    {
        var blockElementServiceMock = new Mock<IBlockElementService>();
        var publishedContentTypeCache = GetPublishedContentTypeCache();
        blockElementServiceMock
            .Setup(service => service.BuildElementAsync(It.IsAny<BlockItemData>(), It.IsAny<bool?>()))
            .Returns<BlockItemData, bool?>((blockItemData, preview) =>
            {
                var publishedElementType = publishedContentTypeCache.Get(PublishedItemType.Element, blockItemData.ContentTypeKey);

                var elementTypeMock = Mock.Of<IPublishedContentType>(mock =>
                    mock.Variations == publishedElementType.Variations
                    && mock.Key == publishedElementType.Key
                    && mock.Alias == publishedElementType.Alias);

                var elementMock = Mock.Of<IPublishedElement>(mock =>
                    mock.Key == blockItemData.Key
                    && mock.ContentType == elementTypeMock);

                return Task.FromResult(elementMock);
            });

        var publishedModelFactory = new NoopPublishedModelFactory();
        var blockVarianceHandler = new BlockEditorVarianceHandler(Mock.Of<ILanguageService>(), Mock.Of<IContentTypeService>(), Mock.Of<IVariationContextAccessor>());
        var editor = new BlockListPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(publishedContentTypeCache, publishedModelFactory, Mock.Of<IVariationContextAccessor>(), blockVarianceHandler, blockElementServiceMock.Object),
            Mock.Of<IContentTypeService>(),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            new BlockListPropertyValueConstructorCache(),
            Mock.Of<IVariationContextAccessor>(),
            blockVarianceHandler,
            Mock.Of<ILanguageService>(),
            Mock.Of<IPropertyRenderingContextAccessor>());
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
        Assert.That(editor.IsConverter(
            Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == Constants.PropertyEditors.Aliases.BlockList)), Is.True);
        Assert.That(editor.IsConverter(Mock.Of<IPublishedPropertyType>(x =>
            x.EditorAlias == Constants.PropertyEditors.Aliases.NestedContent)), Is.False);
    }

    [Test]
    public void Get_Value_Type_Multiple()
    {
        var editor = CreateConverter();
        var config = ConfigForMany();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.That(valueType, Is.EqualTo(typeof(BlockListModel)));
    }

    [Test]
    public void Get_Value_TypeSingle()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        // the result is always block list model
        Assert.That(valueType, Is.EqualTo(typeof(BlockListModel)));
    }

    [Test]
    public void Get_Value_TypeSingleBlockMode()
    {
        var editor = CreateConverter();
        var config = ConfigForSingleBlockMode();

        var dataType = new PublishedDataType(1, "test", "test", new Lazy<object>(() => config));
        var propType = Mock.Of<IPublishedPropertyType>(x => x.DataType == dataType);

        var valueType = editor.GetPropertyValueType(propType);

        Assert.That(valueType, Is.EqualTo(typeof(BlockListItem)));
    }

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));

        json = string.Empty;
        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));
    }

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));

        json = @"{
""layout"": {},
""data"": []}";
        converted = editor.ConvertIntermediateToObject(publishedElement, propertyType, PropertyCacheLevel.None, json, false) as BlockListModel;

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted.Count, Is.EqualTo(0));
    }

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted, Has.Count.EqualTo(1));
        var item0 = converted[0].Content;
        Assert.That(item0.Key, Is.EqualTo(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D")));
        Assert.That(item0.ContentType.Alias, Is.EqualTo("Test1"));
        Assert.That(converted[0].Settings, Is.Null);
        Assert.That(converted[0].ContentKey, Is.EqualTo(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D")));
    }

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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted, Has.Count.EqualTo(2));

        var item0 = converted[0];
        Assert.That(item0.Content.Key, Is.EqualTo(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D")));
        Assert.That(item0.Content.ContentType.Alias, Is.EqualTo("Test1"));
        Assert.That(item0.Settings!.Key, Is.EqualTo(Guid.Parse("1F613E26-CE27-4898-908A-561437AF5100")));
        Assert.That(item0.Settings.ContentType.Alias, Is.EqualTo("Setting2"));

        var item1 = converted[1];
        Assert.That(item1.Content.Key, Is.EqualTo(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A")));
        Assert.That(item1.Content.ContentType.Alias, Is.EqualTo("Test2"));
        Assert.That(item1.Settings!.Key, Is.EqualTo(Guid.Parse("63027539-B0DB-45E7-B704-59762D4E83DD")));
        Assert.That(item1.Settings.ContentType.Alias, Is.EqualTo("Setting1"));
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

        Assert.That(converted, Is.Not.Null);
        Assert.That(converted, Has.Count.EqualTo(1));

        var item0 = converted[0];
        Assert.That(item0.Content.Key, Is.EqualTo(Guid.Parse("0A4A416E-547D-464F-ABCC-6F345C17809A")));
        Assert.That(item0.Content.ContentType.Alias, Is.EqualTo("Test2"));
        Assert.That(item0.Settings, Is.Null);
    }
}
