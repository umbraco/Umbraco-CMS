using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ContentRouteBuilderTests : DeliveryApiTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForRoot(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath);
        var result = builder.Build(root);
        Assert.IsNotNull(result);
        Assert.AreEqual("/", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForChild(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath);
        var result = builder.Build(child);
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForGrandchild(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, child);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath);
        var result = builder.Build(grandchild);
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child/the-grandchild", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForCultureVariantRootAndChild()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, root);

        var builder = CreateApiContentRouteBuilder(false);
        var result = builder.Build(child, "en-us");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child-en-us", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root-en-us", result.StartItem.Path);

        result = builder.Build(child, "da-dk");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child-da-dk", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root-da-dk", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForCultureVariantRootAndCultureInvariantChild()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var builder = CreateApiContentRouteBuilder(false);
        var result = builder.Build(child, "en-us");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root-en-us", result.StartItem.Path);

        result = builder.Build(child, "da-dk");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root-da-dk", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForCultureInvariantRootAndCultureVariantChild()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, root);

        var builder = CreateApiContentRouteBuilder(false);
        var result = builder.Build(child, "en-us");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child-en-us", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);

        result = builder.Build(child, "da-dk");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child-da-dk", result.Path);
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

        var builder = CreateApiContentRouteBuilder(true);
        Assert.Throws<ArgumentException>(() => builder.Build(content.Object));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("#")]
    public void FallsBackToContentPathIfUrlProviderCannotResolveUrl(string resolvedUrl)
    {
        var result = GetUnRoutableRoute(resolvedUrl, "/the/content/route");
        Assert.IsNotNull(result);
        Assert.AreEqual("/the/content/route", result.Path);
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase("#")]
    public void YieldsNullForUnRoutableContent(string contentPath)
    {
        var result = GetUnRoutableRoute(contentPath, contentPath);
        Assert.IsNull(result);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void VerifyPublishedUrlProviderSetup(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, child);

        // yes... actually testing the mock setup here. but it's important for the rest of the tests that this behave correct, so we better test it.
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath);
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/" : "/the-root", publishedUrlProvider.GetUrl(root));
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child" : "/the-root/the-child", publishedUrlProvider.GetUrl(child));
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child/the-grandchild" : "/the-root/the-child/the-grandchild", publishedUrlProvider.GetUrl(grandchild));
    }

    private IPublishedContent SetupInvariantPublishedContent(string name, Guid key, IPublishedContent? parent = null)
    {
        var publishedContentType = CreatePublishedContentType();
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent);
        return content.Object;
    }

    private IPublishedContent SetupVariantPublishedContent(string name, Guid key, IPublishedContent? parent = null)
    {
        var publishedContentType = CreatePublishedContentType();
        publishedContentType.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent);
        var cultures = new[] { "en-us", "da-dk" };
        content
            .SetupGet(m => m.Cultures)
            .Returns(cultures.ToDictionary(
                c => c,
                c => new PublishedCultureInfo(c, $"{name}-{c}", DefaultUrlSegment(name, c), DateTime.Now)));
        return content.Object;
    }

    private Mock<IPublishedContent> CreatePublishedContentMock(IPublishedContentType publishedContentType, string name, Guid key, IPublishedContent? parent)
    {
        var content = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(content, key, name, DefaultUrlSegment(name), publishedContentType, Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.Parent).Returns(parent);
        content.SetupGet(c => c.Level).Returns((parent?.Level ?? 0) + 1);
        return content;
    }

    private static Mock<IPublishedContentType> CreatePublishedContentType()
    {
        var publishedContentType = new Mock<IPublishedContentType>();
        publishedContentType.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        publishedContentType.SetupGet(c => c.Alias).Returns("TheContentType");
        return publishedContentType;
    }

    private IPublishedUrlProvider SetupPublishedUrlProvider(bool hideTopLevelNodeFromPath)
    {
        var variantContextAccessor = Mock.Of<IVariationContextAccessor>();
        string Url(IPublishedContent content, string? culture)
            => string.Join("/", content.AncestorsOrSelf().Reverse().Skip(hideTopLevelNodeFromPath ? 1 : 0).Select(c => c.UrlSegment(variantContextAccessor, culture))).EnsureStartsWith("/");

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => Url(content, culture));
        return publishedUrlProvider.Object;
    }

    private ApiContentRouteBuilder CreateApiContentRouteBuilder(bool hideTopLevelNodeFromPath)
        => new(
            SetupPublishedUrlProvider(hideTopLevelNodeFromPath),
            CreateGlobalSettings(hideTopLevelNodeFromPath),
            Mock.Of<IVariationContextAccessor>(),
            Mock.Of<IPublishedSnapshotAccessor>());

    private IApiContentRoute? GetUnRoutableRoute(string publishedUrl, string routeById)
    {
        var publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        publishedUrlProviderMock
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);

        var publishedContentCacheMock = new Mock<IPublishedContentCache>();
        publishedContentCacheMock
            .Setup(c => c.GetRouteById(It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(routeById);

        var publishedSnapshotMock = new Mock<IPublishedSnapshot>();
        publishedSnapshotMock
            .SetupGet(s => s.Content)
            .Returns(publishedContentCacheMock.Object);
        var publishedSnapshot = publishedSnapshotMock.Object;

        var publishedSnapshotAccessorMock = new Mock<IPublishedSnapshotAccessor>();
        publishedSnapshotAccessorMock
            .Setup(a => a.TryGetPublishedSnapshot(out publishedSnapshot))
            .Returns(true);

        var content = SetupVariantPublishedContent("The Content", Guid.NewGuid());

        var builder = new ApiContentRouteBuilder(
            publishedUrlProviderMock.Object,
            CreateGlobalSettings(),
            Mock.Of<IVariationContextAccessor>(),
            publishedSnapshotAccessorMock.Object);

        return builder.Build(content);
    }
}
