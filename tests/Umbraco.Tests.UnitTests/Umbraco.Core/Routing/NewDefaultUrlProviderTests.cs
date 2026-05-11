using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class NewDefaultUrlProviderTests
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

        public Mock<IIdKeyMap> IdKeyMap { get; } = new();

        public Mock<IDocumentUrlService> DocumentUrlService { get; } = new();

        public Mock<IDocumentNavigationQueryService> NavigationQueryService { get; } = new();

        public Mock<IPublishedContentStatusFilteringService> StatusFilteringService { get; } = new();

        public Mock<ILanguageService> LanguageService { get; } = new();

        public RequestHandlerSettings RequestConfig { get; set; } = new() { AddTrailingSlash = true };

        public string DefaultCulture { get; set; } = "en-US";

        public TestContext()
        {
            UmbracoContextAccessor
                .Setup(x => x.TryGetUmbracoContext(out It.Ref<IUmbracoContext?>.IsAny))
                .Callback(new TryGetUmbracoContextDelegate((out IUmbracoContext? ctx) => ctx = UmbracoContext.Object))
                .Returns(true);
            UmbracoContext.Setup(x => x.Content).Returns(PublishedContentCache.Object);
            UmbracoContext.Setup(x => x.Domains).Returns(DomainCache.Object);

            DomainCache.Setup(x => x.GetAssigned(It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(Enumerable.Empty<Domain>());

            LanguageService.Setup(x => x.GetDefaultIsoCodeAsync())
                .ReturnsAsync(() => DefaultCulture);
        }

        public NewDefaultUrlProvider CreateProvider()
        {
            var hostingEnv = new Mock<IHostingEnvironment>();
            hostingEnv.Setup(x => x.ApplicationVirtualPath).Returns("/");
            var uriUtility = new UriUtility(hostingEnv.Object);

            var optionsMonitor = new Mock<IOptionsMonitor<RequestHandlerSettings>>();
            optionsMonitor.Setup(x => x.CurrentValue).Returns(RequestConfig);

            return new NewDefaultUrlProvider(
                optionsMonitor.Object,
                Mock.Of<ILogger<NewDefaultUrlProvider>>(),
                SiteDomainMapper.Object,
                UmbracoContextAccessor.Object,
                uriUtility,
                PublishedContentCache.Object,
                DomainCache.Object,
                IdKeyMap.Object,
                DocumentUrlService.Object,
                NavigationQueryService.Object,
                StatusFilteringService.Object,
                LanguageService.Object);
        }

        public void SetupDomainForNode(int nodeId, DomainAndUri domainAndUri)
        {
            var domain = new Domain(1, domainAndUri.Name, nodeId, domainAndUri.Culture, false, 0);
            DomainCache.Setup(x => x.GetAssigned(nodeId, It.IsAny<bool>()))
                .Returns(new[] { domain });

            SiteDomainMapper.Setup(x => x.MapDomain(
                    It.IsAny<IReadOnlyCollection<DomainAndUri>>(),
                    It.IsAny<Uri>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>()))
                .Returns(domainAndUri);
        }
    }

    /// <summary>
    /// Verifies that a null route returns null (unpublished content).
    /// </summary>
    [Test]
    public void Can_Return_Null_For_Null_Route()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute(null, 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that an empty string route returns null (unpublished content).
    /// </summary>
    [Test]
    public void Can_Return_Null_For_Empty_Route()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute(string.Empty, 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that a whitespace-only route returns null (unpublished content).
    /// </summary>
    [Test]
    public void Can_Return_Null_For_Whitespace_Route()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("   ", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that the "#" route marker returns null (unpublished content).
    /// </summary>
    [Test]
    public void Can_Return_Null_For_Hash_Route()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("#", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that a root path "/" produces a relative URL of "/".
    /// </summary>
    [Test]
    public void Can_Get_Relative_Url_For_Root_Path()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/"));
    }

    /// <summary>
    /// Verifies that a simple path-only route produces a relative URL with trailing slash.
    /// </summary>
    [Test]
    public void Can_Get_Relative_Url_For_Simple_Path()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/my-page/"));
    }

    /// <summary>
    /// Verifies that a nested path route produces the correct relative URL.
    /// </summary>
    [Test]
    public void Can_Get_Relative_Url_For_Nested_Path()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/parent/child/grandchild", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/parent/child/grandchild/"));
    }

    /// <summary>
    /// Verifies that UrlMode.Absolute produces an absolute URL including the authority.
    /// </summary>
    [Test]
    public void Can_Get_Absolute_Url_For_Path_When_Mode_Is_Absolute()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Absolute, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://localhost/my-page/"));
    }

    /// <summary>
    /// Verifies that a route with a domain root ID prefix resolves to the domain-based URL.
    /// </summary>
    [Test]
    public void Can_Get_Url_For_Route_With_Domain_Root_Id()
    {
        var ctx = new TestContext();
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 100, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(100, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("100/my-page", 123, _currentUri, UrlMode.Absolute, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://example.com/my-page/"));
    }

    /// <summary>
    /// Verifies that a route with domain root ID and root path "/" produces the domain root URL.
    /// </summary>
    [Test]
    public void Can_Get_Url_For_Route_With_Domain_Root_Id_And_Root_Path()
    {
        var ctx = new TestContext();
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 100, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(100, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("100/", 123, _currentUri, UrlMode.Absolute, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://example.com/"));
    }

    /// <summary>
    /// Regression test for issue #22308: a route of just "1234" (domain root ID without slash separator)
    /// occurs when the original path was "/" and got trimmed. Previously threw ArgumentOutOfRangeException
    /// and crashed the Redirect URL Management dashboard.
    /// </summary>
    [Test]
    public void Can_Handle_Route_Without_Slash_Separator()
    {
        var ctx = new TestContext();
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 1234, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(1234, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("1234", 123, _currentUri, UrlMode.Absolute, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://example.com/"));
    }

    /// <summary>
    /// Verifies that a numeric route without slash does not throw when no domain is configured,
    /// falling back to a relative root URL.
    /// </summary>
    [Test]
    public void Can_Handle_Route_Without_Slash_When_No_Domain_Configured()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("9999", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/"));
    }

    /// <summary>
    /// Verifies that a non-numeric route without slash (malformed data) does not throw.
    /// The int.TryParse fails gracefully, and domainUri stays null.
    /// </summary>
    [Test]
    public void Can_Handle_Non_Numeric_Route_Without_Slash()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("notanumber", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/"));
    }

    /// <summary>
    /// Verifies that a non-default culture without a matching domain returns null,
    /// because only the default culture is served without a domain.
    /// </summary>
    [Test]
    public void Can_Return_Null_When_Culture_Does_Not_Match_Default_And_No_Domain()
    {
        var ctx = new TestContext { DefaultCulture = "en-US" };
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, "fr-FR");

        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Verifies that the default culture produces a URL even without a domain.
    /// </summary>
    [Test]
    public void Can_Get_Url_When_Culture_Matches_Default()
    {
        var ctx = new TestContext { DefaultCulture = "en-US" };
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/my-page/"));
    }

    /// <summary>
    /// Verifies that culture matching against the default is case-insensitive.
    /// </summary>
    [Test]
    public void Can_Get_Url_When_Culture_Matches_Default_Case_Insensitive()
    {
        var ctx = new TestContext { DefaultCulture = "en-US" };
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, "EN-us");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/my-page/"));
    }

    /// <summary>
    /// Verifies that a null culture (invariant content) always produces a URL.
    /// </summary>
    [Test]
    public void Can_Get_Url_When_Culture_Is_Null()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that an empty culture string (invariant content) always produces a URL.
    /// </summary>
    [Test]
    public void Can_Get_Url_When_Culture_Is_Empty()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, string.Empty);

        Assert.That(result, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that a non-default culture with a matching domain produces the correct domain-based URL.
    /// </summary>
    [Test]
    public void Can_Get_Url_When_Non_Default_Culture_Has_Domain()
    {
        var ctx = new TestContext { DefaultCulture = "en-US" };
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://fr.example.com", 100, "fr-FR", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(100, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("100/ma-page", 123, _currentUri, UrlMode.Absolute, "fr-FR");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://fr.example.com/ma-page/"));
        Assert.That(result.Culture, Is.EqualTo("fr-FR"));
    }

    /// <summary>
    /// Verifies that Auto mode produces a relative URL when the domain authority matches the current request.
    /// </summary>
    [Test]
    public void Can_Get_Relative_Url_When_Domain_Matches_Current_Request()
    {
        var ctx = new TestContext();
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://localhost", 100, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(100, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("100/my-page", 123, _currentUri, UrlMode.Auto, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/my-page/"));
    }

    /// <summary>
    /// Verifies that Auto mode produces an absolute URL when the domain differs from the current request.
    /// </summary>
    [Test]
    public void Can_Get_Absolute_Url_When_Domain_Differs_From_Current_Request()
    {
        var ctx = new TestContext();
        var domainAndUri = new DomainAndUri(
            new Domain(1, "http://example.com", 100, "en-US", false, 0),
            _currentUri);
        ctx.SetupDomainForNode(100, domainAndUri);

        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("100/my-page", 123, _currentUri, UrlMode.Auto, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("http://example.com/my-page/"));
    }

    /// <summary>
    /// Verifies that the trailing slash is omitted when AddTrailingSlash is disabled.
    /// </summary>
    [Test]
    public void Cannot_Get_Trailing_Slash_When_Setting_Disabled()
    {
        var ctx = new TestContext
        {
            RequestConfig = new RequestHandlerSettings { AddTrailingSlash = false },
        };
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/my-page", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Url!.ToString(), Is.EqualTo("/my-page"));
    }

    /// <summary>
    /// Verifies that the UrlInfo Provider property is set to the content URL provider alias.
    /// </summary>
    [Test]
    public void Can_Return_Correct_Provider_On_UrlInfo()
    {
        var ctx = new TestContext();
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/test", 123, _currentUri, UrlMode.Auto, null);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Provider, Is.EqualTo("umbDocumentUrlProvider"));
    }

    /// <summary>
    /// Verifies that the culture is correctly propagated to the UrlInfo result.
    /// </summary>
    [Test]
    public void Can_Return_Culture_On_UrlInfo()
    {
        var ctx = new TestContext { DefaultCulture = "en-US" };
        var provider = ctx.CreateProvider();

        var result = provider.GetUrlFromRoute("/test", 123, _currentUri, UrlMode.Auto, "en-US");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Culture, Is.EqualTo("en-US"));
    }
}
