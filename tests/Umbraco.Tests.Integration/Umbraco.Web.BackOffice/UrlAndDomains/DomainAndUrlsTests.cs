using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Tests.Common;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.UrlAndDomains;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, Mapper = true, WithApplication = true, Logger = UmbracoTestOptions.Logger.Console)]
public class DomainAndUrlsTests : UmbracoIntegrationTest
{
    [SetUp]
    public void Setup()
    {
        var xml = PackageMigrationResource.GetEmbeddedPackageDataManifest(GetType());
        var packagingService = GetRequiredService<IPackagingService>();
        InstallationSummary = packagingService.InstallCompiledPackageData(xml);

        Root = InstallationSummary.ContentInstalled.First();
        ContentService.SaveAndPublish(Root);

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
        builder.AddNuCache();
    }

    private readonly TestVariationContextAccessor _variationContextAccessor = new();

    public IContent Root { get; set; }
    public string[] Cultures { get; set; }


    [Test]
    public void Having_three_cultures_and_set_domain_on_all_of_them()
    {
        foreach (var culture in Cultures)
        {
            SetDomainOnContent(Root, culture, GetDomainUrlFromCultureCode(culture));
        }

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
    public void Having_three_cultures_but_set_domain_on_a_non_default_language()
    {
        var culture = Cultures[1];
        var domain = GetDomainUrlFromCultureCode(culture);
        SetDomainOnContent(Root, culture, domain);

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
            GetRequiredService<ILocalizationService>(),
            GetRequiredService<ILocalizedTextService>(),
            ContentService,
            GetRequiredService<IVariationContextAccessor>(),
            GetRequiredService<ILogger<IContent>>(),
            GetRequiredService<UriUtility>(),
            GetRequiredService<IPublishedUrlProvider>()).GetAwaiter().GetResult();
}
