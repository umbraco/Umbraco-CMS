using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class PropertyRendererTests : DeliveryApiTests
{
    [TestCase(123)]
    [TestCase("hello, world")]
    [TestCase(null)]
    [TestCase("")]
    public void NoFallback_YieldsPropertyValueWhenValueIsSet(object value)
    {
        var property = SetupProperty(value, true);
        var renderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        Assert.AreEqual(value, renderer.GetPropertyValue(property, false));
    }

    [TestCase(123)]
    [TestCase("hello, world")]
    [TestCase(null)]
    [TestCase("")]
    public void NoFallback_YieldsNullWhenValueIsNotSet(object? value)
    {
        var property = SetupProperty(value, false);
        var renderer = new ApiPropertyRenderer(new NoopPublishedValueFallback());

        Assert.AreEqual(null, renderer.GetPropertyValue(property, false));
    }

    [TestCase(123)]
    [TestCase("hello, world")]
    [TestCase(null)]
    [TestCase("")]
    public void CustomFallback_YieldsCustomFallbackValueWhenValueIsNotSet(object? value)
    {
        var property = SetupProperty(value, false);
        object? defaultValue = "Default value";
        var customPublishedValueFallback = new Mock<IPublishedValueFallback>();
        customPublishedValueFallback
            .Setup(p => p.TryGetValue(property, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Fallback>(), It.IsAny<object?>(), out defaultValue))
            .Returns(true);
        var renderer = new ApiPropertyRenderer(customPublishedValueFallback.Object);

        Assert.AreEqual("Default value", renderer.GetPropertyValue(property, false));
    }

    private IPublishedProperty SetupProperty(object? value, bool isValue)
    {
        var propertyTypeMock = new Mock<IPublishedPropertyType>();
        propertyTypeMock.Setup(p => p.IsDeliveryApiValue(It.IsAny<object?>(), It.IsAny<PropertyValueLevel>())).Returns(isValue);
        propertyTypeMock.SetupGet(p => p.CacheLevel).Returns(PropertyCacheLevel.None);
        propertyTypeMock.SetupGet(p => p.DeliveryApiCacheLevel).Returns(PropertyCacheLevel.None);

        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(p => p.PropertyType).Returns(propertyTypeMock.Object);
        propertyMock
            .Setup(p => p.GetDeliveryApiValue(It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(value);

        return propertyMock.Object;
    }
}
