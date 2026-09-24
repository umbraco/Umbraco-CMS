using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Blocks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.DeliveryApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors;

/// <summary>
/// Verifies that the default converters shipped for the same property editor resolve to the most specific one through
/// shadowing, instead of relying on the less specific converter excluding the editor or being removed from the collection.
/// </summary>
[TestFixture]
public class DefaultPropertyValueConverterShadowingTests
{
    [TestCase(Constants.PropertyEditors.Aliases.MediaPicker3)]
    [TestCase("My.Custom.Json")]
    public void JsonValueConverter_IsConverterForAnyPropertyEditorWithJsonValueType(string editorAlias)
    {
        var converter = new JsonValueConverter(PropertyEditors(editorAlias, ValueTypes.Json), Mock.Of<ILogger<JsonValueConverter>>());

        Assert.IsTrue(converter.IsConverter(Mock.Of<IPublishedPropertyType>(x => x.EditorAlias == editorAlias)));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void MediaPicker3_ResolvesMediaPickerWithCropsValueConverterOverJsonValueConverter(bool jsonConverterFirst)
    {
        IPropertyValueConverter jsonConverter = new JsonValueConverter(
            PropertyEditors(Constants.PropertyEditors.Aliases.MediaPicker3, ValueTypes.Json),
            Mock.Of<ILogger<JsonValueConverter>>());
        IPropertyValueConverter mediaPickerConverter = new MediaPickerWithCropsValueConverter(
            Mock.Of<IPublishedMediaCache>(),
            Mock.Of<IPublishedUrlProvider>(),
            Mock.Of<IPublishedValueFallback>(),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IApiMediaWithCropsBuilder>());

        IPublishedPropertyType propertyType = PublishedPropertyType(
            Constants.PropertyEditors.Aliases.MediaPicker3,
            jsonConverterFirst ? [jsonConverter, mediaPickerConverter] : [mediaPickerConverter, jsonConverter]);

        Assert.AreEqual(typeof(MediaWithCrops), propertyType.ModelClrType);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void RichText_ResolvesRteBlockRenderingValueConverterOverSimpleRichTextValueConverter(bool simpleConverterFirst)
    {
        IPropertyValueConverter simpleConverter = new SimpleRichTextValueConverter();
        IPropertyValueConverter blockRenderingConverter = CreateRteBlockRenderingValueConverter();

        IPublishedPropertyType propertyType = PublishedPropertyType(
            Constants.PropertyEditors.Aliases.RichText,
            simpleConverterFirst ? [simpleConverter, blockRenderingConverter] : [blockRenderingConverter, simpleConverter]);

        Assert.AreEqual(typeof(RichTextModel), propertyType.DeliveryApiModelClrType);
    }

    private static PropertyEditorCollection PropertyEditors(string editorAlias, string valueType)
    {
        var valueEditor = Mock.Of<IDataValueEditor>(x => x.ValueType == valueType);
        var dataEditor = Mock.Of<IDataEditor>(x => x.Alias == editorAlias && x.GetValueEditor() == valueEditor);
        return new PropertyEditorCollection(new DataEditorCollection(() => [dataEditor]));
    }

    private static IPublishedPropertyType PublishedPropertyType(string editorAlias, IPropertyValueConverter[] converters)
    {
        var dataType = new PublishedDataType(123, editorAlias, editorAlias, new Lazy<object?>(() => null));
        var contentTypeFactory = Mock.Of<IPublishedContentTypeFactory>(x => x.GetDataType(dataType.Id) == dataType);

        return new PublishedPropertyType(
            "test",
            dataType.Id,
            true,
            ContentVariation.Nothing,
            new PropertyValueConverterCollection(() => converters),
            Mock.Of<IPublishedModelFactory>(),
            contentTypeFactory);
    }

    private static RteBlockRenderingValueConverter CreateRteBlockRenderingValueConverter()
    {
        var publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        var variationContextAccessor = Mock.Of<IVariationContextAccessor>();
        var languageService = Mock.Of<ILanguageService>();
        var blockEditorVarianceHandler = new BlockEditorVarianceHandler(languageService, Mock.Of<IContentTypeService>(), variationContextAccessor);

        return new RteBlockRenderingValueConverter(
            new HtmlLocalLinkParser(publishedUrlProvider),
            new HtmlUrlParser(
                Mock.Of<IOptionsMonitor<ContentSettings>>(x => x.CurrentValue == new ContentSettings()),
                Mock.Of<ILogger<HtmlUrlParser>>(),
                Mock.Of<IProfilingLogger>(),
                Mock.Of<IIOHelper>()),
            new HtmlImageSourceParser(publishedUrlProvider, Mock.Of<IImageUrlTokenGenerator>()),
            Mock.Of<IApiRichTextElementParser>(),
            Mock.Of<IApiRichTextMarkupParser>(),
            Mock.Of<IPartialViewBlockEngine>(),
            new BlockEditorConverter(
                Mock.Of<IPublishedContentTypeCache>(),
                Mock.Of<IPublishedModelFactory>(),
                variationContextAccessor,
                blockEditorVarianceHandler,
                Mock.Of<IBlockElementService>()),
            Mock.Of<IJsonSerializer>(),
            Mock.Of<IApiElementBuilder>(),
            new RichTextBlockPropertyValueConstructorCache(),
            Mock.Of<ILogger<RteBlockRenderingValueConverter>>(),
            variationContextAccessor,
            blockEditorVarianceHandler,
            Mock.Of<IOptionsMonitor<DeliveryApiSettings>>(x => x.CurrentValue == new DeliveryApiSettings()),
            languageService,
            Mock.Of<IPropertyRenderingContextAccessor>(),
            Mock.Of<IElementCacheService>());
    }
}
