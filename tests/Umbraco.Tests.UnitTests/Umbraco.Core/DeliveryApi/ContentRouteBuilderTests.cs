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
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(root);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForChild(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanBuildForGrandchild(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, navigationQueryServiceMock, child, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), grandchild.Key)).Returns(grandchild);

        IEnumerable<Guid> ancestorsKeys = [childKey, rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(grandchildKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(grandchild);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child/the-grandchild"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
    }

    [Test]
    public void CanBuildForCultureVariantRootAndChild()
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child, "en-us");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child-en-us"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root-en-us"));

        result = builder.Build(child, "da-dk");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child-da-dk"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root-da-dk"));
    }

    [Test]
    public void CanBuildForCultureVariantRootAndCultureInvariantChild()
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupVariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child, "en-us");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root-en-us"));

        result = builder.Build(child, "da-dk");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root-da-dk"));
    }

    [Test]
    public void CanBuildForCultureInvariantRootAndCultureVariantChild()
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupVariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(false, navigationQueryServiceMock.Object, contentCache: contentCache, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child, "en-us");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child-en-us"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));

        result = builder.Build(child, "da-dk");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the-child-da-dk"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
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
    [TestCase(Constants.Routing.Unroutable)]
    public void FallsBackToContentPathIfUrlProviderCannotResolveUrl(string resolvedUrl)
    {
        var result = GetUnRoutableRoute(resolvedUrl, "/the/content/route");
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/the/content/route/"));
    }

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(Constants.Routing.Unroutable)]
    public void YieldsNullForUnRoutableContent(string contentPath)
    {
        var result = GetUnRoutableRoute(contentPath, contentPath);
        Assert.That(result, Is.Null);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void VerifyPublishedUrlProviderSetup(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var grandchildKey = Guid.NewGuid();
        var grandchild = SetupInvariantPublishedContent("The Grandchild", grandchildKey, navigationQueryServiceMock, child, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = Mock.Of<IPublishedContentCache>();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), grandchild.Key)).Returns(grandchild);

        IEnumerable<Guid> grandchildAncestorsKeys = [childKey, rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(grandchildKey, out grandchildAncestorsKeys)).Returns(true);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        // yes... actually testing the mock setup here. but it's important for the rest of the tests that this behave correct, so we better test it.
        var publishedUrlProvider = SetupPublishedUrlProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryServiceMock.Object, documentUrlServiceMock.Object);
        Assert.That(publishedUrlProvider.GetUrl(root), Is.EqualTo(hideTopLevelNodeFromPath ? "/" : "/the-root"));
        Assert.That(publishedUrlProvider.GetUrl(child), Is.EqualTo(hideTopLevelNodeFromPath ? "/the-child" : "/the-root/the-child"));
        Assert.That(publishedUrlProvider.GetUrl(grandchild), Is.EqualTo(hideTopLevelNodeFromPath ? "/the-child/the-grandchild" : "/the-root/the-child/the-grandchild"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanRouteUnpublishedChild(bool hideTopLevelNodeFromPath)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, false, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(hideTopLevelNodeFromPath, navigationQueryServiceMock.Object, contentCache: contentCache, isPreview: true, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo($"/{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{childKey:D}"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void UnpublishedChildRouteRespectsTrailingSlashSettings(bool addTrailingSlash)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, false, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x => x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, addTrailingSlash, contentCache: contentCache, isPreview: true, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path.EndsWith("/"), Is.EqualTo(addTrailingSlash));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void CanRoutePublishedChildOfUnpublishedParentInPreview(bool isPreview)
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, published: false, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, contentCache: contentCache, isPreview: isPreview, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(child);

        if (isPreview)
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Path, Is.EqualTo($"/{Constants.DeliveryApi.Routing.PreviewContentPathPrefix}{childKey:D}"));
            Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
            Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
        }
        else
        {
            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public void CanUseCustomContentPathProvider()
    {
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();

        var rootKey = Guid.NewGuid();
        var root = SetupInvariantPublishedContent("The Root", rootKey, navigationQueryServiceMock, published: false, documentUrlServiceMock: documentUrlServiceMock);

        var childKey = Guid.NewGuid();
        var child = SetupInvariantPublishedContent("The Child", childKey, navigationQueryServiceMock, root, documentUrlServiceMock: documentUrlServiceMock);

        var contentCache = CreatePublishedContentCache();
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), root.Key)).Returns(root);
        Mock.Get(contentCache).Setup(x => x.GetById(It.IsAny<bool>(), child.Key)).Returns(child);

        var apiContentPathProvider = new Mock<IApiContentPathProvider>();
        apiContentPathProvider
            .Setup(p => p.GetContentPath(It.IsAny<IPublishedContent>(), It.IsAny<string?>()))
            .Returns((IPublishedContent content, string? culture) => $"my-custom-path-for-{DefaultUrlSegment(content.Name)}");

        IEnumerable<Guid> ancestorsKeys = [rootKey];
        navigationQueryServiceMock.Setup(x=>x.TryGetAncestorsKeys(childKey, out ancestorsKeys)).Returns(true);

        var builder = CreateApiContentRouteBuilder(true, navigationQueryServiceMock.Object, contentCache: contentCache, apiContentPathProvider: apiContentPathProvider.Object, documentUrlServiceMock: documentUrlServiceMock);
        var result = builder.Build(root);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/my-custom-path-for-the-root"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));

        result = builder.Build(child);
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Path, Is.EqualTo("/my-custom-path-for-the-child"));
        Assert.That(result.StartItem.Id, Is.EqualTo(rootKey));
        Assert.That(result.StartItem.Path, Is.EqualTo("the-root"));
    }

    private IPublishedContent SetupInvariantPublishedContent(
        string name,
        Guid key,
        Mock<IDocumentNavigationQueryService> navigationQueryServiceMock,
        IPublishedContent? parent = null,
        bool published = true,
        Mock<IDocumentUrlService>? documentUrlServiceMock = null)
    {
        var publishedContentType = CreatePublishedContentType();
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published, navigationQueryServiceMock);
        documentUrlServiceMock?
            .Setup(s => s.GetUrlSegment(key, It.IsAny<string>(), It.IsAny<bool>()))
            .Returns(DefaultUrlSegment(name));
        return content.Object;
    }

    private IPublishedContent SetupVariantPublishedContent(
        string name,
        Guid key,
        Mock<IDocumentNavigationQueryService> navigationQueryServiceMock,
        IPublishedContent? parent = null,
        bool published = true,
        Mock<IDocumentUrlService>? documentUrlServiceMock = null)
    {
        var publishedContentType = CreatePublishedContentType();
        publishedContentType.SetupGet(m => m.Variations).Returns(ContentVariation.Culture);
        var content = CreatePublishedContentMock(publishedContentType.Object, name, key, parent, published, navigationQueryServiceMock);
        var cultures = new[] { "en-us", "da-dk" };
        content
            .SetupGet(m => m.Cultures)
            .Returns(cultures.ToDictionary(
                c => c,
                c => new PublishedCultureInfo(c, $"{name}-{c}", DefaultUrlSegment(name, c), DateTime.UtcNow)));

        if (documentUrlServiceMock is not null)
        {
            foreach (var culture in cultures)
            {
                var capturedCulture = culture;
                documentUrlServiceMock
                    .Setup(s => s.GetUrlSegment(key, capturedCulture, It.IsAny<bool>()))
                    .Returns(DefaultUrlSegment(name, capturedCulture));
            }
        }

        return content.Object;
    }

    private Mock<IPublishedContent> CreatePublishedContentMock(IPublishedContentType publishedContentType, string name, Guid key, IPublishedContent? parent, bool published, Mock<IDocumentNavigationQueryService> navigationQueryServiceMock)
    {
        var content = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(content, key, name, publishedContentType, Array.Empty<PublishedPropertyBase>());
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

    private IPublishedUrlProvider SetupPublishedUrlProvider(bool hideTopLevelNodeFromPath, IPublishedContentCache contentCache, IDocumentNavigationQueryService navigationQueryService, IDocumentUrlService documentUrlService)
    {
        var variantContextAccessor = Mock.Of<IVariationContextAccessor>();

        string Url(IPublishedContent content, string? culture)
        {
            var publishedContentStatusFilteringService = new PublishedContentStatusFilteringService(
                variantContextAccessor,
                PublishStatusQueryService,
                Mock.Of<IPreviewService>(),
                contentCache);
            var ancestorsOrSelf = content.AncestorsOrSelf(navigationQueryService, publishedContentStatusFilteringService).ToArray();
            return ancestorsOrSelf.All(c => c.IsPublished(culture))
                ? string.Join("/", ancestorsOrSelf.Reverse().Skip(hideTopLevelNodeFromPath ? 1 : 0).Select(c => documentUrlService.GetUrlSegment(c.Key, culture ?? string.Empty, false))).EnsureStartsWith("/")
                : Constants.Routing.Unroutable;
        }

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => Url(content, culture));
        return publishedUrlProvider.Object;
    }

    private IApiContentPathProvider SetupApiContentPathProvider(bool hideTopLevelNodeFromPath, IPublishedContentCache contentCache, IDocumentNavigationQueryService navigationQueryService, IDocumentUrlService documentUrlService)
        => new ApiContentPathProvider(SetupPublishedUrlProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryService, documentUrlService));

    private ApiContentRouteBuilder CreateApiContentRouteBuilder(
        bool hideTopLevelNodeFromPath,
        IDocumentNavigationQueryService navigationQueryService,
        bool addTrailingSlash = false,
        bool isPreview = false,
        IPublishedContentCache? contentCache = null,
        IApiContentPathProvider? apiContentPathProvider = null,
        Mock<IDocumentUrlService>? documentUrlServiceMock = null)
    {
        var requestHandlerSettings = new RequestHandlerSettings { AddTrailingSlash = addTrailingSlash };
        var requestHandlerSettingsMonitorMock = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
        requestHandlerSettingsMonitorMock.Setup(m => m.CurrentValue).Returns(requestHandlerSettings);

        var requestPreviewServiceMock = new Mock<IRequestPreviewService>();
        requestPreviewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);

        contentCache ??= CreatePublishedContentCache();
        documentUrlServiceMock ??= new Mock<IDocumentUrlService>();
        apiContentPathProvider ??= SetupApiContentPathProvider(hideTopLevelNodeFromPath, contentCache, navigationQueryService, documentUrlServiceMock.Object);

        return CreateContentRouteBuilder(
            apiContentPathProvider,
            CreateGlobalSettings(hideTopLevelNodeFromPath),
            requestHandlerSettingsMonitor: requestHandlerSettingsMonitorMock.Object,
            requestPreviewService: requestPreviewServiceMock.Object,
            contentCache: contentCache,
            navigationQueryService: navigationQueryService,
            documentUrlService: documentUrlServiceMock.Object);
    }

    private IApiContentRoute? GetUnRoutableRoute(string publishedUrl, string routeById)
    {
        var publishedUrlProviderMock = new Mock<IPublishedUrlProvider>();
        publishedUrlProviderMock
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns(publishedUrl);
        var contentPathProvider = new ApiContentPathProvider(publishedUrlProviderMock.Object);

        var contentCache = CreatePublishedContentCache();
        var navigationQueryServiceMock = new Mock<IDocumentNavigationQueryService>();
        var documentUrlServiceMock = new Mock<IDocumentUrlService>();
        var content = SetupVariantPublishedContent("The Content", Guid.NewGuid(), navigationQueryServiceMock, documentUrlServiceMock: documentUrlServiceMock);

        documentUrlServiceMock
            .Setup(m => m.GetLegacyRouteFormat(It.IsAny<Guid>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(routeById);

        var builder = CreateContentRouteBuilder(
            contentPathProvider,
            CreateGlobalSettings(),
            contentCache: contentCache,
            documentUrlService: documentUrlServiceMock.Object);

        return builder.Build(content);
    }

    private IPublishedContentCache CreatePublishedContentCache()
        => Mock.Of<IPublishedContentCache>();
}
