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
            new Dictionary<string, object>
            {
                { Constants.Conventions.Media.Width, 111 },
                { Constants.Conventions.Media.Height, 222 },
                { Constants.Conventions.Media.Extension, ".my-ext" },
                { Constants.Conventions.Media.Bytes, 333 }
            });

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Name, Is.EqualTo("The media"));
        Assert.That(result.MediaType, Is.EqualTo("theMediaType"));
        Assert.That(result.Url, Is.EqualTo("media-url:media-url-segment"));
        Assert.That(result.Id, Is.EqualTo(key));
        Assert.That(result.Width, Is.EqualTo(111));
        Assert.That(result.Height, Is.EqualTo(222));
        Assert.That(result.Extension, Is.EqualTo(".my-ext"));
        Assert.That(result.Bytes, Is.EqualTo(333));
    }

    [Test]
    public void MediaBuilder_HandlesMissingDefaultProperties()
    {
        var media = SetupMedia(
            Guid.NewGuid(),
            "The media",
            new Dictionary<string, object>());

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Properties, Is.Empty);
    }

    [Test]
    public void MediaBuilder_IncludesNonDefaultProperties()
    {
        var media = SetupMedia(
            Guid.NewGuid(),
            "The media",
            new Dictionary<string, object> { { "myProperty", 123 }, { "anotherProperty", "A value goes here" } });

        var builder = new ApiMediaBuilder(new ApiContentNameProvider(), SetupMediaUrlProvider(), Mock.Of<IPublishedValueFallback>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(media);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Properties, Has.Count.EqualTo(2));
        Assert.That(result.Properties["myProperty"], Is.EqualTo(123));
        Assert.That(result.Properties["anotherProperty"], Is.EqualTo("A value goes here"));
    }

    private IPublishedContent SetupMedia(Guid key, string name, Dictionary<string, object> properties)
    {
        var media = new Mock<IPublishedContent>();

        var mediaType = new Mock<IPublishedContentType>();
        mediaType.SetupGet(c => c.Alias).Returns("theMediaType");

        var mediaProperties = properties.Select(kvp => SetupProperty(kvp.Key, kvp.Value)).ToArray();

        media.SetupGet(c => c.Properties).Returns(mediaProperties);
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

    private IApiMediaUrlProvider SetupMediaUrlProvider(string urlSegment = "media-url-segment")
    {
        var mock = new Mock<IApiMediaUrlProvider>();
        mock.Setup(m => m.GetUrl(It.IsAny<IPublishedContent>())).Returns($"media-url:{urlSegment}");
        return mock.Object;
    }
}
