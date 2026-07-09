using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class AliasUrlProviderTests
{
    private static readonly Uri _currentUri = new("http://localhost", UriKind.Absolute);

    private delegate void TryGetUmbracoContextDelegate(out IUmbracoContext? umbracoContext);

    private sealed class TestContext
    {
        public Mock<IUmbracoContextAccessor> UmbracoContextAccessor { get; } = new();

        public Mock<IUmbracoContext> UmbracoContext { get; } = new();

        public Mock<IPublishedContentCache> PublishedContentCache { get; } = new();

        public Mock<IDomainCache> DomainCache { get; } = new();

        public Mock<ISiteDomainMapper> SiteDomainMapper { get; } = new();

        public Mock<IDocumentNavigationQueryService> NavigationQueryService { get; } = new();

        public Mock<IPublishedContentStatusFilteringService> StatusFilteringService { get; } = new();

        public Mock<IPublishedValueFallback> PublishedValueFallback { get; } = new();

        public RequestHandlerSettings RequestConfig { get; set; } = new() { AddTrailingSlash = true };

        public TestContext()
        {
            // Wire up UmbracoContext
            UmbracoContextAccessor
                .Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
                .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = UmbracoContext.Object))
                .Returns(true);
            UmbracoContext.Setup(x => x.Content).Returns(PublishedContentCache.Object);
            UmbracoContext.Setup(x => x.Domains).Returns(DomainCache.Object);

            // Default: no domains assigned to any node
            DomainCache.Setup(x => x.GetAssigned(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Enumerable.Empty<Domain>());

            // Default: parent walk stops at root (null parent key)
            Guid? nullKey = null;
            NavigationQueryService
                .Setup(x => x.TryGetParentKey(It.IsAny<Guid>(), out nullKey))
                .Returns(true);
        }

        public AliasUrlProvider CreateProvider()
        {
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.Setup(x => x.ApplicationVirtualPath).Returns("/");
            var uriUtility = new UriUtility(hostingEnv.Object);

            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(x => x.CurrentValue).Returns(RequestConfig);

            return new AliasUrlProvider(
                optionsMonitor.Object,
                SiteDomainMapper.Object,
                uriUtility,
                PublishedValueFallback.Object,
                UmbracoContextAccessor.Object,
                NavigationQueryService.Object,
                StatusFilteringService.Object);
        }

        public Mock<IPublishedContent> SetupNode(
            int nodeId,
            string? aliasValue,
            bool variesByCulture = false,
            bool hasProperty = true)
        {
            var node = new Mock<IPublishedContent>();
            node.Setup(x => x.Id).Returns(nodeId);
            node.Setup(x => x.Key).Returns(Guid.NewGuid());

            var contentType = new Mock<IPublishedContentType>();

            if (hasProperty)
            {
                var propertyType = new Mock<IPublishedPropertyType>();
                propertyType.Setup(x => x.Variations).Returns(
                    variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing);

                contentType.Setup(x => x.GetPropertyType(Constants.Conventions.Content.UrlAlias))
                    .Returns(propertyType.Object);

                var property = new Mock<IPublishedProperty>();
                property.Setup(x => x.PropertyType).Returns(propertyType.Object);

                if (aliasValue != null)
                {
                    property.Setup(x => x.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(true);
                    property.Setup(x => x.GetValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(aliasValue);
                }
                else
                {
                    property.Setup(x => x.HasValue(It.IsAny<string?>(), It.IsAny<string?>())).Returns(false);
                }

                node.Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias))
                    .Returns(property.Object);
            }
            else
            {
                contentType.Setup(x => x.GetPropertyType(Constants.Conventions.Content.UrlAlias))
                    .Returns((IPublishedPropertyType?)null);
            }

            node.Setup(x => x.ContentType).Returns(contentType.Object);

            PublishedContentCache.Setup(x => x.GetById(nodeId)).Returns(node.Object);

            return node;
        }

        /// <summary>
        /// Sets up a node with culture-varying alias values.
        /// </summary>
        public Mock<IPublishedContent> SetupVariantNode(
            int nodeId,
            Dictionary<string, string> cultureAliases,
            IReadOnlyDictionary<string, PublishedCultureInfo>? cultures = null)
        {
            var node = new Mock<IPublishedContent>();
            node.Setup(x => x.Id).Returns(nodeId);
            node.Setup(x => x.Key).Returns(Guid.NewGuid());

            var contentType = new Mock<IPublishedContentType>();
            var propertyType = new Mock<IPublishedPropertyType>();
            propertyType.Setup(x => x.Variations).Returns(ContentVariation.Culture);

            contentType.Setup(x => x.GetPropertyType(Constants.Conventions.Content.UrlAlias))
                .Returns(propertyType.Object);

            var property = new Mock<IPublishedProperty>();
            property.Setup(x => x.PropertyType).Returns(propertyType.Object);

            foreach (var kvp in cultureAliases)
            {
                property.Setup(x => x.HasValue(kvp.Key, null)).Returns(true);
                property.Setup(x => x.GetValue(kvp.Key, null)).Returns(kvp.Value);
            }

            node.Setup(x => x.GetProperty(Constants.Conventions.Content.UrlAlias))
                .Returns(property.Object);
            node.Setup(x => x.ContentType).Returns(contentType.Object);

            cultures ??= new Dictionary<string, PublishedCultureInfo>();
            node.Setup(x => x.Cultures).Returns(cultures);

            PublishedContentCache.Setup(x => x.GetById(nodeId)).Returns(node.Object);

            return node;
        }

        public void SetupDomainsForNode(int nodeId, params DomainAndUri[] domainAndUris)
        {
            // Return domains from GetAssigned so DomainsForNode doesn't return null
            var domains = domainAndUris.Select((d, i) =>
                new Domain(i + 1, d.Name, nodeId, d.Culture, false, i)).ToArray();
            DomainCache.Setup(x => x.GetAssigned(nodeId, false)).Returns(domains);

            // MapDomains returns the filtered set
            SiteDomainMapper.Setup(x => x.MapDomains(
                    It.IsAny<IReadOnlyCollection<DomainAndUri>>(),
                    It.IsAny<Uri>(),
                    It.IsAny<bool>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns(domainAndUris);
        }
    }

    [Test]
    public void ReturnsEmpty_WhenNodeNotFound()
    {
        var ctx = new TestContext();
        ctx.PublishedContentCache.Setup(x => x.GetById(It.IsAny<int>())).Returns((IPublishedContent?)null);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReturnsEmpty_WhenNodeHasNoUrlAliasProperty()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: null, hasProperty: false);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReturnsEmpty_WhenPropertyVariesByCulture_AndNoDomains()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "my-alias", variesByCulture: true);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void ReturnsEmpty_WhenAliasValueIsNull()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: null);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void NoDomain_ReturnsUrl_ForRelativeAlias()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "my-alias");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/my-alias/"));
        Assert.That(result[0].Culture, Is.Null);
    }

    [Test]
    public void NoDomain_ReturnsUrl_ForAliasWithLeadingSlash()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "/my-alias");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/my-alias/"));
    }

    [Test]
    public void NoDomain_ReturnsUrl_ForAbsolutePathAlias()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "/some/deep/path");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/some/deep/path/"));
    }

    [Test]
    public void NoDomain_ReturnsMultipleUrls_ForCommaSeparatedAliases()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "alias1,alias2");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/alias1/"));
        Assert.That(result[1].Url!.ToString(), Is.EqualTo("/alias2/"));
    }

    [Test]
    public void NoDomain_DeduplicatesAliases()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "same,same");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/same/"));
    }

    [Test]
    public void NoDomain_NoTrailingSlash_WhenSettingDisabled()
    {
        var ctx = new TestContext
        {
            RequestConfig = new RequestHandlerSettings { AddTrailingSlash = false },
        };
        ctx.SetupNode(123, aliasValue: "my-alias");

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("/my-alias"));
    }

    [Test]
    public void WithDomain_ReturnsUrl_ForRelativeAlias()
    {
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "my-alias");

        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 123, null, false, 0),
            _currentUri);
        ctx.SetupDomainsForNode(123, domainAndUri);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("http://example.com/my-alias/"));
    }

    [Test]
    public void WithDomain_ReturnsUrl_ForAliasWithLeadingSlash()
    {
        // Regression test for PR #22068: "/my-alias" with domain should produce
        // "http://example.com/my-alias/", NOT "http://example.com//my-alias/"
        var ctx = new TestContext();
        ctx.SetupNode(123, aliasValue: "/my-alias");

        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 123, null, false, 0),
            _currentUri);
        ctx.SetupDomainsForNode(123, domainAndUri);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("http://example.com/my-alias/"));
    }

    [Test]
    public void WithDomain_SkipsCultureNotPublished()
    {
        var ctx = new TestContext();
        var cultureAliases = new Dictionary<string, string> { { "en-US", "english-alias" } };

        // Node has culture "en-US" but NOT "fr-FR"
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            { "en-US", new PublishedCultureInfo("en-US", "English", "/", DateTime.Now) },
        };
        ctx.SetupVariantNode(123, cultureAliases, cultures);

        // Domain for fr-FR culture
        var frDomain = new DomainAndUri(
            new Domain(1, "http://fr.example.com", 123, "fr-FR", false, 0),
            _currentUri);
        ctx.SetupDomainsForNode(123, frDomain);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        // Should be empty because the node doesn't have the fr-FR culture
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void WithDomain_ReturnsCultureOnUrlInfo()
    {
        var ctx = new TestContext();
        var cultureAliases = new Dictionary<string, string> { { "en-US", "english-alias" } };
        var cultures = new Dictionary<string, PublishedCultureInfo>
        {
            { "en-US", new PublishedCultureInfo("en-US", "English", "/", DateTime.Now) },
        };
        ctx.SetupVariantNode(123, cultureAliases, cultures);

        var enDomain = new DomainAndUri(
            new Domain(1, "http://en.example.com", 123, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainsForNode(123, enDomain);

        var result = ctx.CreateProvider().GetOtherUrls(123, _currentUri).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Url!.ToString(), Is.EqualTo("http://en.example.com/english-alias/"));
        Assert.That(result[0].Culture, Is.EqualTo("en-US"));
    }
}
