using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class PropertyMapperTests : HeadlessTests
{
    [Test]
    public void HeadlessPropertyMapper_MapsHeadlessPropertyValuesByAlias()
    {
        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(HeadlessPropertyType, element.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, element.Object, false, PropertyCacheLevel.None);

        element.SetupGet(e => e.Properties).Returns(new[] { prop1, prop2 });

        var mapper = new HeadlessPropertyMapper();
        var result = mapper.Map(element.Object);

        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Headless value", result["headless"]);
        Assert.AreEqual("Default value", result["default"]);
    }
}
