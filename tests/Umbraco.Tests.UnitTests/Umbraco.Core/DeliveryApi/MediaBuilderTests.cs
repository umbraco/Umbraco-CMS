using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class MediaBuilderTests : DeliveryApiTests
{
    [Test]
    public void MediaBuilder_MapsMediaDataAndDefaultProperties()
    {
        var key = Guid.NewGuid();
        var media = SetupMedia(
            key,
            "The media",
            "media-url-segment",
            new Dictionary<string, object>
            {
                { Constants.Conventions.Media.Width, 111 },
                { Constants.Conventions.Media.Height, 222 },
                { Constants.Conventions.Media.Extension, ".my-ext" },
                { Constants.Conventions.Media.Bytes, 333 }
            });

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.NotNull(result);
        Assert.AreEqual("The media", result.Name);
        Assert.AreEqual("theMediaType", result.MediaType);
        Assert.AreEqual("media-url:media-url-segment", result.Url);
        Assert.AreEqual(key, result.Id);
        Assert.AreEqual(111, result.Width);
        Assert.AreEqual(222, result.Height);
        Assert.AreEqual(".my-ext", result.Extension);
        Assert.AreEqual(333, result.Bytes);
    }

    [Test]
    public void MediaBuilder_HandlesMissingDefaultProperties()
    {
        var media = SetupMedia(
            Guid.NewGuid(),
            "The media",
            "media-url-segment",
            new Dictionary<string, object>());

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.NotNull(result);
        Assert.IsEmpty(result.Properties);
    }

    [Test]
    public void MediaBuilder_IncludesNonDefaultProperties()
    {
        var media = SetupMedia(
            Guid.NewGuid(),
            "The media",
            "media-url-segment",
            new Dictionary<string, object> { { "myProperty", 123 }, { "anotherProperty", "A value goes here" } });

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.NotNull(result);
        Assert.AreEqual(2, result.Properties.Count);
        Assert.AreEqual(123, result.Properties["myProperty"]);
        Assert.AreEqual("A value goes here", result.Properties["anotherProperty"]);
    }

    private IPublishedContent SetupMedia(Guid key, string name, string urlSegment, Dictionary<string, object> properties)
    {
        var media = new Mock<IPublishedContent>();

        var mediaType = new Mock<IPublishedContentType>();
        mediaType.SetupGet(c => c.Alias).Returns("theMediaType");

        var mediaProperties = properties.Select(kvp => SetupProperty(kvp.Key, kvp.Value)).ToArray();

        media.SetupGet(c => c.Properties).Returns(mediaProperties);
        media.SetupGet(c => c.UrlSegment).Returns(urlSegment);
        media.SetupGet(c => c.Name).Returns(name);
        media.SetupGet(c => c.Key).Returns(key);
        media.SetupGet(c => c.ContentType).Returns(mediaType.Object);
        media.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);
        media.Setup(m => m.GetProperty(It.IsAny<string>())).Returns((string alias) => mediaProperties.FirstOrDefault(p => p.Alias == alias));

        return media.Object;
    }

    private IPublishedProperty SetupProperty<T>(string alias, T value)
    {
        var propertyMock = new Mock<IPublishedProperty>();
        propertyMock.SetupGet(p => p.Alias).Returns(alias);
        propertyMock.Setup(p => p.GetValue(It.IsAny<string?>(), It.IsAny<string>())).Returns(value);
        // needed for NoopOutputExpansionStrategyAccessor
        propertyMock.Setup(p => p.GetDeliveryApiValue(It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<string>())).Returns(value);
        return propertyMock.Object;
    }

    private IApiMediaUrlProvider SetupMediaUrlProvider()
    {
        var mock = new Mock<IApiMediaUrlProvider>();
        mock.Setup(m => m.GetUrl(It.IsAny<IPublishedContent>())).Returns((IPublishedContent media) => $"media-url:{media.UrlSegment}");
        return mock.Object;
    }
}
