using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class PublishedPropertyTypeTests : HeadlessTests
{
    [Test]
    public void HeadlessPropertyValue_UsesHeadlessValueForHeadlessOutput()
    {
        var result = HeadlessPropertyType.ConvertInterToHeadlessObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Headless value", result);
    }

    [Test]
    public void HeadlessPropertyValue_UsesDefaultValueForDefaultOutput()
    {
        var result = HeadlessPropertyType.ConvertInterToObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }

    [Test]
    public void NonHeadlessPropertyValueConverter_PerformsFallbackToDefaultValueForHeadlessOutput()
    {
        var result = DefaultPropertyType.ConvertInterToHeadlessObject(new Mock<IPublishedElement>().Object, PropertyCacheLevel.None, null, false);
        Assert.NotNull(result);
        Assert.AreEqual("Default value", result);
    }
}
