// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.HybridCache;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Attributes;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class RedirectUrlServiceTests : UmbracoIntegrationTestWithContent
{
    private IContent _firstSubPage;
    private IContent _secondSubPage;
    private IContent _thirdSubPage;
    private const string Url = "blah";
    private const string UrlAlt = "alternativeUrl";
    private const string CultureEnglish = "en";
    private const string CultureGerman = "de";
    private const string UnusedCulture = "es";

    private IRedirectUrlService RedirectUrlService => GetRequiredService<IRedirectUrlService>();

    private IPublishedUrlProvider PublishedUrlProvider => GetRequiredService<IPublishedUrlProvider>();

    public override void CreateTestData()
    {
        base.CreateTestData();

        using (var scope = ScopeProvider.CreateScope())
        {
            var repository = MockRepository();
            var rootContent = ContentService.GetRootContent().First();
            var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _).ToList();
            _firstSubPage = subPages[0];
            _secondSubPage = subPages[1];
            _thirdSubPage = subPages[2];


            repository.Save(new RedirectUrl { ContentKey = _firstSubPage.Key, Url = Url, Culture = CultureEnglish });
            Thread.Sleep(
                1000); //Added delay to ensure timestamp difference as sometimes they seem to have the same timestamp
            repository.Save(new RedirectUrl { ContentKey = _secondSubPage.Key, Url = Url, Culture = CultureGerman });
            Thread.Sleep(1000);
            repository.Save(new RedirectUrl { ContentKey = _thirdSubPage.Key, Url = UrlAlt, Culture = string.Empty });

            scope.Complete();
        }
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url);
        Assert.AreEqual(redirect.ContentId, _secondSubPage.Id);
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl_With_Culture()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(Url, CultureEnglish);
        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }

    [Test]
    [LongRunning]
    public void Can_Get_Most_Recent_RedirectUrl_With_Culture_When_No_CultureVariant_Exists()
    {
        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(UrlAlt, UnusedCulture);
        Assert.AreEqual(redirect.ContentId, _thirdSubPage.Id);
    }

    [Test]
    public void Can_Register_Redirect()
    {
        var umbracoContextFactory = GetRequiredService<IUmbracoContextFactory>();

        const string TestUrl = "testUrl";
        using (umbracoContextFactory.EnsureUmbracoContext())
        {
            RedirectUrlService.Register(TestUrl, _firstSubPage.Key);
        }

        var redirect = RedirectUrlService.GetMostRecentRedirectUrl(TestUrl, CultureEnglish);

        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }

    /// <summary>
    /// The service if there already exists any redirect URLs that point to the new URL. If thats the case we delete it.
    /// With this test, a self referencing redirect already exists through mocks, and we test to see if the Register method deletes it.
    /// </summary>
    [Test]
    public void Deletes_Self_Referencing_Redirects()
    {
        var service = mockServiceSetup();
        var redirects = RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key);
        var originalRedirect = RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key).First().Url;

        // Self referencing redirect exists already through mocks.
        Assert.True(redirects.Any(x => x.Url == originalRedirect));

        var umbracoContextFactory = GetRequiredService<IUmbracoContextFactory>();
        string redirectUrl = "newRedirectUrl";
        using (umbracoContextFactory.EnsureUmbracoContext())
        {
            service.Register(redirectUrl, _firstSubPage.Key);
        }

        redirects = RedirectUrlService.GetContentRedirectUrls(_firstSubPage.Key);

        // The self referencing redirect has been successfully deleted.
        Assert.False(redirects.Any(x => x.Url == originalRedirect));
    }

    private RedirectUrlService mockServiceSetup()
    {
        IPublishedUrlProvider publishedUrlProvider = Mock.Of<IPublishedUrlProvider>();
        Mock.Get(publishedUrlProvider)
            .Setup(x => x.GetUrl(_firstSubPage.Key, UrlMode.Relative, null, null))
            .Returns(Url);

        return new RedirectUrlService(
            ScopeProvider,
            Mock.Of<ILoggerFactory>(),
            Mock.Of<IEventMessagesFactory>(),
            MockRepository(),
            publishedUrlProvider);
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
}
