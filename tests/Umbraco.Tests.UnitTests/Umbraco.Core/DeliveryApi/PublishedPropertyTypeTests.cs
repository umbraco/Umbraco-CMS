using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class PublishedPropertyTypeTests : DeliveryApiTests
{
    [Test]
    public void PropertyDeliveryApiValue_UsesDeliveryApiValueForDeliveryApiOutput()
    {
        var result = DeliveryApiPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Delivery API value", result);
    }

    [Test]
    public void DeliveryApiPropertyValue_UsesDefaultValueForDefaultOutput()
    {
        var result = DeliveryApiPropertyType.ConvertInterToObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }

    [Test]
    public void NonDeliveryApiPropertyValueConverter_PerformsFallbackToDefaultValueForDeliveryApiOutput()
    {
        var result = DefaultPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }
}
