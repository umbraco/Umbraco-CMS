// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
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
    public void Get_Value_Type_Is_Typed_By_The_Content_And_Settings_Element_Types()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor(ContentKey1, SettingKey2));

        Type valueType = editor.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(BlockListItem<,>), valueType.GetGenericTypeDefinition());
        Assert.IsTrue(ModelType.Equals(
            typeof(BlockListItem<,>).MakeGenericType(ModelType.For(ContentAlias1), ModelType.For(SettingAlias2)),
            valueType));
    }

    [Test]
    public void Get_Value_Type_Is_Typed_By_The_Content_Element_Type_When_There_Are_No_Settings()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor(ContentKey1, settingsElementTypeKey: null));

        Type valueType = editor.GetPropertyValueType(propertyType);

        Assert.AreEqual(typeof(BlockListItem<>), valueType.GetGenericTypeDefinition());
        Assert.IsTrue(ModelType.Equals(
            typeof(BlockListItem<>).MakeGenericType(ModelType.For(ContentAlias1)),
            valueType));
    }

    [Test]
    public void Get_Value_Type_Is_Untyped_When_The_Content_Element_Type_Cannot_Be_Resolved()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter(Mock.Of<IContentTypeService>());
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor(ContentKey1, SettingKey2));

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
    /// The declared type can only be as strong as the model the block is converted into, so the conversion has to
    /// produce the generic model the type is declared against.
    /// </summary>
    /// <remarks>
    /// The generic arguments themselves cannot be compared here: the declared type carries
    /// <see cref="ModelType"/> placeholders for models builder to map, where the converted value carries the models
    /// the published model factory actually produced.
    /// </remarks>
    [Test]
    public void Convert_Produces_A_Generic_Block_List_Item()
    {
        SingleBlockPropertyValueConverter editor = CreateConverter();
        IPublishedPropertyType propertyType = GetPropertyType(ConfigFor(ContentKey1, settingsElementTypeKey: null));

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
        Assert.AreEqual(typeof(BlockListItem<,>), converted.GetType().GetGenericTypeDefinition());
        Assert.AreEqual(ContentAlias1, converted.Content.ContentType.Alias);
    }

    private static SingleBlockConfiguration ConfigFor(Guid contentElementTypeKey, Guid? settingsElementTypeKey) => new()
    {
        Blocks =
        [
            new BlockListConfiguration.BlockConfiguration
            {
                ContentElementTypeKey = contentElementTypeKey,
                SettingsElementTypeKey = settingsElementTypeKey,
            },
        ],
    };

    /// <summary>
    /// Gets a content type service that resolves the element types the base fixture describes, as the model type of a
    /// block is derived from the alias of its element type.
    /// </summary>
    private IContentTypeService ElementTypeResolvingContentTypeService()
    {
        var contentTypeService = new Mock<IContentTypeService>();

        foreach ((Guid key, var alias) in new[]
                 {
                     (ContentKey1, ContentAlias1),
                     (ContentKey2, ContentAlias2),
                     (SettingKey1, SettingAlias1),
                     (SettingKey2, SettingAlias2),
                 })
        {
            contentTypeService.Setup(x => x.Get(key)).Returns(Mock.Of<IContentType>(x => x.Alias == alias));
        }

        return contentTypeService.Object;
    }

    private SingleBlockPropertyValueConverter CreateConverter(IContentTypeService? contentTypeService = null)
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
            Mock.Of<IElementCacheService>(),
            contentTypeService ?? ElementTypeResolvingContentTypeService());
    }
}
