using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class DeliveryApiValueConverterTests : DeliveryApiTests
{
    [Test]
    public void ApiValueConverter_RendersApiValue()
    {
        var content = new Mock<IPublishedContent>();
        var property = SetupApiProperty(content.Object, true);
        var propertyRenderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        var value = propertyRenderer.GetPropertyValue(property, false);
        Assert.AreEqual("Delivery API value", value);
    }

    [Test]
    public void DefaultValueConverter_RendersDefaultValue()
    {
        var content = new Mock<IPublishedContent>();
        var property = SetupDefaultProperty(content.Object, true);
        var propertyRenderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        var value = propertyRenderer.GetPropertyValue(property, false);
        Assert.AreEqual("Default value", value);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ApiValueConverter_UsesApiIsValue(bool isApiValue)
    {
        var content = new Mock<IPublishedContent>();
        var property = SetupApiProperty(content.Object, isApiValue);
        var propertyRenderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        var value = propertyRenderer.GetPropertyValue(property, false);
        if (isApiValue)
        {
            Assert.AreEqual("Delivery API value", value);
        }
        else
        {
            Assert.IsNull(value);
        }
    }

    [Test]
    public void DefaultValueConverter_UsesDefaultIsValue()
    {
        var content = new Mock<IPublishedContent>();
        var property = SetupDefaultProperty(content.Object, false);
        var propertyRenderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        var value = propertyRenderer.GetPropertyValue(property, false);
        Assert.IsNull(value);
    }

    private IPublishedProperty SetupDefaultProperty(IPublishedContent content, bool isValue)
    {
        var valueConverter = SetupValueConverter<IPropertyValueConverter>(isValue);
        var propertyType = SetupPublishedPropertyType(valueConverter.Object, "default", "Default.Editor");

        return new PublishedElementPropertyBase(propertyType, content, false, PropertyCacheLevel.None);
    }

    private IPublishedProperty SetupApiProperty(IPublishedContent content, bool isApiValue)
    {
        var valueConverter = SetupValueConverter<IDeliveryApiPropertyValueConverter>(true);
        valueConverter.Setup(p => p.ConvertIntermediateToDeliveryApiObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>(),
            It.IsAny<bool>())
        ).Returns("Delivery API value");
        valueConverter.Setup(p => p.GetDeliveryApiPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        valueConverter.Setup(p => p.IsDeliveryApiValue("Delivery API value", PropertyValueLevel.Object)).Returns(isApiValue);

        var propertyType = SetupPublishedPropertyType(valueConverter.Object, "deliveryApi", "Delivery.Api.Editor");

        return new PublishedElementPropertyBase(propertyType, content, false, PropertyCacheLevel.None);
    }

    private Mock<T> SetupValueConverter<T>(bool isValue)
        where T : class, IPropertyValueConverter
    {
        var valueConverter = new Mock<T>();
        valueConverter.Setup(p => p.ConvertIntermediateToObject(
            It.IsAny<IPublishedElement>(),
            It.IsAny<IPublishedPropertyType>(),
            It.IsAny<PropertyCacheLevel>(),
            It.IsAny<object?>(),
            It.IsAny<bool>())
        ).Returns("Default value");

        bool IsValue() => isValue;
        valueConverter.Setup(p => p.IsConverter(It.IsAny<IPublishedPropertyType>())).Returns(true);
        valueConverter.Setup(p => p.GetPropertyCacheLevel(It.IsAny<IPublishedPropertyType>())).Returns(PropertyCacheLevel.None);
        valueConverter.Setup(p => p.IsValue("Default value", PropertyValueLevel.Object)).Returns(() => IsValue());

        return valueConverter;
    }
}
