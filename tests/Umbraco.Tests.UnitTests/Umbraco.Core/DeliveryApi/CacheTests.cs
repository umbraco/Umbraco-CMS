using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Blocks;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class CacheTests : DeliveryApiTests
{
    [TestCase(PropertyCacheLevel.Elements, false, 1)]
    [TestCase(PropertyCacheLevel.Elements, true, 1)]
    [TestCase(PropertyCacheLevel.Element, false, 1)]
    [TestCase(PropertyCacheLevel.Element, true, 1)]
    [TestCase(PropertyCacheLevel.None, false, 4)]
    [TestCase(PropertyCacheLevel.None, true, 4)]
    public void PublishedElementProperty_CachesDeliveryApiValueConversion(PropertyCacheLevel cacheLevel, bool expanding, int expectedConverterHits)
    {
        var propertyValueConverter = new Mock<IDeliveryApiPropertyValueConverter>();
        var invocationCount = 0;
        propertyValueConverter.Setup(p => p.ConvertIntermediateToDeliveryApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>())).Returns(() => $"Delivery API value: {++invocationCount}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "something", "Some.Thing");

        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(e => e.ItemType).Returns(PublishedItemType.Element);
        var element = new Mock<IPublishedElement>();
        element.SetupGet(e => e.ContentType).Returns(elementType.Object);

        var propertyData = new PropertyData { Culture = "abc", Segment = string.Empty, Value = "n/a" };

        var prop1 = new PublishedProperty(propertyType, element.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), cacheLevel);

        var results = new List<string>
        {
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString(),
            prop1.GetDeliveryApiValue(expanding)!.ToString()
        };

        Assert.AreEqual("Delivery API value: 1", results.First());
        Assert.AreEqual(expectedConverterHits, results.Distinct().Count());

        propertyValueConverter.Verify(
            converter => converter.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()),
            Times.Exactly(expectedConverterHits));
    }

    [TestCase(PropertyCacheLevel.Elements, "value: en-US", "value: en-US")]
    [TestCase(PropertyCacheLevel.Element, "value: en-US", "value: da-DK")]
    [TestCase(PropertyCacheLevel.None, "value: en-US", "value: da-DK")]
    public void PublishedElementProperty_DeliveryApiValue_CultureIsolationAcrossSeparateInstances_DependsOnCacheLevel(
        PropertyCacheLevel cacheLevel, string expectedFirstResult, string expectedSecondResult)
    {
        // Simulates a converter (like the Rich Text block converter) whose Delivery API output depends on the
        // ambient VariationContext even though the property itself is invariant - e.g. it embeds culture-variant
        // blocks. Two SEPARATE PublishedProperty instances (as a fresh request would build) share the same
        // backing elements cache, mirroring how the elements cache persists across real, separate HTTP requests.
        //
        // At PropertyCacheLevel.Elements, the second instance intentionally reuses the first instance's cached
        // value - that's the leak this cache level allows, and the second test case asserts it happens.
        // Element and None both re-invoke the converter per instance, so they correctly isolate cultures.
        var variationContextAccessor = new TestVariationContextAccessor();

        var propertyValueConverter = new Mock<IDeliveryApiPropertyValueConverter>();
        propertyValueConverter
            .Setup(p => p.ConvertIntermediateToDeliveryApiObject(
                It.IsAny<IPublishedElement>(),
                It.IsAny<IPublishedPropertyType>(),
                It.IsAny<PropertyCacheLevel>(),
                It.IsAny<object?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(() => $"value: {variationContextAccessor.VariationContext?.Culture}");
        propertyValueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        propertyValueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);
        propertyValueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevelForExpansion(It.IsAny<IPublishedPropertyType>())).Returns(cacheLevel);

        // Both the property and its owning element type are invariant, matching a Rich Text property nested
        // inside an invariant Block List element.
        var propertyType = SetupPublishedPropertyType(propertyValueConverter.Object, "richText", "Rich.Text");

        var elementType = new Mock<IPublishedContentType>();
        elementType.SetupGet(e => e.ItemType).Returns(PublishedItemType.Element);
        elementType.SetupGet(e => e.Variations).Returns(ContentVariation.Nothing);
        var element = new Mock<IPublishedElement>();
        element.SetupGet(e => e.ContentType).Returns(elementType.Object);
        element.SetupGet(e => e.Key).Returns(Guid.NewGuid());

        var propertyData = new PropertyData { Culture = string.Empty, Segment = string.Empty, Value = "n/a" };
        var elementsCache = new ElementsDictionaryAppCache();

        // First "request": ambient culture is en-US, resolved via a fresh PublishedProperty instance, exactly
        // as a fresh HTTP request would build a fresh object graph.
        variationContextAccessor.VariationContext = new VariationContext("en-US");
        var firstRequestProperty = new PublishedProperty(propertyType, element.Object, variationContextAccessor, CreatePropertyRenderingContextAccessor(), false, [propertyData], elementsCache, cacheLevel);
        var firstResult = firstRequestProperty.GetDeliveryApiValue(false)!.ToString();

        // A second, separate request for a different ambient culture - only isolated from the first when the
        // cache level forces the converter to be re-invoked (see the leak note above).
        variationContextAccessor.VariationContext = new VariationContext("da-DK");
        var secondRequestProperty = new PublishedProperty(propertyType, element.Object, variationContextAccessor, CreatePropertyRenderingContextAccessor(), false, [propertyData], elementsCache, cacheLevel);
        var secondResult = secondRequestProperty.GetDeliveryApiValue(false)!.ToString();

        Assert.AreEqual(expectedFirstResult, firstResult);
        Assert.AreEqual(expectedSecondResult, secondResult);
    }

    [Test]
    public void RteBlockRenderingValueConverter_GetDeliveryApiPropertyCacheLevel_IsNone()
    {
        RteBlockRenderingValueConverter converter = CreateRteBlockRenderingValueConverter();

        Assert.AreEqual(PropertyCacheLevel.None, converter.GetDeliveryApiPropertyCacheLevel(Mock.Of<IPublishedPropertyType>()));
    }

    // Exercises the concrete converter rather than a mock, so this fails if GetDeliveryApiPropertyCacheLevel
    // regresses back to PropertyCacheLevel.Elements (the bug behind #23951: cached RTE block content leaking
    // its rendered culture into later requests for a different culture).
    private static RteBlockRenderingValueConverter CreateRteBlockRenderingValueConverter()
    {
        var variationContextAccessor = new TestVariationContextAccessor();
        var blockEditorVarianceHandler = new BlockEditorVarianceHandler(
            Mock.Of<ILanguageService>(),
            Mock.Of<IContentTypeService>(),
            variationContextAccessor);

        var deliveryApiSettingsMonitor = new Mock<IOptionsMonitor<DeliveryApiSettings>>();
        deliveryApiSettingsMonitor.SetupGet(m => m.CurrentValue).Returns(new DeliveryApiSettings());

        var contentSettingsMonitor = new Mock<IOptionsMonitor<ContentSettings>>();
        contentSettingsMonitor.SetupGet(m => m.CurrentValue).Returns(new ContentSettings());

        return new RteBlockRenderingValueConverter(
            new HtmlLocalLinkParser(Mock.Of<IPublishedUrlProvider>()),
            new HtmlUrlParser(contentSettingsMonitor.Object, NullLogger<HtmlUrlParser>.Instance, Mock.Of<IProfilingLogger>(), Mock.Of<IIOHelper>()),
            new HtmlImageSourceParser(_ => string.Empty, Mock.Of<IImageUrlTokenGenerator>()),
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
            NullLogger<RteBlockRenderingValueConverter>.Instance,
            variationContextAccessor,
            blockEditorVarianceHandler,
            deliveryApiSettingsMonitor.Object,
            Mock.Of<ILanguageService>(),
            new TestPropertyRenderingContextAccessor());
    }
}
