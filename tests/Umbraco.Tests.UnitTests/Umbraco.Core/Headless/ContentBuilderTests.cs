using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Headless;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Headless;

[TestFixture]
public class ContentBuilderTests : HeadlessTests
{
    [Test]
    public void ContentBuilder_MapsContentDataAndPropertiesCorrectly()
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(HeadlessPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        var key = Guid.NewGuid();
        content.SetupGet(c => c.Properties).Returns(new[] { prop1, prop2 });
        content.SetupGet(c => c.UrlSegment).Returns("url-segment");
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => $"url:{content.UrlSegment}");

        var builder = new HeadlessContentBuilder(new HeadlessPropertyMapper(), new HeadlessContentNameProvider(), publishedUrlProvider.Object);
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual("thePageType", result.ContentType);
        Assert.AreEqual("url:url-segment", result.Url);
        Assert.AreEqual(key, result.Key);
        Assert.AreEqual(2, result.Properties.Count);
        Assert.AreEqual("Headless value", result.Properties["headless"]);
        Assert.AreEqual("Default value", result.Properties["default"]);
    }

    [Test]
    public void ContentBuilder_CanCustomizeContentNameInHeadlessOutput()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        content.SetupGet(c => c.Properties).Returns(Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var customNameProvider = new Mock<IHeadlessContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(content.Object)).Returns($"Custom name for: {content.Object.Name}");

        var builder = new HeadlessContentBuilder(new HeadlessPropertyMapper(), customNameProvider.Object, Mock.Of<IPublishedUrlProvider>());
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Name);
    }

    [Test]
    public void ContentBuilder_MapsMediaUrlCorrectly()
    {
        var media = new Mock<IPublishedContent>();

        var mediaType = new Mock<IPublishedContentType>();
        mediaType.SetupGet(c => c.Alias).Returns("theMediaType");

        var key = Guid.NewGuid();
        media.SetupGet(c => c.Properties).Returns(Array.Empty<IPublishedProperty>());
        media.SetupGet(c => c.UrlSegment).Returns("media-url-segment");
        media.SetupGet(c => c.Name).Returns("The media");
        media.SetupGet(c => c.Key).Returns(key);
        media.SetupGet(c => c.ContentType).Returns(mediaType.Object);
        media.SetupGet(c => c.ItemType).Returns(PublishedItemType.Media);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetMediaUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, string? propertyAlias, Uri? current) => $"media-url:{content.UrlSegment}");

        var builder = new HeadlessContentBuilder(new HeadlessPropertyMapper(), new HeadlessContentNameProvider(), publishedUrlProvider.Object);
        var result = builder.Build(media.Object);

        Assert.NotNull(result);
        Assert.AreEqual("The media", result.Name);
        Assert.AreEqual("theMediaType", result.ContentType);
        Assert.AreEqual("media-url:media-url-segment", result.Url);
        Assert.AreEqual(key, result.Key);
    }
}
