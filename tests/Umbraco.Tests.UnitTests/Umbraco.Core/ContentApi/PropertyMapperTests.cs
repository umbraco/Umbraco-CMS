using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class PropertyMapperTests : ContentApiTests
{
    [Test]
    public void ContentApiPropertyMapper_MapsContentApiPropertyValuesByAlias()
    {
        var element = new Mock<IPublishedElement>();

        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, element.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, element.Object, false, PropertyCacheLevel.None);

        element.SetupGet(e => e.Properties).Returns(new[] { prop1, prop2 });

        var mapper = new PropertyMapper();
        var result = mapper.Map(element.Object);

        Assert.NotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Content API value", result["contentApi"]);
        Assert.AreEqual("Default value", result["default"]);
    }
}
