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

    [Test]
    public void Can_Store_Old_Route()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker();

        redirectTracker.StoreOldRoute(_testPage, dict);

        Assert.AreEqual(1, dict.Count);
        var key = dict.Keys.First();
        Assert.AreEqual(_testPage.Key, dict[key].ContentKey);
        Assert.AreEqual("/new-route", dict[key].OldRoute);
    }

    [Test]
    public void Can_Store_Old_Route_With_Domain_Root_Prefix()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict = [];

        var redirectTracker = CreateRedirectTracker(assignDomain: true);

        redirectTracker.StoreOldRoute(_testPage, dict);

        Assert.AreEqual(1, dict.Count);
        var key = dict.Keys.First();
        Assert.AreEqual(_testPage.Key, dict[key].ContentKey);
        Assert.AreEqual($"{_rootPage.Id}/new-route", dict[key].OldRoute);
    }

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

    private RedirectUrlRepository CreateRedirectUrlRepository() =>
        new(
            (IScopeAccessor)ScopeProvider,
            AppCaches.Disabled,
            new NullLogger<RedirectUrlRepository>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    private IRedirectTracker CreateRedirectTracker(bool assignDomain = false)
    {
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
            .Returns("/new-route");

        IDocumentNavigationQueryService documentNavigationQueryService = Mock.Of<IDocumentNavigationQueryService>();
        IEnumerable<Guid> ancestorKeys = [_rootPage.Key];
        Mock.Get(documentNavigationQueryService)
            .Setup(x => x.TryGetAncestorsKeys(_testPage.Key, out ancestorKeys))
            .Returns(true);

        IPublishedContentStatusFilteringService publishedContentStatusFilteringService = Mock.Of<IPublishedContentStatusFilteringService>();
        Mock.Get(publishedContentStatusFilteringService)
            .Setup(x => x.FilterAvailable(It.IsAny<IEnumerable<Guid>>(), It.IsAny<string?>()))
            .Returns([rootContent.Object]);

        IDomainCache domainCache = Mock.Of<IDomainCache>();
        Mock.Get(domainCache)
            .Setup(x => x.HasAssigned(_testPage.Id, It.IsAny<bool>()))
            .Returns(false);
        Mock.Get(domainCache)
            .Setup(x => x.HasAssigned(_rootPage.Id, It.IsAny<bool>()))
            .Returns(assignDomain);

        return new RedirectTracker(
            GetRequiredService<ILanguageService>(),
            RedirectUrlService,
            contentCache,
            documentNavigationQueryService,
            GetRequiredService<ILogger<RedirectTracker>>(),
            publishedUrlProvider,
            publishedContentStatusFilteringService,
            domainCache);
    }

    private void CreateExistingRedirect()
    {
        using var scope = ScopeProvider.CreateScope();
        var repository = CreateRedirectUrlRepository();
        repository.Save(new RedirectUrl { ContentKey = _testPage.Key, Url = "/new-route", Culture = "en" });
        scope.Complete();
    }
}
