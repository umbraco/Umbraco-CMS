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
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ContentRouteBuilderTests : DeliveryApiTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForRoot(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, navigationQueryServiceMock, child);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), grandchild.Key)).Returns(grandchild);

        IEnumerable<Guid> ancestorsKeys = [childKey, rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(grandchildKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache);
        var result = builder.Build(grandchild);
        Assert.IsNotNull(result);
        Assert.AreEqual("/the-child/the-grandchild", result.Path);
        Assert.AreEqual(rootKey, result.StartItem.Id);
        Assert.AreEqual("the-root", result.StartItem.Path);
    }

    [Test]
    public void CanBuildForCultureVariantRootAndChild()
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache);
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

        var builder = CreateApiContentRouteBuilder(true, Mock.Of<IDocumentNavigationQueryService>());
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, navigationQueryServiceMock, child);

        var contentCache = Mock.Of<IPublishedContentCache>();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), grandchild.Key)).Returns(grandchild);

        IEnumerable<Guid> grandchildAncestorsKeys = [childKey, rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(grandchildKey, out grandchildAncestorsKeys)).Returns(true);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        // yes... actually testing the mock setup here. but it's important for the rest of the tests that this behave correct, so we better test it.
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryServiceMock.Object);
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/" : "/the-root", publishedUrlProvider.GetUrl(root));
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child" : "/the-root/the-child", publishedUrlProvider.GetUrl(child));
        Assert.AreEqual(hideTopLevelNodeFromPath ? "/the-child/the-grandchild" : "/the-root/the-child/the-grandchild", publishedUrlProvider.GetUrl(grandchild));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanRouteUnpublishedChild(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, false);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache, isPreview: true);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, false);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, addTrailingSlash, contentCache: contentCache, isPreview: true);
        var result = builder.Build(child);
        Assert.IsNotNull(result);
        Assert.AreEqual(addTrailingSlash, result.Path.EndsWith("/"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanRoutePublishedChildOfUnpublishedParentInPreview(bool isPreview)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, published: false);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, contentCache: contentCache, isPreview: isPreview);
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
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, published: false);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root);

        var contentCache = CreatePublishedContentCache("#");
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        var apiContentPathProvider = new Mock<IApiContentPathProvider>();
        apiContentPathProvider
            .Setup(p => p.GetContentPath(It.IsAny<IPublishedContent>(), It.IsAny<string?>()))
            .Returns((IPublishedContent content, string? culture) => $"my-custom-path-for-{content.UrlSegment}");

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, contentCache: contentCache, apiContentPathProvider: apiContentPathProvider.Object);
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

    private IPublishedContent SetupInvariantPublishedContent(string name, Guid key, Mock<IDocumentNavigationQueryService> navigationQueryServiceMock, IPublishedContent? parent = null, bool published = true)
    {
        var publishedContentType = CreatePublishedContentType();
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published, navigationQueryServiceMock);
        return content.Object;
    }

    private IPublishedContent SetupVariantPublishedContent(string name, Guid key, Mock<IDocumentNavigationQueryService> navigationQueryServiceMock, IPublishedContent? parent = null, bool published = true)
    {
        var publishedContentType = CreatePublishedContentType();
        publishedContentType.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published, navigationQueryServiceMock);
        var cultures = new[] { "en-us", "da-dk" };
        content
            .SetupGet(m => m.Cultures)
            .Returns(cultures.ToDictionary(
                c => c,
                c => new PublishedCultureInfo(c, $"{name}-{c}", DefaultUrlSegment(name, c), DateTime.Now)));
        return content.Object;
    }

    private Mock<IPublishedContent> CreatePublishedContentMock(IPublishedContentType publishedContentType, string name, Guid key, IPublishedContent? parent, bool published, Mock<IDocumentNavigationQueryService> navigationQueryServiceMock)
    {
        var content = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(content, key, name, DefaultUrlSegment(name), publishedContentType, Array.Empty<PublishedElementPropertyBase>());
        content.Setup(c => c.IsPublished(It.IsAny<string?>())).Returns(published);

        Guid? parentKey = parent?.Key;
        navigationQueryServiceMock.Setup(x => x.TryGetParentKey(key, out parentKey)).Returns(true);

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

    private IPublishedUrlProvider SetupPublishedUrlProvider(bool hideTopLevelNodeFromPath, IPublishedContentCache contentCache, IDocumentNavigationQueryService navigationQueryService)
    {
        var variantContextAccessor = Mock.Of<IVariationContextAccessor>();


        string Url(IPublishedContent content, string? culture)
        {
            var publishedContentStatusFilteringService = new PublishedContentStatusFilteringService(
                variantContextAccessor,
                PublishStatusQueryService,
                Mock.Of<IDocumentNavigationQueryService>(),
                Mock.Of<IPreviewService>(),
                contentCache);
            var ancestorsOrSelf = content.AncestorsOrSelf(navigationQueryService, publishedContentStatusFilteringService).ToArray();
            return ancestorsOrSelf.All(c => c.IsPublished(culture))
                ? string.Join("/", ancestorsOrSelf.Reverse().Skip(hideTopLevelNodeFromPath ? 1 : 0).Select(c => c.UrlSegment(variantContextAccessor, culture))).EnsureStartsWith("/")
                : "#";
        }

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => Url(content, culture));
        return publishedUrlProvider.Object;
    }

    private IApiContentPathProvider SetupApiContentPathProvider(bool hideTopLevelNodeFromPath, IPublishedContentCache contentCache, IDocumentNavigationQueryService navigationQueryService)
        => new ApiContentPathProvider(SetupPublishedUrlProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryService));

    private ApiContentRouteBuilder CreateApiContentRouteBuilder(bool hideTopLevelNodeFromPath, IDocumentNavigationQueryService navigationQueryService, bool addTrailingSlash = false, bool isPreview = false, IPublishedContentCache? contentCache = null, IApiContentPathProvider? apiContentPathProvider = null)
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = addTrailingSlash };
        var requestHandlerSettingsMonitorMock = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
        requestHandlerSettingsMonitorMock.Setup(m => m.CurrentValue).Returns(requestHandlerSettings);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        contentCache ??= CreatePublishedContentCache("#");
        apiContentPathProvider ??= SetupApiContentPathProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryService);

        return CreateContentRouteBuilder(
            apiContentPathProvider,
            CreateGlobalSettings(hideTopLevelNodeFromPath),
            requestHandlerSettingsMonitor: requestHandlerSettingsMonitorMock.Object,
            requestPreviewService: requestPreviewServiceMock.Object,
            contentCache: contentCache,
            navigationQueryService: navigationQueryService);
    }

    private IApiContentRoute? GetUnRoutableRoute(string publishedUrl, string routeById)
    {
        var publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        publishedUrlProviderMock
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);
        var contentPathProvider = new ApiContentPathProvider(publishedUrlProviderMock.Object);

        var contentCache = CreatePublishedContentCache(routeById);
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var content = SetupVariantPublishedContent("The Content", Guid.NewGuid(), navigationQueryServiceMock);

        var builder = CreateContentRouteBuilder(
            contentPathProvider,
            CreateGlobalSettings(),
            contentCache: contentCache);

        return builder.Build(content);
    }

    private IPublishedContentCache CreatePublishedContentCache(string routeById)
    {
        var publishedContentCacheMock = new Mock<IPublishedContentCache>();
        publishedContentCacheMock
            .Setup(c => c.GetRouteById(It.IsAny<int>(), It.IsAny<string?>()))
            .Returns(routeById);

        return publishedContentCacheMock.Object;
    }
}
