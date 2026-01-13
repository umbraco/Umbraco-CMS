using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class ContentFinderByUrlAliasTests
{
    private static readonly Guid DocumentKey = Guid.NewGuid();

    [TestCase("/this/is/my/alias", 1001)]
    [TestCase("/anotheralias", 1001)]
    [TestCase("/page2/alias", 10011)]
    [TestCase("/2ndpagealias", 10011)]
    [TestCase("/only/one/alias", 100111)]
    [TestCase("/ONLY/one/Alias", 100111)]
    [TestCase("/alias43", 100121)]
    public async Task Lookup_By_Url_Alias(string relativeUrl, int nodeMatch)
    {
        // Arrange
        var absoluteUrl = "http://localhost" + relativeUrl;

        // Normalize the alias the same way the service does
        var normalizedAlias = relativeUrl.TrimStart('/').TrimEnd('/').ToLowerInvariant();

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = new Mock<IUmbracoContext>();
        var publishedContentCache = new Mock<IPublishedContentCache>();
        var documentAliasService = new Mock<IDocumentAliasService>();
        var idKeyMap = new Mock<IIdKeyMap>();
        var contentItem = new Mock<IPublishedContent>();
        var fileService = new Mock<IFileService>();

        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
            .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = umbracoContext.Object))
            .Returns(true);
        umbracoContext.Setup(x => x.Content).Returns(publishedContentCache.Object);

        // Setup the document alias service to return the document key for the alias
        documentAliasService
            .Setup(x => x.GetDocumentKeyByAlias(normalizedAlias, It.IsAny<string?>(), It.IsAny<Guid?>()))
            .Returns(DocumentKey);

        // Setup the published content cache to return the content item
        publishedContentCache.Setup(x => x.GetById(DocumentKey)).Returns(contentItem.Object);
        contentItem.Setup(x => x.Id).Returns(nodeMatch);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService.Object);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            umbracoContextAccessor.Object,
            documentAliasService.Object,
            idKeyMap.Object);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(publishedRequestBuilder.PublishedContent!.Id, Is.EqualTo(nodeMatch));
    }

    [Test]
    public async Task Returns_False_When_No_Alias_Match()
    {
        // Arrange
        var absoluteUrl = "http://localhost/non-existent-alias";

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = new Mock<IUmbracoContext>();
        var publishedContentCache = new Mock<IPublishedContentCache>();
        var documentAliasService = new Mock<IDocumentAliasService>();
        var idKeyMap = new Mock<IIdKeyMap>();
        var fileService = new Mock<IFileService>();

        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
            .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = umbracoContext.Object))
            .Returns(true);
        umbracoContext.Setup(x => x.Content).Returns(publishedContentCache.Object);

        // Setup the document alias service to return null (no match)
        documentAliasService
            .Setup(x => x.GetDocumentKeyByAlias(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>()))
            .Returns((Guid?)null);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService.Object);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            umbracoContextAccessor.Object,
            documentAliasService.Object,
            idKeyMap.Object);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(publishedRequestBuilder.PublishedContent, Is.Null);
    }

    [Test]
    public async Task Returns_False_For_Root_Path()
    {
        // Arrange
        var absoluteUrl = "http://localhost/";

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = new Mock<IUmbracoContext>();
        var publishedContentCache = new Mock<IPublishedContentCache>();
        var documentAliasService = new Mock<IDocumentAliasService>();
        var idKeyMap = new Mock<IIdKeyMap>();
        var fileService = new Mock<IFileService>();

        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
            .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = umbracoContext.Object))
            .Returns(true);
        umbracoContext.Setup(x => x.Content).Returns(publishedContentCache.Object);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService.Object);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            umbracoContextAccessor.Object,
            documentAliasService.Object,
            idKeyMap.Object);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        // Assert - root path "/" should not trigger alias lookup
        Assert.That(result, Is.False);
        documentAliasService.Verify(
            x => x.GetDocumentKeyByAlias(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<Guid?>()),
            Times.Never);
    }

    [Test]
    public async Task Returns_False_When_No_Umbraco_Context()
    {
        // Arrange
        var absoluteUrl = "http://localhost/some-alias";

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var documentAliasService = new Mock<IDocumentAliasService>();
        var idKeyMap = new Mock<IIdKeyMap>();
        var fileService = new Mock<IFileService>();

        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
            .Returns(false);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService.Object);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            umbracoContextAccessor.Object,
            documentAliasService.Object,
            idKeyMap.Object);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task Uses_Domain_Root_Key_When_Domain_Is_Set()
    {
        // Arrange
        var absoluteUrl = "http://localhost/my-alias";
        var domainRootId = 1234;
        var domainRootKey = Guid.NewGuid();

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        var umbracoContext = new Mock<IUmbracoContext>();
        var publishedContentCache = new Mock<IPublishedContentCache>();
        var documentAliasService = new Mock<IDocumentAliasService>();
        var idKeyMap = new Mock<IIdKeyMap>();
        var contentItem = new Mock<IPublishedContent>();
        var fileService = new Mock<IFileService>();
        var domain = new Domain(1, "localhost", domainRootId, null, false, 1);
        var domainAndUri = new DomainAndUri(domain, new Uri("http://localhost"));

        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
            .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = umbracoContext.Object))
            .Returns(true);
        umbracoContext.Setup(x => x.Content).Returns(publishedContentCache.Object);

        // Setup IdKeyMap to convert domain root ID to key
        idKeyMap.Setup(x => x.GetKeyForId(domainRootId, UmbracoObjectTypes.Document))
            .Returns(Attempt<Guid>.Succeed(domainRootKey));

        // Setup the document alias service - verify it's called with the domain root key
        documentAliasService
            .Setup(x => x.GetDocumentKeyByAlias("my-alias", It.IsAny<string?>(), domainRootKey))
            .Returns(DocumentKey);

        publishedContentCache.Setup(x => x.GetById(DocumentKey)).Returns(contentItem.Object);
        contentItem.Setup(x => x.Id).Returns(999);

        var publishedRequestBuilder = new PublishedRequestBuilder(new Uri(absoluteUrl, UriKind.Absolute), fileService.Object);
        publishedRequestBuilder.SetDomain(domainAndUri);

        // Act
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            umbracoContextAccessor.Object,
            documentAliasService.Object,
            idKeyMap.Object);
        var result = await sut.TryFindContent(publishedRequestBuilder);

        // Assert
        Assert.That(result, Is.True);
        documentAliasService.Verify(
            x => x.GetDocumentKeyByAlias("my-alias", It.IsAny<string?>(), domainRootKey),
            Times.Once);
    }

    private delegate void TryGetUmbracoContextDelegate(out IUmbracoContext? umbracoContext);
}
