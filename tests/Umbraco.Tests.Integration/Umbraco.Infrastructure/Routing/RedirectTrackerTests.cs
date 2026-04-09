using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Routing;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Routing;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class RedirectTrackerTests : UmbracoIntegrationTestWithContent
{
    private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

    private IContent _rootPage;
    private IContent _testPage;

    public override void CreateTestData()
    {
        base.CreateTestData();

        var rootContent = ContentService.GetRootContent().First();
        _rootPage = rootContent;
        var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _, propertyAliases: null, filter: null, ordering: null).ToList();
        _testPage = subPages[0];
    }

    /// <summary>
    /// Verifies that <see cref="IRedirectTracker.StoreOldRoute"/> stores the correct content key
    /// and relative route for a content item without domain assignment.
    /// </summary>
    [Test]
    public void Can_Store_Old_Route()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker();

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: true);

        Assert.AreEqual(1, dict.Count);
        var key = dict.Keys.First();
        Assert.AreEqual(_testPage.Key, dict[key].ContentKey);
        Assert.AreEqual("/new-route", dict[key].OldRoute);
    }

    /// <summary>
    /// Verifies that when a domain is assigned to the root node, the stored route is prefixed
    /// with the root node ID.
    /// </summary>
    [Test]
    public void Can_Store_Old_Route_With_Domain_Root_Prefix()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions { AssignDomain = true });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: true);

        Assert.AreEqual(1, dict.Count);
        var key = dict.Keys.First();
        Assert.AreEqual(_testPage.Key, dict[key].ContentKey);
        Assert.AreEqual($"{_rootPage.Id}/new-route", dict[key].OldRoute);
    }

    /// <summary>
    /// Verifies that <see cref="IRedirectTracker.CreateRedirects"/> registers a redirect URL
    /// when the old route differs from the new route.
    /// </summary>
    [Test]
    public void Can_Create_Redirects()
    {
        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_testPage.Id, "en")] = (_testPage.Key, "/old-route"),
            };
        var redirectTracker = CreateRedirectTracker();

        redirectTracker.CreateRedirects(dict);

        var redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);
        Assert.AreEqual(1, redirects.Count());
        var redirect = redirects.First();
        Assert.AreEqual("/old-route", redirect.Url);
    }

    /// <summary>
    /// Verifies that creating a redirect removes any existing redirect whose URL matches the
    /// content's current route, preventing self-referencing redirects.
    /// </summary>
    [Test]
    public void Will_Remove_Self_Referencing_Redirects()
    {
        CreateExistingRedirect();

        var redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);
        Assert.IsTrue(redirects.Any(x => x.Url == "/new-route")); // Ensure self referencing redirect exists.

        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_testPage.Id, "en")] = (_testPage.Key, "/old-route"),
            };

        var redirectTracker = CreateRedirectTracker();
        redirectTracker.CreateRedirects(dict);

        redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);
        Assert.AreEqual(1, redirects.Count());
        var redirect = redirects.First();
        Assert.AreEqual("/old-route", redirect.Url);
    }

    /// <summary>
    /// Verifies that when a domain includes a path prefix (e.g. "example.com/en/"), the stored
    /// route strips that prefix to avoid duplicating the culture segment.
    /// </summary>
    [Test]
    public void Can_Store_Old_Route_With_Domain_Path_Does_Not_Duplicate_Segment()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        // Domain configured as "example.com/en/" — GetUrl returns "/en/new-route".
        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            AssignDomain = true, DomainName = "example.com/en/", RelativeUrl = "/en/new-route",
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: true);

        Assert.AreEqual(1, dict.Count);
        var key = dict.Keys.First();
        Assert.AreEqual(_testPage.Key, dict[key].ContentKey);

        // The stored route should strip the domain path "/en" so the result is "{rootId}/new-route", NOT "{rootId}/en/new-route".
        Assert.AreEqual($"{_rootPage.Id}/new-route", dict[key].OldRoute);
    }

    /// <summary>
    /// Verifies that no redirect is created when the old route matches the new route after
    /// correctly stripping the domain path prefix.
    /// </summary>
    [Test]
    public void Create_Redirects_With_Domain_Path_Skips_When_Route_Unchanged()
    {
        // oldRoute matches the correctly computed newRoute (domain path stripped).
        // Without the fix, newRoute would be "{rootId}/en/new-route" (duplicated), causing a
        // spurious redirect even though the URL hasn't actually changed.
        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_testPage.Id, "en")] = (_testPage.Key, $"{_rootPage.Id}/new-route"),
            };

        // Domain configured as "example.com/en/" — GetUrl returns "/en/new-route".
        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            AssignDomain = true, DomainName = "example.com/en/", RelativeUrl = "/en/new-route",
        });

        redirectTracker.CreateRedirects(dict);

        var redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);
        Assert.AreEqual(0, redirects.Count());
    }

    /// <summary>
    /// Verifies that publishing content with an unchanged URL segment skips descendant traversal
    /// entirely, storing no routes.
    /// </summary>
    [Test]
    public void Publish_With_Unchanged_Segment_Skips_Descendants()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "test-page",
            NewSegment = "test-page",
            DocumentUrlServiceInitialized = true,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: false);

        Assert.AreEqual(0, dict.Count);
    }

    /// <summary>
    /// Verifies that publishing content with a changed URL segment triggers full descendant
    /// traversal, storing routes for the entity and its descendants.
    /// </summary>
    [Test]
    public void Publish_With_Changed_Segment_Traverses_Descendants()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "old-name",
            NewSegment = "new-name",
            DocumentUrlServiceInitialized = true,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: false);

        Assert.IsTrue(dict.Count > 0);
    }

    /// <summary>
    /// Verifies that move operations always traverse descendants even when the URL segment
    /// is unchanged, since the parent path has changed.
    /// </summary>
    [Test]
    public void Move_Always_Traverses_Descendants_Regardless_Of_Segment()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "test-page",
            NewSegment = "test-page",
            DocumentUrlServiceInitialized = true,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: true);

        Assert.IsTrue(dict.Count > 0);
    }

    /// <summary>
    /// Verifies that when a parent and child are both processed in the same batch, the child's
    /// URL is only resolved once (during the parent's traversal) and skipped on the second call.
    /// </summary>
    [Test]
    public void Batch_Deduplication_Skips_Already_Processed_Descendants()
    {
        var childKey = Guid.NewGuid();
        const int childId = 99999;

        var childEntity = new Mock<IContent>();
        childEntity.SetupGet(c => c.Id).Returns(childId);
        childEntity.SetupGet(c => c.Key).Returns(childKey);
        childEntity.SetupGet(c => c.Name).Returns("Child Page");

        var getUrlForChildCallCount = 0;
        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            ChildKey = childKey,
            ChildId = childId,
            OnGetUrlForChild = () => getUrlForChildCallCount++,
        });

        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        // Store routes for parent (traverses child), then for child.
        redirectTracker.StoreOldRoute(_testPage, dict, isMove: true);
        redirectTracker.StoreOldRoute(childEntity.Object, dict, isMove: true);

        // GetUrl for the child should only be called once (during parent's traversal).
        // The second StoreOldRoute skips because the child is already in oldRoutes.
        Assert.AreEqual(1, getUrlForChildCallCount);
        Assert.IsTrue(dict.ContainsKey((_testPage.Id, "en")));
        Assert.IsTrue(dict.ContainsKey((childId, "en")));
    }

    /// <summary>
    /// Verifies that when <see cref="IDocumentUrlService"/> is not yet initialized (e.g. during
    /// upgrades), the segment optimization is bypassed and full descendant traversal occurs.
    /// </summary>
    [Test]
    public void Fallback_To_Full_Traversal_When_DocumentUrlService_Not_Initialized()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "test-page",
            NewSegment = "test-page",
            DocumentUrlServiceInitialized = false,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: false);

        Assert.IsTrue(dict.Count > 0);
    }

    /// <summary>
    /// Verifies that a custom <see cref="IUrlSegmentProvider"/> can override
    /// <see cref="IUrlSegmentProvider.HasUrlSegmentChanged"/> to force descendant traversal
    /// even when the default segment comparison would detect no change.
    /// </summary>
    [Test]
    public void Custom_UrlSegmentProvider_Override_Of_HasUrlSegmentChanged()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "test-page",
            NewSegment = "test-page",
            HasUrlSegmentChangedOverride = true,
            DocumentUrlServiceInitialized = true,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: false);

        Assert.IsTrue(dict.Count > 0);
    }

    /// <summary>
    /// Verifies that when an <see cref="IUrlSegmentProvider"/> reports that changes to the
    /// published content may affect descendant URL segments (via <see cref="IUrlSegmentProvider.MayAffectDescendantSegments"/>),
    /// descendant traversal occurs even though the content's own URL segment is unchanged.
    /// This supports custom providers that derive descendant segments from ancestor properties.
    /// </summary>
    [Test]
    public void Provider_Affecting_Descendants_Forces_Traversal_Despite_Unchanged_Segment()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(new RedirectTrackerSetupOptions
        {
            IncludeChild = true,
            CurrentPublishedSegment = "test-page",
            NewSegment = "test-page",
            DocumentUrlServiceInitialized = true,
            MayAffectDescendantSegments = true,
        });

        redirectTracker.StoreOldRoute(_testPage, dict, isMove: false);

        Assert.IsTrue(dict.Count > 0);
    }

    private RedirectUrlRepository CreateRedirectUrlRepository() =>
        new(
            (IScopeAccessor)ScopeProvider,
            AppCaches.Disabled,
            new NullLogger<RedirectUrlRepository>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    /// <summary>
    /// Creates and configures an instance of an object that tracks redirects for published content, using the specified
    /// setup options.
    /// </summary>
    /// <param name="options">Configuration options for the redirect tracker. If null, default options are used.</param>
    private IRedirectTracker CreateRedirectTracker(RedirectTrackerSetupOptions? options = null)
    {
        options ??= new RedirectTrackerSetupOptions();
        var resolvedChildKey = options.ChildKey ?? Guid.NewGuid();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            { "en", new PublishedCultureInfo("en", "english", "/en/", DateTime.UtcNow) },
        };

        var rootContent = new Mock<IPublishedContent>();
        rootContent.SetupGet(c => c.Id).Returns(_rootPage.Id);
        rootContent.SetupGet(c => c.Key).Returns(_rootPage.Key);
        rootContent.SetupGet(c => c.Name).Returns(_rootPage.Name);
        rootContent.SetupGet(c => c.Path).Returns(_rootPage.Path);

        var content = new Mock<IPublishedContent>();
        content.SetupGet(c => c.Id).Returns(_testPage.Id);
        content.SetupGet(c => c.Key).Returns(_testPage.Key);
        content.SetupGet(c => c.Name).Returns(_testPage.Name);
        content.SetupGet(c => c.Path).Returns(_testPage.Path);
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);
        content.SetupGet(c => c.Cultures).Returns(cultures);

        IPublishedContentCache contentCache = Mock.Of<IPublishedContentCache>();
        Mock.Get(contentCache)
            .Setup(x => x.GetById(_testPage.Id))
            .Returns(content.Object);
        Mock.Get(contentCache)
            .Setup(x => x.GetById(_testPage.Key))
            .Returns(content.Object);

        IPublishedUrlProvider publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        Mock.Get(publishedUrlProvider)
            .Setup(x => x.GetUrl(_testPage.Key, UrlMode.Relative, "en", null))
            .Returns(options.RelativeUrl ?? "/new-route");

        IDocumentNavigationQueryService documentNavigationQueryService = Mock.Of<IDocumentNavigationQueryService>();
        IEnumerable<Guid> ancestorKeys = [_rootPage.Key];
        Mock.Get(documentNavigationQueryService)
            .Setup(x => x.TryGetAncestorsKeys(_testPage.Key, out ancestorKeys))
            .Returns(true);

        IPublishedContentStatusFilteringService publishedContentStatusFilteringService = Mock.Of<IPublishedContentStatusFilteringService>();
        Mock.Get(publishedContentStatusFilteringService)
            .Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
            .Returns([rootContent.Object]);

        // Set up child content if requested.
        if (options.IncludeChild)
        {
            var childPublishedContent = new Mock<IPublishedContent>();
            childPublishedContent.SetupGet(c => c.Id).Returns(options.ChildId);
            childPublishedContent.SetupGet(c => c.Key).Returns(resolvedChildKey);
            childPublishedContent.SetupGet(c => c.Name).Returns("Child Page");
            childPublishedContent.SetupGet(c => c.Path).Returns($"{_rootPage.Path},{_testPage.Id},{options.ChildId}");
            childPublishedContent.SetupGet(c => c.ContentType).Returns(contentType.Object);
            childPublishedContent.SetupGet(c => c.Cultures).Returns(cultures);

            Mock.Get(contentCache)
                .Setup(x => x.GetById(resolvedChildKey))
                .Returns(childPublishedContent.Object);
            Mock.Get(contentCache)
                .Setup(x => x.GetById(options.ChildId))
                .Returns(childPublishedContent.Object);

            var getUrlSetup = Mock.Get(publishedUrlProvider)
                .Setup(x => x.GetUrl(resolvedChildKey, UrlMode.Relative, "en", null))
                .Returns("/new-route/child-page");
            if (options.OnGetUrlForChild is not null)
            {
                getUrlSetup.Callback(options.OnGetUrlForChild);
            }

            IEnumerable<Guid> descendantKeys = [resolvedChildKey];
            Mock.Get(documentNavigationQueryService)
                .Setup(x => x.TryGetDescendantsKeys(_testPage.Key, out descendantKeys))
                .Returns(true);

            IEnumerable<Guid> emptyKeys = [];
            Mock.Get(documentNavigationQueryService)
                .Setup(x => x.TryGetDescendantsKeys(resolvedChildKey, out emptyKeys))
                .Returns(true);

            IEnumerable<Guid> childAncestorKeys = [_rootPage.Key, _testPage.Key];
            Mock.Get(documentNavigationQueryService)
                .Setup(x => x.TryGetAncestorsKeys(resolvedChildKey, out childAncestorKeys))
                .Returns(true);

            Mock.Get(publishedContentStatusFilteringService)
                .Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
                .Returns((IEnumerable<Guid> keys, string? culture) =>
                {
                    var result = new List<IPublishedContent>();
                    foreach (var k in keys)
                    {
                        if (k == _rootPage.Key)
                        {
                            result.Add(rootContent.Object);
                        }
                        else if (k == resolvedChildKey)
                        {
                            result.Add(childPublishedContent.Object);
                        }
                    }

                    return result;
                });
        }

        // Domain setup.
        IDomainCache domainCache = Mock.Of<IDomainCache>();
        Mock.Get(domainCache)
            .Setup(x => x.HasAssigned(_testPage.Id, It.IsAny<bool>()))
            .Returns(false);
        Mock.Get(domainCache)
            .Setup(x => x.HasAssigned(_rootPage.Id, It.IsAny<bool>()))
            .Returns(options.AssignDomain);

        if (options.IncludeChild)
        {
            Mock.Get(domainCache)
                .Setup(x => x.HasAssigned(options.ChildId, It.IsAny<bool>()))
                .Returns(false);
        }

        if (options.AssignDomain)
        {
            var effectiveDomainName = options.DomainName ?? "example.com";
            var domains = new[] { new Domain(1, effectiveDomainName, _rootPage.Id, "en", false, 0) };
            Mock.Get(domainCache)
                .Setup(x => x.GetAssigned(_rootPage.Id, false))
                .Returns(domains);
        }

        // URL segment provider setup.
        UrlSegmentProviderCollection urlSegmentProviders;
        if (options.NewSegment is not null)
        {
            var urlSegmentProvider = new Mock<IUrlSegmentProvider>();
            urlSegmentProvider
                .Setup(x => x.GetUrlSegment(It.IsAny<IContentBase>(), false, It.IsAny<string?>()))
                .Returns(options.NewSegment);

            if (options.HasUrlSegmentChangedOverride.HasValue)
            {
                urlSegmentProvider
                    .Setup(x => x.HasUrlSegmentChanged(It.IsAny<IContentBase>(), It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns(options.HasUrlSegmentChangedOverride.Value);
            }
            else
            {
                var capturedNewSegment = options.NewSegment;
                urlSegmentProvider
                    .Setup(x => x.HasUrlSegmentChanged(It.IsAny<IContentBase>(), It.IsAny<string?>(), It.IsAny<string?>()))
                    .Returns((IContentBase _, string? currentSeg, string? _) =>
                        !string.Equals(capturedNewSegment, currentSeg, StringComparison.OrdinalIgnoreCase));
            }

            urlSegmentProvider.SetupGet(x => x.AllowAdditionalSegments).Returns(false);
            urlSegmentProvider
                .Setup(x => x.MayAffectDescendantSegments(It.IsAny<IContentBase>()))
                .Returns(options.MayAffectDescendantSegments);
            urlSegmentProviders = new UrlSegmentProviderCollection(() => [urlSegmentProvider.Object]);
        }
        else
        {
            urlSegmentProviders = new UrlSegmentProviderCollection(() => []);
        }

        // Document URL service setup.
        var documentUrlService = new Mock<IDocumentUrlService>();
        documentUrlService.SetupGet(x => x.IsInitialized).Returns(options.DocumentUrlServiceInitialized);
        if (options.CurrentPublishedSegment is not null)
        {
            documentUrlService
                .Setup(x => x.GetUrlSegment(_testPage.Key, It.IsAny<string>(), false))
                .Returns(options.CurrentPublishedSegment);
        }

        return new RedirectTracker(
            GetRequiredService<ILanguageService>(),
            RedirectUrlService,
            contentCache,
            documentNavigationQueryService,
            GetRequiredService<ILogger<RedirectTracker>>(),
            publishedUrlProvider,
            publishedContentStatusFilteringService,
            domainCache,
            urlSegmentProviders,
            documentUrlService.Object);
    }

    private void CreateExistingRedirect()
    {
        using var scope = ScopeProvider.CreateScope();
        var repository = CreateRedirectUrlRepository();
        repository.Save(new RedirectUrl { ContentKey = _testPage.Key, Url = "/new-route", Culture = "en" });
        scope.Complete();
    }

    /// <summary>
    /// Configuration options for <see cref="CreateRedirectTracker"/>, controlling which mock
    /// dependencies are set up and how they behave.
    /// </summary>
    private class RedirectTrackerSetupOptions
    {
        /// <summary>
        /// Gets a value indicating whether a domain should be assigned to the root content node.
        /// When <c>true</c>, the stored route is prefixed with the root node ID.
        /// </summary>
        public bool AssignDomain { get; init; }

        /// <summary>
        /// Gets the domain name to assign (e.g. "example.com" or "example.com/en/").
        /// Only used when <see cref="AssignDomain"/> is <c>true</c>. Defaults to "example.com".
        /// </summary>
        public string? DomainName { get; init; }

        /// <summary>
        /// Gets the relative URL returned by <see cref="IPublishedUrlProvider.GetUrl"/> for the test page.
        /// Defaults to "/new-route".
        /// </summary>
        public string? RelativeUrl { get; init; }

        /// <summary>
        /// Gets a value indicating whether a child published content node should be added as a
        /// descendant of the test page, enabling tests that verify descendant traversal behavior.
        /// </summary>
        public bool IncludeChild { get; init; }

        /// <summary>
        /// Gets the key to use for the child content node. A random key is generated if not specified.
        /// Only used when <see cref="IncludeChild"/> is <c>true</c>.
        /// </summary>
        public Guid? ChildKey { get; init; }

        /// <summary>
        /// Gets the integer ID to use for the child content node.
        /// Only used when <see cref="IncludeChild"/> is <c>true</c>.
        /// </summary>
        public int ChildId { get; init; } = 99999;

        /// <summary>
        /// Gets a callback invoked each time <see cref="IPublishedUrlProvider.GetUrl"/> is called
        /// for the child node. Useful for tracking call counts in deduplication tests.
        /// Only used when <see cref="IncludeChild"/> is <c>true</c>.
        /// </summary>
        public Action? OnGetUrlForChild { get; init; }

        /// <summary>
        /// Gets the URL segment returned by <see cref="IDocumentUrlService.GetUrlSegment"/> for
        /// the test page (representing the currently published segment). When set together with
        /// <see cref="NewSegment"/>, enables segment change detection tests.
        /// </summary>
        public string? CurrentPublishedSegment { get; init; }

        /// <summary>
        /// Gets the URL segment returned by the mock <see cref="IUrlSegmentProvider"/> for the
        /// test page (representing the segment being published). Compared against
        /// <see cref="CurrentPublishedSegment"/> to determine if the segment has changed.
        /// </summary>
        public string? NewSegment { get; init; }

        /// <summary>
        /// Gets an explicit return value for <see cref="IUrlSegmentProvider.HasUrlSegmentChanged"/>,
        /// overriding the default segment comparison logic. When <c>null</c>, the mock provider
        /// compares <see cref="NewSegment"/> against the current segment case-insensitively.
        /// </summary>
        public bool? HasUrlSegmentChangedOverride { get; init; }

        /// <summary>
        /// Gets a value indicating whether the mock <see cref="IDocumentUrlService.IsInitialized"/>
        /// returns <c>true</c>. When <c>false</c>, the redirect tracker falls back to full
        /// descendant traversal regardless of segment changes.
        /// </summary>
        public bool DocumentUrlServiceInitialized { get; init; }

        /// <summary>
        /// Gets an explicit return value for <see cref="IUrlSegmentProvider.MayAffectDescendantSegments"/>.
        /// When <c>true</c>, the provider signals that changes to this content may affect descendant
        /// segments, forcing descendant traversal even if the content's own segment is unchanged.
        /// Defaults to <c>false</c>.
        /// </summary>
        public bool MayAffectDescendantSegments { get; init; }
    }
}
