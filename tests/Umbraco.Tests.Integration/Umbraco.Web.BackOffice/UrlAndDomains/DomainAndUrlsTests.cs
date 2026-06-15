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

        // Note: this SetUp must remain synchronous. EnsureUmbracoContext() below writes to an AsyncLocal
        // (via HybridUmbracoContextAccessor) and AsyncLocal mutations made inside an awaited Task do not
        // flow back to the test method's execution context.
        var cultures = new List<string>
        {
            GetRequiredService<ILanguageService>().GetDefaultIsoCodeAsync().GetAwaiter().GetResult(),
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));

        void VerifyDomains(IDomain[] domains)
        {
            Assert.That(domains, Has.Length.EqualTo(3));
            for (var i = 0; i < domains.Length; i++)
            {
                Assert.That(domains[i].LanguageIsoCode, Is.EqualTo(Cultures[i]));
                Assert.That(domains[i].DomainName, Is.EqualTo(GetDomainUrlFromCultureCode(Cultures[i])));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));

        void VerifyDomains(IDomain[] domains)
        {
            Assert.That(domains, Has.Length.EqualTo(3));
            for (var i = 0; i < domains.Length; i++)
            {
                Assert.That(domains[i].LanguageIsoCode, Is.EqualTo(reversedCultures[i]));
                Assert.That(domains[i].DomainName, Is.EqualTo(GetDomainUrlFromCultureCode(reversedCultures[i])));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));
        Assert.That(result.Result.Domains.Count(), Is.EqualTo(3));

        updateModel.Domains = Enumerable.Empty<DomainModel>();

        result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));
        Assert.That(result.Result.Domains.Count(), Is.EqualTo(0));

        // re-get and verify again
        var domains = await domainService.GetAssignedDomainsAsync(Root.Key, true);
        Assert.That(domains.Count(), Is.EqualTo(0));
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));
        Assert.That(result.Result.Domains.Count(), Is.EqualTo(3));

        updateModel.Domains = new[] { updateModel.Domains.First(), updateModel.Domains.Last() };

        result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.That(result.Success, Is.True);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.Success));
        Assert.That(result.Result.Domains.Count(), Is.EqualTo(2));
        Assert.That(result.Result.Domains.First().LanguageIsoCode, Is.EqualTo(Cultures.First()));
        Assert.That(result.Result.Domains.Last().LanguageIsoCode, Is.EqualTo(Cultures.Last()));
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
        Assert.That(result.Success, Is.True);

        var rootUrls = GetContentUrlsAsync(Root).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(rootUrls.Count(), Is.EqualTo(6));
            foreach (var culture in Cultures)
            {
                var domain = GetDomainUrlFromCultureCode(culture);
                Assert.That(rootUrls.Any(x => x.Url?.ToString() == domain && x.Message == null), Is.True);
                Assert.That(rootUrls.Any(x => x.Url?.ToString() == "https://localhost" + domain && x.Message == null), Is.True);
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
        Assert.That(result.Success, Is.True);

        var rootUrls = GetContentUrlsAsync(Root).ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(rootUrls.Count(), Is.EqualTo(4));

            //We expect two for the domain that is setup
            Assert.That(rootUrls.Any(x => x.Url?.ToString() == domain && x.Culture == culture && x.Message == null), Is.True);
            Assert.That(rootUrls.Any(x => x.Url?.ToString() == "https://localhost" + domain && x.Culture == culture && x.Message == null), Is.True);

            //We expect the default language to be routable on the default path "/"
            Assert.That(rootUrls.Any(x => x.Url?.ToString() == "/" && x.Culture == Cultures[0] && x.Message == null), Is.True);

            //We dont expect non-default languages without a domain to be routable
            Assert.That(rootUrls.Any(x => x.Url == null && x.Culture == Cultures[2]), Is.True);
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
        Assert.That(result.Success, Is.True);
        Assert.That(result.Result.Domains.Count(), Is.EqualTo(1));

        // default culture is represented as a wildcard domain
        var domain = result.Result.Domains.First();
        Assert.That(domain.IsWildcard, Is.True);
        Assert.That(domain.LanguageIsoCode, Is.EqualTo(culture));
        Assert.That(domain.DomainName, Is.EqualTo("*" + Root.Id));
    }

    [Test]
    public async Task Cannot_Update_Domains_For_Non_Existent_Content()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = new DomainModel { DomainName = "/domain", IsoCode = Cultures.First() }.Yield()
        };

        var result = await domainService.UpdateDomainsAsync(Guid.NewGuid(), updateModel);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.ContentNotFound));
    }

    [Test]
    public async Task Cannot_Update_Domains_With_Unknown_Iso_Code()
    {
        var domainService = GetRequiredService<IDomainService>();
        var updateModel = new DomainsUpdateModel
        {
            Domains = new DomainModel { DomainName = "/domain", IsoCode = "xx-XX" }.Yield()
        };

        var result = await domainService.UpdateDomainsAsync(Root.Key, updateModel);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.LanguageNotFound));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.DuplicateDomainName));
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
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.InvalidDomainName));
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
        Assert.That(result.Success, Is.True);

        result = await domainService.UpdateDomainsAsync(copy.Key, updateModel);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Status, Is.EqualTo(DomainOperationStatus.ConflictingDomainName));

        Assert.That(result.Result.ConflictingDomains, Is.Not.Null);
        Assert.That(result.Result.ConflictingDomains, Is.Not.Empty);
        Assert.That(result.Result.ConflictingDomains.Count(), Is.EqualTo(updateModel.Domains.Count()));
        foreach (var culture in Cultures)
        {
            Assert.That(result.Result.ConflictingDomains.SingleOrDefault(c => c.RootContentId == Root.Id && c.DomainName == GetDomainUrlFromCultureCode(culture)), Is.Not.Null);
        }
    }

    private static string GetDomainUrlFromCultureCode(string culture) =>
        "/" + culture.Replace("-", string.Empty).ToLower() + "/";

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
