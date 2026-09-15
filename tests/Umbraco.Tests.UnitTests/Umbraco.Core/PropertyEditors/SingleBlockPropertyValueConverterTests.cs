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
using Umbraco.Cms.Infrastructure.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

[TestFixture]
public class SingleBlockPropertyValueConverterTests : BlockPropertyValueConverterTestsBase<SingleBlockConfiguration>
{
    protected override string PropertyEditorAlias => Constants.PropertyEditors.Aliases.SingleBlock;

    [Test]
    public void Get_Value_Type_Is_Untyped()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor((ContentKey1, SettingKey2)));

        Type valueType = editor.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(BlockListItem), valueType);
    }

    /// <summary>
    /// The editor holds only one block chosen from any element type its data type allows, so the property value type
    /// can only ever be untyped - even if only a single element type is configured as allowed.
    /// </summary>
    [TestCase(false)]
    [TestCase(true)]
    public void Get_Value_Type_Is_Untyped_When_Any_Element_Types_Are_Configured(bool singleElementTypeConfiguration)
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        (Guid ContentKey, Guid? SettingsKey)[] blocks = singleElementTypeConfiguration
            ? [(ContentKey1, SettingKey1)]
            : [(ContentKey1, SettingKey1), (ContentKey2, SettingKey2)];

        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor(blocks));

        Type valueType = editor.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(BlockListItem), valueType);
    }

    [Test]
    public void Get_Value_Type_Is_Untyped_When_No_Block_Is_Configured()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(new SingleBlockConfiguration());

        Type valueType = editor.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(BlockListItem), valueType);
    }

    /// <summary>
    /// The converted value carries the element types it was built from, which the declared type does not name. It
    /// remains a <see cref="BlockListItem" />, so it is what the declared type promises either way.
    /// </summary>
    [Test]
    public void Convert_Produces_A_Block_List_Item()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor((ContentKey1, null)));

        var json = @"
{
    ""layout"": {
        """ + Constants.PropertyEditors.Aliases.SingleBlock + @""": [
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

        var converted = editor.ConvertIntermediateToObject(
            GetPublishedElement(), propertyType, PropertyCacheLevel.None, json, false) as BlockListItem;

        Assert.IsNotNull(converted);
        Assert.AreEqual(ContentAlias1, converted.Content.ContentType.Alias);
    }

    private static SingleBlockConfiguration ConfigFor(
        params (Guid ContentElementTypeKey, Guid? SettingsElementTypeKey)[] blocks) => new()
    {
        Blocks = blocks
            .Select(block => new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = block.ContentElementTypeKey,
                SettingsElementTypeKey = block.SettingsElementTypeKey,
            })
            .ToArray(),
    };

    private SingleBlockPropertyValueConverter CreateConverter()
    {
        var blockElementServiceMock = new Mock<IBlockElementService>();
        IPublishedContentTypeCache publishedContentTypeCache = GetPublishedContentTypeCache();
        blockElementServiceMock
            .Setup(service => service.BuildElementAsync(It.IsAny<IPublishedElement>(), It.IsAny<BlockItemData>(), It.IsAny<bool?>()))
            .Returns<IPublishedElement, BlockItemData, bool?>((owner, blockItemData, preview) =>
            {
                IPublishedContentType publishedElementType = publishedContentTypeCache.Get(PublishedItemType.Element, blockItemData.ContentTypeKey);

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

        return new SingleBlockPropertyValueConverter(
            Mock.Of<IProfilingLogger>(),
            new BlockEditorConverter(publishedContentTypeCache, publishedModelFactory, Mock.Of<IVariationContextAccessor>(), blockVarianceHandler, blockElementServiceMock.Object),
            new ApiElementBuilder(Mock.Of<IOutputExpansionStrategyAccessor>()),
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            new BlockListPropertyValueConstructorCache(),
            Mock.Of<IVariationContextAccessor>(),
            blockVarianceHandler,
            Mock.Of<ILanguageService>(),
            Mock.Of<IPropertyRenderingContextAccessor>(),
            Mock.Of<IElementCacheService>());
    }
}
