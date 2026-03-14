using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

/// <summary>
/// Contains unit tests for the <see cref="PublishedPropertyType"/> class within the Delivery API of Umbraco CMS.
/// </summary>
[TestFixture]
public class PublishedPropertyTypeTests : DeliveryApiTests
{
    /// <summary>
    /// Tests that PropertyDeliveryApiValue uses the delivery API value for delivery API output.
    /// </summary>
    [Test]
    public void PropertyDeliveryApiValue_UsesDeliveryApiValueForDeliveryApiOutput()
    {
        var result = DeliveryApiPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false, false);
        Assert.NotNull(result);
        Assert.AreEqual("Delivery API value", result);
    }

    /// <summary>
    /// Tests that <see cref="DeliveryApiPropertyType.ConvertInterToObject"/> returns the default value ("Default value")
    /// when called with default output parameters.
    /// </summary>
    [Test]
    public void DeliveryApiPropertyValue_UsesDefaultValueForDefaultOutput()
    {
        var result = DeliveryApiPropertyType.ConvertInterToObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }

    /// <summary>
    /// Tests that the NonDeliveryApiPropertyValueConverter falls back to the default value for Delivery API output.
    /// </summary>
    [Test]
    public void NonDeliveryApiPropertyValueConverter_PerformsFallbackToDefaultValueForDeliveryApiOutput()
    {
        var result = DefaultPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }
}
