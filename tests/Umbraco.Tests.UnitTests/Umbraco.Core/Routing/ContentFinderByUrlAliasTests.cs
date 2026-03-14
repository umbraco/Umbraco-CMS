using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

/// <summary>
/// Tests for the ContentFinderByUrlAlias class, verifying URL alias content finding functionality.
/// </summary>
[TestFixture]
public class ContentFinderByUrlAliasTests
{
    private static readonly Guid _documentKey = Guid.NewGuid();

    private delegate void TryGetUmbracoContextDelegate(out IUmbracoContext? umbracoContext);

    /// <summary>
    /// Holds all mocks needed for ContentFinderByUrlAlias tests.
    /// </summary>
    private sealed class TestContext
    {
    /// <summary>
    /// Gets the mock for the Umbraco context accessor.
    /// </summary>
        public Mock<IUmbracoContextAccessor> UmbracoContextAccessor { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="Umbraco.Cms.Core.Web.IUmbracoContext"/> used in the test context.
    /// </summary>
        public Mock<IUmbracoContext> UmbracoContext { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="Umbraco.Cms.Core.PublishedCache.IPublishedContentCache"/> used for testing.
    /// </summary>
        public Mock<IPublishedContentCache> PublishedContentCache { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="Umbraco.Cms.Core.Services.IDocumentUrlAliasService"/> used for testing.
    /// </summary>
        public Mock<IDocumentUrlAliasService> DocumentUrlAliasService { get; } = new();

    /// <summary>
    /// Gets the mock for the document navigation query service.
    /// </summary>
        public Mock<IDocumentNavigationQueryService> DocumentNavigationQueryService { get; } = new();

    /// <summary>
    /// Gets the mock instance of <see cref="IIdKeyMap"/> used in the test context.
    /// </summary>
        public Mock<IIdKeyMap> IdKeyMap { get; } = new();

    /// <summary>
    /// Gets the mock instance of the IFileService used for testing.
    /// </summary>
        public Mock<IFileService> FileService { get; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TestContext"/> class used for unit testing the <see cref="ContentFinderByUrlAlias"/>.
    /// </summary>
        public TestContext()
        {
            // Default setup: UmbracoContext is available and returns content cache
            UmbracoContextAccessor
                .Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
                .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = UmbracoContext.Object))
                .Returns(true);
            UmbracoContext.Setup(x => x.Content).Returns(PublishedContentCache.Object);
        }

    /// <summary>
    /// Creates an instance of <see cref="ContentFinderByUrlAlias"/> for testing.
    /// </summary>
    /// <returns>A new <see cref="ContentFinderByUrlAlias"/> instance.</returns>
        public ContentFinderByUrlAlias CreateContentFinder() =>
            new(
                Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
                UmbracoContextAccessor.Object,
                DocumentNavigationQueryService.Object,
                DocumentUrlAliasService.Object,
                IdKeyMap.Object);

    /// <summary>
    /// Creates a PublishedRequestBuilder for the specified URL and optional domain.
    /// </summary>
    /// <param name="url">The URL to create the request builder for.</param>
    /// <param name="domain">An optional domain and URI to set on the request builder.</param>
    /// <returns>A PublishedRequestBuilder instance configured with the given URL and domain.</returns>
        public PublishedRequestBuilder CreateRequestBuilder(string url, DomainAndUri? domain = null)
        {
            var builder = new PublishedRequestBuilder(new Uri(url, UriKind.Absolute), FileService.Object);
            if (domain is not null)
            {
                builder.SetDomain(domain);
            }

            return builder;
        }

    /// <summary>
    /// Sets up the alias to return the specified document keys.
    /// </summary>
    /// <param name="alias">The alias to setup.</param>
    /// <param name="documentKeys">The document keys to return for the alias.</param>
        public void SetupAliasReturnsDocuments(string alias, params Guid[] documentKeys) =>
            DocumentUrlAliasService
                .Setup(x => x.GetDocumentKeysByAliasAsync(alias, It.IsAny<string?>()))
                .ReturnsAsync(documentKeys);

    /// <summary>
    /// Sets up the DocumentUrlAliasService to return an empty array for any alias query.
    /// </summary>
        public void SetupAliasReturnsEmpty() =>
            DocumentUrlAliasService
                .Setup(x => x.GetDocumentKeysByAliasAsync(It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync([]);

    /// <summary>
    /// Sets up the domain root mapping for the specified domain root ID and key.
    /// </summary>
    /// <param name="domainRootId">The ID of the domain root.</param>
    /// <param name="domainRootKey">The key of the domain root.</param>
        public void SetupDomainRoot(int domainRootId, Guid domainRootKey) =>
            IdKeyMap
                .Setup(x => x.GetKeyForId(domainRootId, UmbracoObjectTypes.Document))
                .Returns(Attempt<Guid>.Succeed(domainRootKey));

    /// <summary>
    /// Sets up the document ancestors for the specified document key.
    /// </summary>
    /// <param name="documentKey">The unique identifier of the document.</param>
    /// <param name="ancestorKeys">An array of ancestor document keys.</param>
        public void SetupDocumentAncestors(Guid documentKey, params Guid[] ancestorKeys)
        {
            IEnumerable<Guid> ancestors = ancestorKeys;
            DocumentNavigationQueryService
                .Setup(x => x.TryGetAncestorsKeys(documentKey, out ancestors))
                .Returns(true);
        }

    /// <summary>
    /// Sets up a mock IPublishedContent item with the specified document key and node ID.
    /// </summary>
    /// <param name="documentKey">The unique identifier for the document.</param>
    /// <param name="nodeId">The node ID to assign to the content item.</param>
    /// <returns>A mock of IPublishedContent configured with the given node ID.</returns>
        public Mock<IPublishedContent> SetupContentItem(Guid documentKey, int nodeId)
        {
            var contentItem = new Mock<IPublishedContent>();
            contentItem.Setup(x => x.Id).Returns(nodeId);
            PublishedContentCache.Setup(x => x.GetById(documentKey)).Returns(contentItem.Object);
            return contentItem;
        }

    /// <summary>
    /// Sets up the UmbracoContextAccessor to simulate the absence of an Umbraco context.
    /// </summary>
        public void SetupNoUmbracoContext() =>
            UmbracoContextAccessor
                .Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
                .Returns(false);
    }

    /// <summary>
    /// Verifies that content can be correctly located using a specified URL alias.
    /// </summary>
    /// <param name="relativeUrl">The relative URL alias to look up (case-insensitive, leading/trailing slashes ignored).</param>
    /// <param name="nodeMatch">The expected node ID that should be matched for the given alias.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
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
        var ctx = new TestContext();
        var normalizedAlias = relativeUrl.TrimStart('/').TrimEnd('/').ToLowerInvariant();

        ctx.SetupAliasReturnsDocuments(normalizedAlias, _documentKey);
        ctx.SetupContentItem(_documentKey, nodeMatch);

        var requestBuilder = ctx.CreateRequestBuilder("http://localhost" + relativeUrl);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(requestBuilder.PublishedContent!.Id, Is.EqualTo(nodeMatch));
    }

    /// <summary>
    /// Tests that the content finder returns false when there is no matching alias for the requested URL.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_False_When_No_Alias_Match()
    {
        // Arrange
        var ctx = new TestContext();
        ctx.SetupAliasReturnsEmpty();

        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/non-existent-alias");

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert
        Assert.That(result, Is.False);
        Assert.That(requestBuilder.PublishedContent, Is.Null);
    }

    /// <summary>
    /// Tests that the content finder returns false when the request path is the root path "/".
    /// This ensures that alias lookup is not triggered for the root path.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_False_For_Root_Path()
    {
        // Arrange
        var ctx = new TestContext();
        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/");

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - root path "/" should not trigger alias lookup
        Assert.That(result, Is.False);
        ctx.DocumentUrlAliasService.Verify(
            x => x.GetDocumentKeysByAliasAsync(It.IsAny<string>(), It.IsAny<string?>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that <see cref="ContentFinderByUrlAlias"/> returns <c>false</c> when no Umbraco context is available.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_False_When_No_Umbraco_Context()
    {
        // Arrange
        var ctx = new TestContext();
        ctx.SetupNoUmbracoContext();

        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/some-alias");

        // Act - use the full constructor for this edge case
        var sut = new ContentFinderByUrlAlias(
            Mock.Of<ILogger<ContentFinderByUrlAlias>>(),
            Mock.Of<IPublishedValueFallback>(),
            ctx.UmbracoContextAccessor.Object,
            ctx.DocumentNavigationQueryService.Object,
            Mock.Of<IPublishedContentStatusFilteringService>(),
            ctx.DocumentUrlAliasService.Object,
            ctx.IdKeyMap.Object);
        var result = await sut.TryFindContent(requestBuilder);

        // Assert
        Assert.That(result, Is.False);
    }

    /// <summary>
    /// Tests that when multiple documents match a URL alias, the document under the domain root is returned.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_Document_Under_Domain_Root_When_Multiple_Matches()
    {
        // Arrange
        var ctx = new TestContext();
        var domainRootId = 1234;
        var domainRootKey = Guid.NewGuid();
        var documentUnderDomain = Guid.NewGuid();
        var documentOutsideDomain = Guid.NewGuid();

        ctx.SetupDomainRoot(domainRootId, domainRootKey);
        ctx.SetupAliasReturnsDocuments("my-alias", documentOutsideDomain, documentUnderDomain);
        ctx.SetupDocumentAncestors(documentOutsideDomain); // No ancestors matching domain
        ctx.SetupDocumentAncestors(documentUnderDomain, domainRootKey);
        ctx.SetupContentItem(documentUnderDomain, 999);

        var domain = new DomainAndUri(new Domain(1, "localhost", domainRootId, null, false, 1), new Uri("http://localhost"));
        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/my-alias", domain);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should find the document under the domain, not the first one
        Assert.That(result, Is.True);
        ctx.PublishedContentCache.Verify(x => x.GetById(documentUnderDomain), Times.Once);
        ctx.PublishedContentCache.Verify(x => x.GetById(documentOutsideDomain), Times.Never);
    }

    /// <summary>
    /// Tests that the content finder returns the first matching document when no domain is set.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_First_Match_When_No_Domain_Is_Set()
    {
        // Arrange
        var ctx = new TestContext();
        var firstDocument = Guid.NewGuid();
        var secondDocument = Guid.NewGuid();

        ctx.SetupAliasReturnsDocuments("my-alias", firstDocument, secondDocument);
        ctx.SetupContentItem(firstDocument, 123);

        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/my-alias");

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should return the first match when no domain is set
        Assert.That(result, Is.True);
        ctx.PublishedContentCache.Verify(x => x.GetById(firstDocument), Times.Once);
        ctx.PublishedContentCache.Verify(x => x.GetById(secondDocument), Times.Never);
    }

    /// <summary>
    /// Tests that the content finder returns the document when it is the domain root.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_Document_When_It_Is_The_Domain_Root()
    {
        // Arrange
        var ctx = new TestContext();
        var domainRootId = 1234;
        var domainRootKey = Guid.NewGuid();

        ctx.SetupDomainRoot(domainRootId, domainRootKey);
        ctx.SetupAliasReturnsDocuments("my-alias", domainRootKey);
        ctx.SetupContentItem(domainRootKey, 999);

        var domain = new DomainAndUri(new Domain(1, "localhost", domainRootId, null, false, 1), new Uri("http://localhost"));
        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/my-alias", domain);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should find the document because it IS the domain root
        Assert.That(result, Is.True);
        ctx.PublishedContentCache.Verify(x => x.GetById(domainRootKey), Times.Once);
    }

    /// <summary>
    /// Tests that the content finder returns the correct document when the same alias exists under different domains.
    /// Ensures that the document associated with the requested domain is returned.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_Correct_Document_When_Same_Alias_Exists_Under_Different_Domains()
    {
        // Arrange - Two domains with different roots, each having a document with the same alias
        var ctx = new TestContext();

        var domainARootKey = Guid.NewGuid();
        var documentUnderDomainA = Guid.NewGuid();

        var domainBRootId = 2000;
        var domainBRootKey = Guid.NewGuid();
        var documentUnderDomainB = Guid.NewGuid();

        ctx.SetupDomainRoot(domainBRootId, domainBRootKey);
        ctx.SetupAliasReturnsDocuments("shared-alias", documentUnderDomainA, documentUnderDomainB);
        ctx.SetupDocumentAncestors(documentUnderDomainA, domainARootKey);
        ctx.SetupDocumentAncestors(documentUnderDomainB, domainBRootKey);
        ctx.SetupContentItem(documentUnderDomainB, 2001);

        var domainB = new DomainAndUri(new Domain(2, "domain-b.com", domainBRootId, null, false, 1), new Uri("http://domain-b.com"));
        var requestBuilder = ctx.CreateRequestBuilder("http://domain-b.com/shared-alias", domainB);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should return document B (under domain B), NOT document A (even though A comes first)
        Assert.That(result, Is.True);
        ctx.PublishedContentCache.Verify(x => x.GetById(documentUnderDomainB), Times.Once);
        ctx.PublishedContentCache.Verify(x => x.GetById(documentUnderDomainA), Times.Never);
    }

    /// <summary>
    /// Verifies that the content finder returns <c>false</c> when a domain is configured and the alias matches documents, but none of those documents are located under the specified domain root.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_False_When_Domain_Is_Set_But_No_Match_Under_Domain()
    {
        // Arrange - Domain is set, alias matches documents but none are under the domain
        var ctx = new TestContext();
        var domainRootId = 1234;
        var domainRootKey = Guid.NewGuid();
        var documentOutsideDomain = Guid.NewGuid();
        var anotherDocumentOutsideDomain = Guid.NewGuid();
        var unrelatedRootKey = Guid.NewGuid();

        ctx.SetupDomainRoot(domainRootId, domainRootKey);
        ctx.SetupAliasReturnsDocuments("my-alias", documentOutsideDomain, anotherDocumentOutsideDomain);
        ctx.SetupDocumentAncestors(documentOutsideDomain, unrelatedRootKey); // Under a different root
        ctx.SetupDocumentAncestors(anotherDocumentOutsideDomain, unrelatedRootKey); // Also under a different root

        var domain = new DomainAndUri(new Domain(1, "localhost", domainRootId, null, false, 1), new Uri("http://localhost"));
        var requestBuilder = ctx.CreateRequestBuilder("http://localhost/my-alias", domain);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should return false, NOT fall back to first match
        Assert.That(result, Is.False);
        Assert.That(requestBuilder.PublishedContent, Is.Null);
        ctx.PublishedContentCache.Verify(x => x.GetById(It.IsAny<Guid>()), Times.Never);
    }

    /// <summary>
    /// Tests that the content finder returns false when the alias exists only under a different domain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_False_When_Alias_Only_Exists_Under_Different_Domain()
    {
        // Arrange - Request is for domain A, but alias only exists under domain B
        var ctx = new TestContext();

        var domainARootId = 1000;
        var domainARootKey = Guid.NewGuid();

        var domainBRootKey = Guid.NewGuid();
        var documentUnderDomainB = Guid.NewGuid();

        ctx.SetupDomainRoot(domainARootId, domainARootKey);
        ctx.SetupAliasReturnsDocuments("alias-only-in-b", documentUnderDomainB);
        ctx.SetupDocumentAncestors(documentUnderDomainB, domainBRootKey); // Under domain B, not domain A

        var domainA = new DomainAndUri(new Domain(1, "domain-a.com", domainARootId, null, false, 1), new Uri("http://domain-a.com"));
        var requestBuilder = ctx.CreateRequestBuilder("http://domain-a.com/alias-only-in-b", domainA);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should return false, document exists but is not under the requested domain
        Assert.That(result, Is.False);
        Assert.That(requestBuilder.PublishedContent, Is.Null);
        ctx.PublishedContentCache.Verify(x => x.GetById(documentUnderDomainB), Times.Never);
    }

    /// <summary>
    /// Verifies that the content finder skips matches outside the current domain and returns the first match within the domain.
    /// This ensures that content resolution respects domain boundaries even if an earlier match exists outside the domain.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task Returns_Match_Under_Domain_Even_When_First_Match_Is_Outside_Domain()
    {
        // Arrange - First match is outside domain, second is inside domain
        // Should skip the first and return the second
        var ctx = new TestContext();
        var domainRootId = 3000;
        var domainRootKey = Guid.NewGuid();
        var firstMatchOutsideDomain = Guid.NewGuid();
        var secondMatchUnderDomain = Guid.NewGuid();
        var otherRootKey = Guid.NewGuid();

        ctx.SetupDomainRoot(domainRootId, domainRootKey);
        ctx.SetupAliasReturnsDocuments("shared-alias", firstMatchOutsideDomain, secondMatchUnderDomain);
        ctx.SetupDocumentAncestors(firstMatchOutsideDomain, otherRootKey); // Outside domain
        ctx.SetupDocumentAncestors(secondMatchUnderDomain, domainRootKey); // Inside domain
        ctx.SetupContentItem(secondMatchUnderDomain, 3001);

        var domain = new DomainAndUri(new Domain(1, "example.com", domainRootId, null, false, 1), new Uri("http://example.com"));
        var requestBuilder = ctx.CreateRequestBuilder("http://example.com/shared-alias", domain);

        // Act
        var result = await ctx.CreateContentFinder().TryFindContent(requestBuilder);

        // Assert - should skip first match and return second match (under domain)
        Assert.That(result, Is.True);
        Assert.That(requestBuilder.PublishedContent!.Id, Is.EqualTo(3001));
        ctx.PublishedContentCache.Verify(x => x.GetById(firstMatchOutsideDomain), Times.Never);
        ctx.PublishedContentCache.Verify(x => x.GetById(secondMatchUnderDomain), Times.Once);
    }
}
