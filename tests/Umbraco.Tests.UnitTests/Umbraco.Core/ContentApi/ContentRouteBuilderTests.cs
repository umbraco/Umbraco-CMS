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
    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForRoot(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        // yes... actually testing the mock setup here. but it's important!
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath);
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/" : "/the-root", publishedUrlProvider.GetUrl(root));

        var builder = new ApiContentRouteBuilder(publishedUrlProvider, CreateGlobalSettings(hideTopLevelNodeFromPath));
        var result = builder.Build(root);
        Assert.AreEqual("/", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForChild(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupPublishedContent("The Child", childKey, root);

        // yes... actually testing the mock setup here. but it's important!
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath);
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child" : "/the-root/the-child", publishedUrlProvider.GetUrl(child));

        var builder = new ApiContentRouteBuilder(publishedUrlProvider, CreateGlobalSettings(hideTopLevelNodeFromPath));
        var result = builder.Build(child);
        Assert.AreEqual("/the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForGrandchild(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupPublishedContent("The Child", childKey, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupPublishedContent("The Grandchild", grandchildKey, child);

        // yes... actually testing the mock setup here. but it's important!
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath);
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child/the-grandchild" : "/the-root/the-child/the-grandchild", publishedUrlProvider.GetUrl(grandchild));

        var builder = new ApiContentRouteBuilder(publishedUrlProvider, CreateGlobalSettings(hideTopLevelNodeFromPath));
        var result = builder.Build(grandchild);
        Assert.AreEqual("/the-child/the-grandchild", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(PublishedItemType.Media)]
    [TestCase(PublishedItemType.Element)]
    [TestCase(PublishedItemType.Member)]
    [TestCase(PublishedItemType.Unknown)]
    public void DoesNotSupportNonContentTypes(PublishedItemType itemType)
    {
        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.ItemType).Returns(itemType);

        var builder = new ApiContentRouteBuilder(SetupPublishedUrlProvider(true), CreateGlobalSettings());
        Assert.Throws<ArgumentException>(() => builder.Build(content.Object));
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

    private IPublishedUrlProvider SetupPublishedUrlProvider(bool hideTopLevelNodeFromPath)
    {
        string Url(IPublishedContent content)
            => string.Join("/", content.AncestorsOrSelf().Reverse().Skip(hideTopLevelNodeFromPath ? 1 : 0).Select(c => c.UrlSegment)).EnsureStartsWith("/");

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => Url(content));
        return publishedUrlProvider.Object;
    }
}
