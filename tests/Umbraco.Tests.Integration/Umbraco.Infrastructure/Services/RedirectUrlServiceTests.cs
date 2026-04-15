// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence.EFCore;
using Umbraco.Cms.Infrastructure.Persistence.EFCore.Scoping;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;
using Umbraco.Cms.Tests.Common.Attributes;
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

    public override async Task CreateTestDataAsync()
    {
        await base.CreateTestDataAsync();

        var provider = NewScopeProvider;
        using (var scope = provider.CreateScope())
        {
            var repository = new RedirectUrlRepository(
                GetRequiredService<IEFCoreScopeAccessor<UmbracoDbContext>>(),
                AppCaches.Disabled,
                LoggerFactory.CreateLogger<RedirectUrlRepository>(),
                Mock.Of<IRepositoryCacheVersionService>(),
                Mock.Of<ICacheSyncService>());
            var rootContent = ContentService.GetRootContent().First();
            var subPages = ContentService.GetPagedChildren(rootContent.Id, 0, 3, out _, propertyAliases: null, filter: null, ordering: null).ToList();
            _firstSubPage = subPages[0];
            _secondSubPage = subPages[1];
            _thirdSubPage = subPages[2];


            await repository.SaveAsync(new RedirectUrl { ContentKey = _firstSubPage.Key, Url = Url, Culture = CultureEnglish }, CancellationToken.None);
            Thread.Sleep(
                1000); //Added delay to ensure timestamp difference as sometimes they seem to have the same timestamp
            await repository.SaveAsync(new RedirectUrl { ContentKey = _secondSubPage.Key, Url = Url, Culture = CultureGerman }, CancellationToken.None);
            Thread.Sleep(1000);
            await repository.SaveAsync(new RedirectUrl { ContentKey = _thirdSubPage.Key, Url = UrlAlt, Culture = string.Empty }, CancellationToken.None);

            scope.Complete();
        }
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Most_Recent_RedirectUrl()
    {
        var redirect = await RedirectUrlService.GetMostRecentRedirectUrlAsync(Url);
        Assert.AreEqual(redirect.ContentId, _secondSubPage.Id);
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Most_Recent_RedirectUrl_With_Culture()
    {
        var redirect = await RedirectUrlService.GetMostRecentRedirectUrlAsync(Url, CultureEnglish);
        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }

    [Test]
    [LongRunning]
    public async Task Can_Get_Most_Recent_RedirectUrl_With_Culture_When_No_CultureVariant_Exists()
    {
        var redirect = await RedirectUrlService.GetMostRecentRedirectUrlAsync(UrlAlt, UnusedCulture);
        Assert.AreEqual(redirect.ContentId, _thirdSubPage.Id);
    }

    [Test]
    public async Task Can_Register_Redirect()
    {
        const string TestUrl = "testUrl";

        await RedirectUrlService.RegisterAsync(TestUrl, _firstSubPage.Key);

        var redirect = await RedirectUrlService.GetMostRecentRedirectUrlAsync(TestUrl, CultureEnglish);

        Assert.AreEqual(redirect.ContentId, _firstSubPage.Id);
    }
}
