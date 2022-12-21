using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class PublishedPropertyTypeTests : ContentApiTests
{
    [Test]
    public void PropertyContentApiValue_UsesContentApiValueForContentApiOutput()
    {
        var result = ContentApiPropertyType.ConvertInterToContentApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Content API value", result);
    }

    [Test]
    public void ContentApiPropertyValue_UsesDefaultValueForDefaultOutput()
    {
        var result = ContentApiPropertyType.ConvertInterToObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }

    [Test]
    public void NonContentApiPropertyValueConverter_PerformsFallbackToDefaultValueForContentApiOutput()
    {
        var result = DefaultPropertyType.ConvertInterToContentApiObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }
}
