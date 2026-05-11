using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ElementPickerValueConverterTests : PropertyValueConverterTests
{
    private ElementPickerValueConverter CreateValueConverter() =>
        new(
            new SystemTextJsonSerializer(new DefaultJsonSerializerEncoderFactory()),
            PublishedElementCacheMock.Object,
            CreateVariationContextAccessor(),
            new ApiElementBuilder(CreateOutputExpansionStrategyAccessor()));

    [Test]
    public void ElementPickerValueConverter_BuildsDeliveryApiOutput()
    {
        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IEnumerable<IApiElement>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{PublishedElement.Key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result!.Count());
        var element = result.First();
        Assert.AreEqual(PublishedElement.Key, element.Id);
        Assert.AreEqual("TheElementType", element.ContentType);
        Assert.IsEmpty(element.Properties);
    }

    [Test]
    public void ElementPickerValueConverter_RendersContentProperties()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("theElementType");
        contentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var publishedElement = new Mock<IPublishedElement>();
        publishedElement.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var propertyData = new PropertyData { Value = "n/a", Culture = "abc", Segment = string.Empty };

        var prop1 = new PublishedProperty(DeliveryApiPropertyType, publishedElement.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);
        var prop2 = new PublishedProperty(DefaultPropertyType, publishedElement.Object, CreateVariationContextAccessor(), CreatePropertyRenderingContextAccessor(), false, [propertyData], new ElementsDictionaryAppCache(), PropertyCacheLevel.None);

        var publishedPropertyType = new Mock<IPublishedPropertyType>();
        publishedPropertyType.SetupGet(p => p.Alias).Returns("test");

        var key = Guid.NewGuid();
        ConfigurePublishedElementMock(publishedElement, key, "The element", contentType.Object, [prop1, prop2]);

        PublishedElementCacheMock
            .Setup(ecc => ecc.GetByIdAsync(key, false))
            .Returns(Task.FromResult(publishedElement.Object));

        var valueConverter = CreateValueConverter();
        Assert.AreEqual(typeof(IEnumerable<IApiElement>), valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.NotNull(result);
        Assert.AreEqual(1, result!.Count());
        var element = result.First();
        Assert.AreEqual(key, element.Id);
        Assert.AreEqual("theElementType", element.ContentType);
        Assert.AreEqual(2, element.Properties.Count);
        Assert.AreEqual("Delivery API value", element.Properties[DeliveryApiPropertyType.Alias]);
        Assert.AreEqual("Default value", element.Properties[DefaultPropertyType.Alias]);
    }
}
