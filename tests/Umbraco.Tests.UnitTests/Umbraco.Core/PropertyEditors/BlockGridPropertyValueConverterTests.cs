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
public class BlockGridPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<BlockGridConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.BlockGrid;

    [Test]
    public void Get_Value_Type()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();
        var propertyType = GetPropertyType(config);

        var valueType = editor.GetPropertyValueType(propertyType);

        // the result is always block grid model
        Assert.AreEqual(typeof(BlockGridModel), valueType);
    }

    [Test]
    public void Convert_Valid_Json()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle(SettingKey1);
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockGrid + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D"",
                ""settingsKey"": ""2D3529ED-B47B-4B10-9F6D-4B802DD5DFE2"",
                ""rowSpan"": 1,
                ""columnSpan"": 12,
                ""areas"": []
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": """ + ContentKey1 + @""",
            ""key"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
        }
    ],
    ""settingsData"": [
        {
            ""contentTypeKey"": """ + SettingKey1 + @""",
            ""key"": ""2D3529ED-B47B-4B10-9F6D-4B802DD5DFE2""
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
                BlockGridModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(1, converted.Count);
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), converted[0].Content.Key);
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), converted[0].ContentKey);
        Assert.AreEqual(ContentAlias1, converted[0].Content.ContentType.Alias);
        Assert.AreEqual(Guid.Parse("2D3529ED-B47B-4B10-9F6D-4B802DD5DFE2"), converted[0].Settings!.Key);
        Assert.AreEqual(Guid.Parse("2D3529ED-B47B-4B10-9F6D-4B802DD5DFE2"), converted[0].SettingsKey);
        Assert.AreEqual(SettingAlias1, converted[0].Settings.ContentType.Alias);
    }

    [Test]
    public void Can_Convert_Without_Settings()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockGrid + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D"",
                ""rowSpan"": 1,
                ""columnSpan"": 12,
                ""areas"": []
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
                BlockGridModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(1, converted.Count);
        var item0 = converted[0].Content;
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Key);
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), converted[0].ContentKey);
        Assert.AreEqual("Test1", item0.ContentType.Alias);
        Assert.IsNull(converted[0].Settings);
    }

    [Test]
    public void Ignores_Other_Layouts()
    {
        var editor = CreateConverter();
        var config = ConfigForSingle();
        var propertyType = GetPropertyType(config);
        var publishedElement = GetPublishedElement();

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.BlockGrid + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D"",
                ""rowSpan"": 1,
                ""columnSpan"": 12,
                ""areas"": []
            }
        ],
       """ + Constants.PropertyEditors.Aliases.BlockList + @""": [
            {
                ""contentKey"": ""1304E1DD-AC87-4396-84FE-8A399231CB3D""
            }
        ],
        ""Some.Custom.BlockEditor"": [
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
                BlockGridModel;

        Assert.IsNotNull(converted);
        Assert.AreEqual(1, converted.Count);
        var item0 = converted[0].Content;
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), item0.Key);
        Assert.AreEqual(Guid.Parse("1304E1DD-AC87-4396-84FE-8A399231CB3D"), converted[0].ContentKey);
        Assert.AreEqual("Test1", item0.ContentType.Alias);
        Assert.IsNull(converted[0].Settings);
    }

    private BlockGridPropertyValueConverter CreateConverter()
    {
        var publishedModelFactory = new NoopPublishedModelFactory();
        var blockVarianceHandler = new BlockEditorVarianceHandler(Mock.Of<ILanguageService>(), Mock.Of<IContentTypeService>());
        var editor = new BlockGridPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(GetPublishedContentTypeCache(), Mock.Of<ICacheManager>(), publishedModelFactory, Mock.Of<IVariationContextAccessor>(), blockVarianceHandler),
            new SystemTextJsonSerializer(),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new BlockGridPropertyValueConstructorCache(),
            Mock.Of<IVariationContextAccessor>(),
            blockVarianceHandler);
        return editor;
    }

    private BlockGridConfiguration ConfigForSingle(Guid? settingsElementTypeKey = null) => new()
    {
        Blocks = new[] { new BlockGridConfiguration.BlockGridBlockConfiguration { ContentElementTypeKey = ContentKey1, SettingsElementTypeKey = settingsElementTypeKey} },
    };
}
