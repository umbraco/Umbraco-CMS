using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
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

    [TestCase(true)]
    [TestCase(false)]
    public void CanRouteUnpublishedChild(bool hideTopLevelNodeFromPath)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root, false);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, isPreview: true);
        var result = builder.Build(child);
        Assert.IsNotNull(result);
        Assert.AreEqual($"/{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{childKey:D}", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void UnpublishedChildRouteRespectsTrailingSlashSettings(bool addTrailingSlash)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root, false);

        var builder = CreateApiContentRouteBuilder(true, addTrailingSlash, isPreview: true);
        var result = builder.Build(child);
        Assert.IsNotNull(result);
        Assert.AreEqual(addTrailingSlash, result.Path.EndsWith("/"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanRoutePublishedChildOfUnpublishedParentInPreview(bool isPreview)
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, published: false);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        var builder = CreateApiContentRouteBuilder(true, isPreview: isPreview);
        var result = builder.Build(child);

        if (isPreview)
        {
            Assert.IsNotNull(result);
            Assert.AreEqual($"/{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{childKey:D}", result.Path);
            Assert.AreEqual(rootKey, result.StartItem.Id);
            Assert.AreEqual("the-root", result.StartItem.Path);
        }
        else
        {
            Assert.IsNull(result);
        }
    }

    [Test]
    public void CanUseCustomContentPathProvider()
    {
        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, published: false);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, root);

        var apiContentPathProvider = new Mock<IApiContentPathProvider>();
        apiContentPathProvider
            .Setup(p => p.GetContentPath(It.IsAny<IPublishedContent>(), It.IsAny<string?>()))
            .Returns((IPublishedContent content, string? culture) => $"my-custom-path-for-{content.UrlSegment}");

        var builder = CreateApiContentRouteBuilder(true, apiContentPathProvider: apiContentPathProvider.Object);
        var result = builder.Build(root);
        Assert.NotNull(result);
        Assert.AreEqual("/my-custom-path-for-the-root", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);

        result = builder.Build(child);
        Assert.NotNull(result);
        Assert.AreEqual("/my-custom-path-for-the-child", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    private IPublishedContent SetupInvariantPublishedContent(string name, Guid key, IPublishedContent? parent = null, bool published = true)
    {
        var publishedContentType = CreatePublishedContentType();
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published);
        return content.Object;
    }

    private IPublishedContent SetupVariantPublishedContent(string name, Guid key, IPublishedContent? parent = null, bool published = true)
    {
        var publishedContentType = CreatePublishedContentType();
        publishedContentType.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published);
        var cultures = new[] { "en-us", "da-dk" };
        content
            .SetupGet(m => m.Cultures)
            .Returns(cultures.ToDictionary(
                c => c,
                c => new PublishedCultureInfo(c, $"{name}-{c}", DefaultUrlSegment(name, c), DateTime.Now)));
        return content.Object;
    }

    private Mock<IPublishedContent> CreatePublishedContentMock(IPublishedContentType publishedContentType, string name, Guid key, IPublishedContent? parent, bool published)
    {
        var content = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(content, key, name, DefaultUrlSegment(name), publishedContentType, Array.Empty<PublishedElementPropertyBase>());
        content.Setup(c => c.IsPublished(It.IsAny<string?>())).Returns(published);
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
        {
            return content.AncestorsOrSelf().All(c => c.IsPublished(culture))
                ? string.Join("/", content.AncestorsOrSelf().Reverse().Skip(hideTopLevelNodeFromPath ? 1 : 0).Select(c => c.UrlSegment(variantContextAccessor, culture))).EnsureStartsWith("/")
                : "#";
        }

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => Url(content, culture));
        return publishedUrlProvider.Object;
    }

    private IApiContentPathProvider SetupApiContentPathProvider(bool hideTopLevelNodeFromPath)
        => new ApiContentPathProvider(SetupPublishedUrlProvider(hideTopLevelNodeFromPath));

    private ApiContentRouteBuilder CreateApiContentRouteBuilder(bool hideTopLevelNodeFromPath, bool addTrailingSlash = false, bool isPreview = false, IPublishedSnapshotAccessor? publishedSnapshotAccessor = null, IApiContentPathProvider? apiContentPathProvider = null)
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = addTrailingSlash };
        var requestHandlerSettingsMonitorMock = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
        requestHandlerSettingsMonitorMock.Setup(m => m.CurrentValue).Returns(requestHandlerSettings);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        publishedSnapshotAccessor ??= CreatePublishedSnapshotAccessorForRoute("#");
        apiContentPathProvider ??= SetupApiContentPathProvider(hideTopLevelNodeFromPath);

        return CreateContentRouteBuilder(
            apiContentPathProvider,
            CreateGlobalSettings(hideTopLevelNodeFromPath),
            requestHandlerSettingsMonitor: requestHandlerSettingsMonitorMock.Object,
            requestPreviewService: requestPreviewServiceMock.Object,
            publishedSnapshotAccessor: publishedSnapshotAccessor);
    }

    private IApiContentRoute? GetUnRoutableRoute(string publishedUrl, string routeById)
    {
        var publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        publishedUrlProviderMock
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);
        var contentPathProvider = new ApiContentPathProvider(publishedUrlProviderMock.Object);

        var publishedSnapshotAccessor = CreatePublishedSnapshotAccessorForRoute(routeById);
        var content = SetupVariantPublishedContent("The Content", Guid.NewGuid());

        var builder = CreateContentRouteBuilder(
            contentPathProvider,
            CreateGlobalSettings(),
            publishedSnapshotAccessor: publishedSnapshotAccessor);

        return builder.Build(content);
    }

    private IPublishedSnapshotAccessor CreatePublishedSnapshotAccessorForRoute(string routeById)
    {
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

        return publishedSnapshotAccessorMock.Object;
    }
}
