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

    private IContent _testPage;

    private const string Url = "RedirectUrl";

    public override void CreateTestData()
    {
        base.CreateTestData();

        using var scope = ScopeProvider.CreateScope();
        var repository = CreateRedirectUrlRepository();
        var rootContent = ContentService.GetRootContent().First();
        var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _).ToList();
        _testPage = subPages[0];

        repository.Save(new RedirectUrl { ContentKey = _testPage.Key, Url = Url, Culture = "en" });

        scope.Complete();
    }

    [Test]
    public void Can_Store_Old_Route()
    {
        Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_testPage.Id, "en")] = (_testPage.Key, "/old-route"),
            };

        var redirectTracker = CreateRedirectTracker();

        redirectTracker.StoreOldRoute(_testPage, dict);

        Assert.That(dict.Count, Is.EqualTo(1));
        Assert.AreEqual(dict.Values.First().OldRoute, Url);
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

        Assert.IsTrue(redirects.Any(x => x.Url == "/old-route"));
    }

    [Test]
    public void Removes_Self_Referncing_Redirects()
    {
        const string newUrl = "newUrl";

        var redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);
        Assert.IsTrue(redirects.Any(x => x.Url == Url)); // Ensure self referencing redirect exists.

        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_testPage.Id, "en")] = (_testPage.Key, newUrl),
            };

        var redirectTracker = CreateRedirectTracker();
        redirectTracker.CreateRedirects(dict);
        redirects = RedirectUrlService.GetContentRedirectUrls(_testPage.Key);

        Assert.IsFalse(redirects.Any(x => x.Url == Url));
        Assert.IsTrue(redirects.Any(x => x.Url == newUrl));
    }

    private RedirectUrlRepository CreateRedirectUrlRepository() =>
        new(
            (IScopeAccessor)ScopeProvider,
            AppCaches.Disabled,
            new NullLogger<RedirectUrlRepository>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());

    private IRedirectTracker CreateRedirectTracker()
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            { "en", new PublishedCultureInfo("en", "english", "/en/", DateTime.UtcNow) },
        };

        var content = new Mock<IPublishedContent>();

        content.SetupGet(c => c.Key).Returns(_testPage.Key);
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);
        content.SetupGet(c => c.Cultures).Returns(cultures);
        content.SetupGet(c => c.Id).Returns(_testPage.Id);

        IPublishedContentCache contentCache = Mock.Of<IPublishedContentCache>();
        Mock.Get(contentCache)
            .Setup(x => x.GetById(_testPage.Id))
            .Returns(content.Object);

        IPublishedUrlProvider publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        Mock.Get(publishedUrlProvider)
            .Setup(x => x.GetUrl(_testPage.Key, UrlMode.Relative, "en", null))
            .Returns(Url);

        Mock.Get(publishedUrlProvider)
            .Setup(x => x.GetUrl(_testPage.Id, UrlMode.Relative, "en", null))
            .Returns(Url);

        return new RedirectTracker(
            GetRequiredService<ILanguageService>(),
            RedirectUrlService,
            contentCache,
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<ILogger<RedirectTracker>>(),
            publishedUrlProvider,
            GetRequiredService<IPublishedContentStatusFilteringService>());
    }
}
