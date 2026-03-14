using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Contains unit tests for the <see cref="PropertyRenderer"/> class within the Delivery API of Umbraco CMS.
/// These tests verify the correct behavior and output of property rendering functionality.
/// </summary>
[TestFixture]
public class PropertyRendererTests : DeliveryApiTests
{
    /// <summary>
    /// Verifies that when no fallback mechanism is applied, the property renderer returns the exact value set on the property.
    /// </summary>
    /// <param name="value">The value assigned to the property for testing.</param>
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

    /// <summary>
    /// Verifies that the property renderer returns <c>null</c> when no fallback is used and the property value is not set.
    /// </summary>
    /// <param name="value">The value assigned to the property for this test case. If the value is not set (e.g., <c>null</c> or empty), the renderer should yield <c>null</c>.</param>
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

    /// <summary>
    /// Verifies that when a property value is not set (i.e., is null or empty), the custom fallback mechanism returns the specified fallback value.
    /// </summary>
    /// <param name="value">The test value assigned to the property; can be a value, null, or an empty string to simulate unset scenarios.</param>
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
        propertyTypeMock.SetupGet(p => p.CacheLevel).Returns(PropertyCacheLevel.None);
        propertyTypeMock.SetupGet(p => p.DeliveryApiCacheLevel).Returns(PropertyCacheLevel.None);

        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.Setup(p => p.PropertyType).Returns(propertyTypeMock.Object);
        propertyMock.Setup(p => p.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(isValue);
        propertyMock
            .Setup(p => p.GetDeliveryApiValue(It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(value);

        return propertyMock.Object;
    }
}
