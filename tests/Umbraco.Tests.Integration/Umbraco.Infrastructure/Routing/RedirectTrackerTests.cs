using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
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
    private IRedirectTracker RedirectTracker => GetRequiredService<IRedirectTracker>();

    private IRedirectUrlService redirectUrlService => GetRequiredService<IRedirectUrlService>();

    private IContent _subPage;

    private const string Url = "RedirectUrl";

    public override void CreateTestData()
    {
        base.CreateTestData();

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = MockRepository();
            var rootContent = ContentService.GetRootContent().First();
            var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _).ToList();
            _subPage = subPages[0];

            repository.Save(new RedirectUrl { ContentKey = _subPage.Key, Url = Url, Culture = "en" });

            scope.Complete();
        }
    }

    [Test]
    public void Can_Create_Redirects()
    {
        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_subPage.Id, "en")] = (_subPage.Key, "/old-route"),
            };
        var redirectTracker = mockRedirectTracker();

        redirectTracker.CreateRedirects(dict);

        var redirects = redirectUrlService.GetContentRedirectUrls(_subPage.Key);

        Assert.IsTrue(redirects.Any(x => x.Url == "/old-route"));
    }

    [Test]
    public void Removes_Self_Referncing_Redirects()
    {
        const string newUrl = "newUrl";

        var redirects = redirectUrlService.GetContentRedirectUrls(_subPage.Key);
        Assert.IsTrue(redirects.Any(x => x.Url == Url)); // Ensure self referencing redirect exists.

        IDictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)> dict =
            new Dictionary<(int ContentId, string Culture), (Guid ContentKey, string OldRoute)>
            {
                [(_subPage.Id, "en")] = (_subPage.Key, newUrl),
            };

        var redirectTracker = mockRedirectTracker();
        redirectTracker.CreateRedirects(dict);
        redirects = redirectUrlService.GetContentRedirectUrls(_subPage.Key);

        Assert.IsFalse(redirects.Any(x => x.Url == Url));
        Assert.IsTrue(redirects.Any(x => x.Url == newUrl));
    }

    private RedirectUrlRepository MockRepository()
    {
        return new RedirectUrlRepository(
            (IScopeAccessor)ScopeProvider,
            AppCaches.Disabled,
            Mock.Of<ILogger<RedirectUrlRepository>>(),
            Mock.Of<IRepositoryCacheVersionService>(),
            Mock.Of<ICacheSyncService>());
    }

    private IRedirectTracker mockRedirectTracker()
    {
        IPublishedUrlProvider publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        Mock.Get(publishedUrlProvider)
            .Setup(x => x.GetUrl(_subPage.Key, UrlMode.Relative, "en", null))
            .Returns(Url);

        return new RedirectTracker(
            GetRequiredService<ILocalizationService>(),
            redirectUrlService,
            GetRequiredService<IPublishedContentCache>(),
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<ILogger<RedirectTracker>>(),
            publishedUrlProvider,
            GetRequiredService<IPublishedContentStatusFilteringService>());
    }
}
