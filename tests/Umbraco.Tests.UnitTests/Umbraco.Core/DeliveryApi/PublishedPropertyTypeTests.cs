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
        var result = DeliveryApiPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false, false);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("Delivery API value"));
    }

    [Test]
    public void DeliveryApiPropertyValue_UsesDefaultValueForDefaultOutput()
    {
        var result = DeliveryApiPropertyType.ConvertInterToObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("Default value"));
    }

    [Test]
    public void NonDeliveryApiPropertyValueConverter_PerformsFallbackToDefaultValueForDeliveryApiOutput()
    {
        var result = DefaultPropertyType.ConvertInterToDeliveryApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false, false);
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("Default value"));
    }
}
