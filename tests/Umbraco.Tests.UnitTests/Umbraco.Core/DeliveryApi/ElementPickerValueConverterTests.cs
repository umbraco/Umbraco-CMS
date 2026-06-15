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
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiElement>)));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{PublishedElement.Key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count(), Is.EqualTo(1));
        var element = result.First();
        Assert.That(element.Id, Is.EqualTo(PublishedElement.Key));
        Assert.That(element.ContentType, Is.EqualTo("TheElementType"));
        Assert.That(element.Properties, Is.Empty);
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
        PublishedElementCacheMock
            .Setup(ecc => ecc.GetById(false, key))
            .Returns(publishedElement.Object);

        var valueConverter = CreateValueConverter();
        Assert.That(valueConverter.GetDeliveryApiPropertyValueType(publishedPropertyType.Object), Is.EqualTo(typeof(IEnumerable<IApiElement>)));
        var result = valueConverter.ConvertIntermediateToDeliveryApiObject(
            Mock.Of<IPublishedContent>(),
            publishedPropertyType.Object,
            PropertyCacheLevel.Element,
            $"[\"{key}\"]",
            false,
            false) as IEnumerable<IApiElement>;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Count(), Is.EqualTo(1));
        var element = result.First();
        Assert.That(element.Id, Is.EqualTo(key));
        Assert.That(element.ContentType, Is.EqualTo("theElementType"));
        Assert.That(element.Properties, Has.Count.EqualTo(2));
        Assert.That(element.Properties[DeliveryApiPropertyType.Alias], Is.EqualTo("Delivery API value"));
        Assert.That(element.Properties[DefaultPropertyType.Alias], Is.EqualTo("Default value"));
    }
}
