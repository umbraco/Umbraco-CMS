using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Scoping;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.UrlAndDomains;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Mapper = true, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
internal sealed class DomainAndUrlsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        var xml = PackageMigrationResource.GetEmbeddedPackageDataManifest(GetType());
        var packagingService = GetRequiredService<IPackagingService>();
        InstallationSummary = packagingService.InstallCompiledPackageData(xml);

        Root = InstallationSummary.ContentInstalled.First();
        ContentService.Publish(Root, Root.AvailableCultures.ToArray());

        var cultures = new List<string>
        {
            GetRequiredService<ILocalizationService>().GetDefaultLanguageIsoCode()
        };

        foreach (var language in InstallationSummary.LanguagesInstalled)
        {
            cultures.Add(language.IsoCode);
        }

        Cultures = cultures.ToArray();

        var httpContextAccessor = GetRequiredService<IHttpContextAccessor>();

        httpContextAccessor.HttpContext = new DefaultHttpContext
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("localhost"),
                Path = "/",
                QueryString = new QueryString(string.Empty)
            }
        };

        //Like the request middleware we specify the VariationContext to the default language.
        _variationContextAccessor.VariationContext = new VariationContext(Cultures[0]);
        GetRequiredService<IUmbracoContextFactory>().EnsureUmbracoContext();
    }

    private IContentService ContentService => GetRequiredService<IContentService>();

    public InstallationSummary InstallationSummary { get; set; }

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.AddUnique<IVariationContextAccessor>(_variationContextAccessor);
        builder.AddUmbracoHybridCache();

        // Ensure cache refreshers runs
        builder.Services.AddUnique<IServerMessenger, ScopedRepositoryTests.LocalServerMessenger>();
        builder.AddNotificationHandler<ContentTreeChangeNotification, ContentTreeChangeDistributedCacheNotificationHandler>();
        builder.AddNotificationHandler<DomainSavedNotification, DomainSavedDistributedCacheNotificationHandler>();

    }

    private readonly TestVariationContextAccessor _variationContextAccessor = new();

    public IContent Root { get; set; }

    public string[] Cultures { get; set; }

    [Test]
    public async Task Can_Update_Domains_For_All_Cultures()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);

        void VerifyDomains(IDomain[] domains)
        {
            Assert.AreEqual(3, domains.Length);
            for (var i = 0; i < domains.Length; i++)
            {
                Assert.AreEqual(Cultures[i], domains[i].LanguageIsoCode);
                Assert.AreEqual(GetDomainUrlFromCultureCode(Cultures[i]), domains[i].DomainName);
            }
        }

        VerifyDomains(result.Result.Domains.ToArray());

        // re-get and verify again
        var domains = await domainService.GetAssignedDomainsAsync(Root.Key, true);
        VerifyDomains(domains.ToArray());
    }

    [Test]
    public async Task Can_Sort_Domains()
    {
        var domainService = GetRequiredService<IDomainService>();
        var reversedCultures = Cultures.Reverse().ToArray();
        var updateModel = new DomainsUpdateModel
        {
            Domains = reversedCultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);

        void VerifyDomains(IDomain[] domains)
        {
            Assert.AreEqual(3, domains.Length);
            for (var i = 0; i < domains.Length; i++)
            {
                Assert.AreEqual(reversedCultures[i], domains[i].LanguageIsoCode);
                Assert.AreEqual(GetDomainUrlFromCultureCode(reversedCultures[i]), domains[i].DomainName);
            }
        }

        VerifyDomains(result.Result.Domains.ToArray());

        // re-get and verify again
        var domains = await domainService.GetAssignedDomainsAsync(Root.Key, true);
        VerifyDomains(domains.ToArray());
    }

    [Test]
    public async Task Can_Remove_All_Domains()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);
        Assert.AreEqual(3, result.Result.Domains.Count());

        updateModel.Domains = Enumerable.Empty<DomainModel>();

        result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);
        Assert.AreEqual(0, result.Result.Domains.Count());

        // re-get and verify again
        var domains = await domainService.GetAssignedDomainsAsync(Root.Key, true);
        Assert.AreEqual(0, domains.Count());
    }

    [Test]
    public async Task Can_Remove_Single_Domain()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);
        Assert.AreEqual(3, result.Result.Domains.Count());

        updateModel.Domains = new[] { updateModel.Domains.First(), updateModel.Domains.Last() };

        result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(DomainOperationStatus.Success, result.Status);
        Assert.AreEqual(2, result.Result.Domains.Count());
        Assert.AreEqual(Cultures.First(), result.Result.Domains.First().LanguageIsoCode);
        Assert.AreEqual(Cultures.Last(), result.Result.Domains.Last().LanguageIsoCode);
    }

    [Test]
    public async Task Can_Resolve_Urls_With_Domains_For_All_Cultures()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);

        var rootUrls = GetContentUrlsAsync(Root).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(6, rootUrls.Count());
            foreach (var culture in Cultures)
            {
                var domain = GetDomainUrlFromCultureCode(culture);
                Assert.IsTrue(rootUrls.Any(x => x.Text == domain));
                Assert.IsTrue(rootUrls.Any(x => x.Text == "https://localhost" + domain));
            }
        });
    }

    [Test]
    public async Task Can_Resolve_Urls_For_Non_Default_Domain_Culture_Only()
    {
        var culture = Cultures[1];
        var domain = GetDomainUrlFromCultureCode(culture);
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = new[]
            {
                new DomainModel { DomainName = domain, IsoCode = culture }
            }
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);

        var rootUrls = GetContentUrlsAsync(Root).ToArray();

        Assert.Multiple(() =>
        {
            Assert.AreEqual(4, rootUrls.Count());

            //We expect two for the domain that is setup
            Assert.IsTrue(rootUrls.Any(x => x.IsUrl && x.Text == domain && x.Culture == culture));
            Assert.IsTrue(rootUrls.Any(x => x.IsUrl && x.Text == "https://localhost" + domain && x.Culture == culture));

            //We expect the default language to be routable on the default path "/"
            Assert.IsTrue(rootUrls.Any(x => x.IsUrl && x.Text == "/" && x.Culture == Cultures[0]));

            //We dont expect non-default languages without a domain to be routable
            Assert.IsTrue(rootUrls.Any(x => x.IsUrl == false && x.Culture == Cultures[2]));
        });
    }

    [Test]
    public async Task Can_Set_Default_Culture()
    {
        var domainService = GetRequiredService<IDomainService>();
        var culture = Cultures[1];
        var updateModel = new DomainsUpdateModel
        {
            DefaultIsoCode = culture,
            Domains = Enumerable.Empty<DomainModel>()
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.Result.Domains.Count());

        // default culture is represented as a wildcard domain
        var domain = result.Result.Domains.First();
        Assert.IsTrue(domain.IsWildcard);
        Assert.AreEqual(culture, domain.LanguageIsoCode);
        Assert.AreEqual("*" + Root.Id, domain.DomainName);
    }

    [Test]
    public void Can_Use_Obsolete_Save()
    {
        foreach (var culture in Cultures)
        {
            SetDomainOnContent(Root, culture, GetDomainUrlFromCultureCode(culture));
        }

        var domains = GetRequiredService<IDomainService>().GetAssignedDomains(Root.Id, true);
        Assert.AreEqual(3, domains.Count());
    }

    [Test]
    public void Can_Use_Obsolete_Delete()
    {
        foreach (var culture in Cultures)
        {
            SetDomainOnContent(Root, culture, GetDomainUrlFromCultureCode(culture));
        }

        var domainService = GetRequiredService<IDomainService>();

        var domains = domainService.GetAssignedDomains(Root.Id, true);
        Assert.AreEqual(3, domains.Count());

        var result = domainService.Delete(domains.First());
        Assert.IsTrue(result.Success);

        domains = domainService.GetAssignedDomains(Root.Id, true);
        Assert.AreEqual(2, domains.Count());
    }

    [TestCase("/domain")]
    [TestCase("/")]
    [TestCase("some.domain.com")]
    public async Task Cannot_Assign_Duplicate_Domains(string domainName)
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel { DomainName = domainName, IsoCode = culture }).ToArray()
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DomainOperationStatus.DuplicateDomainName, result.Status);
    }

    [TestCase("https://*.umbraco.com")]
    [TestCase("&#€%#€")]
    [TestCase("¢”$¢”¢$≈{")]
    public async Task Cannot_Assign_Invalid_Domains(string domainName)
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = new DomainModel { DomainName = domainName, IsoCode = Cultures.First() }.Yield()
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DomainOperationStatus.InvalidDomainName, result.Status);
    }

    [Test]
    public async Task Cannot_Assign_Already_Used_Domains()
    {
        var copy = ContentService.Copy(Root, Root.ParentId, false);
        ContentService.Publish(copy!, copy!.AvailableCultures.ToArray());

        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = Cultures.Select(culture => new DomainModel
            {
                DomainName = GetDomainUrlFromCultureCode(culture), IsoCode = culture
            })
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.IsTrue(result.Success);

        result = await domainService.UpdateDomainsAsync(copy.Key, updateModel);
        Assert.IsFalse(result.Success);
        Assert.AreEqual(DomainOperationStatus.ConflictingDomainName, result.Status);

        Assert.IsNotNull(result.Result.ConflictingDomains);
        Assert.IsNotEmpty(result.Result.ConflictingDomains);
        Assert.AreEqual(updateModel.Domains.Count(), result.Result.ConflictingDomains.Count());
        foreach (var culture in Cultures)
        {
            Assert.IsNotNull(result.Result.ConflictingDomains.SingleOrDefault(c => c.RootContentId == Root.Id && c.DomainName == GetDomainUrlFromCultureCode(culture)));
        }
    }

    private static string GetDomainUrlFromCultureCode(string culture) =>
        "/" + culture.Replace("-", string.Empty).ToLower() + "/";

    private void SetDomainOnContent(IContent content, string cultureIsoCode, string domain)
    {
        var domainService = GetRequiredService<IDomainService>();
        var langId = GetRequiredService<ILocalizationService>().GetLanguageIdByIsoCode(cultureIsoCode);
        domainService.Save(
            new UmbracoDomain(domain) { RootContentId = content.Id, LanguageId = langId });
    }

    private IEnumerable<UrlInfo> GetContentUrlsAsync(IContent root) =>
        root.GetContentUrlsAsync(
            GetRequiredService<IPublishedRouter>(),
            GetRequiredService<IUmbracoContextAccessor>().GetRequiredUmbracoContext(),
            GetRequiredService<ILanguageService>(),
            GetRequiredService<ILocalizedTextService>(),
            ContentService,
            GetRequiredService<IVariationContextAccessor>(),
            GetRequiredService<ILogger<IContent>>(),
            GetRequiredService<UriUtility>(),
            GetRequiredService<IPublishedUrlProvider>(),
            GetRequiredService<IDocumentNavigationQueryService>(),
            GetRequiredService<IPublishedContentStatusFilteringService>()).GetAwaiter().GetResult();
}
