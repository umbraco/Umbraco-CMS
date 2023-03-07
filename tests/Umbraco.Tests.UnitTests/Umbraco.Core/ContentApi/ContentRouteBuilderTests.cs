using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class ContentRouteBuilderTests : ContentApiTests
{
    [Test]
    public void CanBuildForRoot()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        var builder = new ApiContentRouteBuilder(SetupPublishedUrlProvider());
        var result = builder.Build(root);
        Assert.AreEqual("/the-root", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForChild()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupPublishedContent("The Child", childKey, root);

        var builder = new ApiContentRouteBuilder(SetupPublishedUrlProvider());
        var result = builder.Build(child);
        Assert.AreEqual("/the-root/the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForGrandchild()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupPublishedContent("The Child", childKey, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupPublishedContent("The Grandchild", grandchildKey, child);

        var builder = new ApiContentRouteBuilder(SetupPublishedUrlProvider());
        var result = builder.Build(grandchild);
        Assert.AreEqual("/the-root/the-child/the-grandchild", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    private IPublishedContent SetupPublishedContent(string name, Guid key, IPublishedContent? parent = null)
    {
        var publishedContentType = new Mock<IPublishedContentType>();
        publishedContentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        publishedContentType.SetupGet(c => c.Alias).Returns("TheContentType");

        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.Properties).Returns(Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.Name).Returns(name);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        content.SetupGet(c => c.ContentType).Returns(publishedContentType.Object);
        content.SetupGet(c => c.UrlSegment).Returns(name.ToLowerInvariant().Replace(" ", "-"));
        content.SetupGet(c => c.Parent).Returns(parent);
        content.SetupGet(c => c.Level).Returns((parent?.Level ?? 0) + 1);

        return content.Object;
    }

    private IPublishedUrlProvider SetupPublishedUrlProvider()
    {
        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => string.Join("/", content.AncestorsOrSelf().Reverse().Select(c => c.UrlSegment)));
        return publishedUrlProvider.Object;
    }
}
