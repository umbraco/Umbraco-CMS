using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class PublishedUrlInfoProviderTests
{
    private static readonly Guid _contentKey = new("2b3f9a4d-0c1e-4b6a-9c2d-1e2f3a4b5c6d");
    private static readonly Guid _parentKey = new("7a1c2e3d-4f5a-4b6c-8d9e-0f1a2b3c4d5e");
    private static readonly Guid _collidingKey = new("9e8d7c6b-5a4f-4e3d-9c2b-1a0f9e8d7c6b");
    private static readonly Guid _collidingRootKey = new("0f1e2d3c-4b5a-4968-8776-655443322110");
    private const int ContentId = 1234;

    private static readonly string[] _installedCultures = ["en-US", "da-DK"];

    private Mock<IPublishedUrlProvider> _urlProvider = null!;
    private Mock<ILanguageService> _languageService = null!;
    private Mock<IPublishedRouter> _router = null!;
    private Mock<ILocalizedTextService> _textService = null!;
    private Mock<IDocumentNavigationQueryService> _navigationQueryService = null!;
    private Mock<IPublishedContentCache> _publishedContentCache = null!;
    private Mock<IContentService> _contentService = null!;
    private FakePublishStatusQueryService _publishStatus = null!;
    private List<Domain> _domains = null!;
    private IPublishedRequest _routeResult = null!;

    [SetUp]
    public void SetUp()
    {
        _urlProvider = new Mock<IPublishedUrlProvider>();
        _urlProvider
            .Setup(x => x.GetUrl(It.IsAny<Guid>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((Guid _, UrlMode _, string? culture, Uri? _) => $"https://example.com/{culture}/");
        _urlProvider.Setup(x => x.GetOtherUrls(It.IsAny<int>())).Returns(Array.Empty<UrlInfo>());

        _languageService = new Mock<ILanguageService>();
        _languageService.Setup(x => x.GetAllAsync()).ReturnsAsync(_installedCultures.Select(x => CreateLanguage(x)).ToArray());
        _languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("en-US");
        _languageService.Setup(x => x.GetAsync("da-DK")).ReturnsAsync(CreateLanguage("da-DK", "Danish"));
        _languageService.Setup(x => x.GetAsync("en-US")).ReturnsAsync(CreateLanguage("en-US", "English"));

        // Default routing result: the generated URL routes back to the same content (no collision).
        _routeResult = CreateRequest(ContentId);
        _router = new Mock<IPublishedRouter>();
        _router.Setup(x => x.CreateRequestAsync(It.IsAny<Uri>())).ReturnsAsync(Mock.Of<IPublishedRequestBuilder>());
        _router
            .Setup(x => x.RouteRequestAsync(It.IsAny<IPublishedRequestBuilder>(), It.IsAny<RouteRequestOptions>()))
            .ReturnsAsync(() => _routeResult);

        // Echo the alias, and the first token if any, so the assertions can see which text was chosen.
        _textService = new Mock<ILocalizedTextService>();
        _textService
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns((string? _, string? alias, CultureInfo? _, IDictionary<string, string?>? tokens)
                => tokens is { Count: > 0 } ? $"{alias}:{tokens["0"]}" : alias ?? string.Empty);

        _navigationQueryService = new Mock<IDocumentNavigationQueryService>();
        IEnumerable<Guid> noAncestors = [];
        _navigationQueryService.Setup(x => x.TryGetAncestorsKeys(It.IsAny<Guid>(), out noAncestors)).Returns(false);

        _publishedContentCache = new Mock<IPublishedContentCache>();
        _contentService = new Mock<IContentService>();
        _publishStatus = new FakePublishStatusQueryService();
        _domains = [];
    }

    [Test]
    public async Task Can_Get_Url_For_Each_Installed_Culture_When_No_Culture_Requested()
    {
        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true));

        Assert.AreEqual(2, result.Count);
        CollectionAssert.AreEquivalent(_installedCultures, result.Select(x => x.Culture));
        Assert.IsTrue(result.All(x => x.Url is not null));
    }

    [Test]
    public async Task Can_Restrict_Urls_To_A_Single_Requested_Culture()
    {
        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("da-DK", result.Single().Culture);
    }

    [Test]
    public async Task Can_Match_Requested_Culture_Case_Insensitively()
    {
        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "DA-dk");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("da-DK", result.Single().Culture, "The installed culture's casing should be used.");
    }

    [Test]
    public async Task Cannot_Get_Urls_For_An_Unknown_Culture()
    {
        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "xx-XX");

        Assert.IsEmpty(result);
        _urlProvider.Verify(
            x => x.GetUrl(It.IsAny<Guid>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()),
            Times.Never);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Url_Provider_Throws_And_Reports_Message()
    {
        SetupUnroutable("da-DK", Constants.Routing.UrlProviderException);

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.IsNull(info.Url);
        Assert.AreEqual("getUrlException", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_For_Non_Default_Culture_Without_Hostname_And_Reports_Language()
    {
        SetupUnroutable("da-DK");

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.IsNull(info.Url);
        Assert.AreEqual("routeErrorNoDomainForCulture:Danish", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_For_Culture_That_Is_Not_Published_And_Reports_Message()
    {
        SetupUnroutable("da-DK");

        UrlInfo info = await GetSingleAsync("da-DK", CreateContent(variesByCulture: true, publishedCultures: ["en-US"]));

        Assert.AreEqual("itemNotPublished", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Ancestor_Is_Unpublished_And_Reports_Ancestor_Name()
    {
        SetupUnroutable("da-DK");
        SetupAncestors(_contentKey, _parentKey);
        SetupContentName(_parentKey, "Parent Page");
        _publishStatus.Unpublished.Add(_parentKey);

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.AreEqual("parentNotPublished:Parent Page", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Ancestor_Is_Unpublished_In_Culture_And_Reports_Ancestor_Name_In_Culture()
    {
        SetupUnroutable("da-DK");
        SetupAncestors(_contentKey, _parentKey);
        SetupContentName(_parentKey, "Parent Page", ("da-DK", "Forældreside"));
        _publishStatus.UnpublishedCultures.Add((_parentKey, "da-DK"));

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.AreEqual("parentCultureNotPublished:Forældreside", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_For_Unexplained_Missing_Url_And_Reports_Anomaly()
    {
        SetupUnroutable("en-US");

        UrlInfo info = await GetSingleAsync("en-US");

        Assert.AreEqual("parentNotPublishedAnomaly", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_For_Non_Default_Culture_With_Hostname_And_Reports_Anomaly()
    {
        SetupUnroutable("da-DK");
        _domains.Add(new Domain(1, "example.dk", ContentId, "da-DK", false, 0));

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.AreEqual("parentNotPublishedAnomaly", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Route_Collides_And_Reports_Colliding_Content_Path()
    {
        // The generated URL routes back to a different content item, which sits under a root node.
        _routeResult = CreateRequest(resolvedContentId: 9999, CreatePublishedContent(_collidingKey, "Colliding"));
        SetupAncestors(_collidingKey, _collidingRootKey);
        _publishedContentCache.Setup(x => x.GetById(_collidingRootKey)).Returns(CreatePublishedContent(_collidingRootKey, "Root"));

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.IsNull(info.Url);
        Assert.AreEqual("routeError:Root > Colliding", info.Message);
    }

    [Test]
    public async Task Can_Report_Colliding_Content_Path_In_The_Requested_Culture()
    {
        _routeResult = CreateRequest(
            resolvedContentId: 9999,
            CreatePublishedContent(_collidingKey, "Colliding", ("en-US", "Colliding"), ("da-DK", "Kolliderende")));

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.AreEqual("routeError:Kolliderende", info.Message);
    }

    [Test]
    public async Task Can_Fall_Back_To_Default_Name_When_Colliding_Content_Has_No_Name_In_The_Requested_Culture()
    {
        _routeResult = CreateRequest(
            resolvedContentId: 9999,
            CreatePublishedContent(_collidingKey, "Colliding", ("en-US", "Colliding")));

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.AreEqual("routeError:Colliding", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Route_Is_Unresolvable_And_Reports_Message()
    {
        // The generated URL does not route back to any content item.
        _routeResult = CreateRequest(resolvedContentId: null);

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.IsNull(info.Url);
        Assert.AreEqual("routeErrorCannotRoute", info.Message);
    }

    [Test]
    public async Task Can_Get_Url_When_Collisions_Are_Ignored()
    {
        // Routes back to different content, but the request opts out of collision checks.
        _routeResult = CreateRequest(resolvedContentId: 9999, ignoreCollisions: true);

        UrlInfo info = await GetSingleAsync("da-DK");

        Assert.IsNotNull(info.Url);
        Assert.AreEqual("da-DK", info.Culture);
    }

    [Test]
    public async Task Cannot_Get_Other_Urls_For_Trashed_Content()
    {
        _urlProvider
            .Setup(x => x.GetOtherUrls(ContentId))
            .Returns(new[] { UrlInfo.AsUrl("https://example.com/other/", "content", "da-DK") });

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true, trashed: true), "da-DK");

        _urlProvider.Verify(x => x.GetOtherUrls(It.IsAny<int>()), Times.Never);
        Assert.IsFalse(result.Any(x => x.Url is not null && x.Url.ToString().Contains("/other/")));
    }

    [Test]
    public async Task Can_Get_Other_Urls_Filtered_To_The_Scoped_Culture()
    {
        _urlProvider.Setup(x => x.GetOtherUrls(ContentId)).Returns(new[]
        {
            UrlInfo.AsUrl("https://example.com/other-da/", "content", "da-DK"),
            UrlInfo.AsUrl("https://example.com/other-en/", "content", "en-US"),
        });

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        Assert.Multiple(() =>
        {
            Assert.IsTrue(result.Any(x => x.Url is not null && x.Url.ToString().Contains("/other-da/")));
            Assert.IsFalse(result.Any(x => x.Url is not null && x.Url.ToString().Contains("/other-en/")));
        });
    }

    private async Task<UrlInfo> GetSingleAsync(string culture, IContent? content = null)
        => (await CreateSut().GetAllAsync(content ?? CreateContent(variesByCulture: true), culture)).Single();

    private void SetupUnroutable(string culture, string result = Constants.Routing.Unroutable)
        => _urlProvider
            .Setup(x => x.GetUrl(It.IsAny<Guid>(), It.IsAny<UrlMode>(), culture, It.IsAny<Uri?>()))
            .Returns(result);

    private void SetupAncestors(Guid key, params Guid[] ancestorKeys)
    {
        IEnumerable<Guid> ancestors = ancestorKeys;
        _navigationQueryService.Setup(x => x.TryGetAncestorsKeys(key, out ancestors)).Returns(true);
    }

    private void SetupContentName(Guid key, string name, params (string Culture, string Name)[] cultureNames)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.SetupGet(x => x.Variations).Returns(cultureNames.Length > 0 ? ContentVariation.Culture : ContentVariation.Nothing);

        var content = new Mock<IContent>();
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        content.SetupGet(x => x.Name).Returns(name);
        foreach ((string culture, string cultureName) in cultureNames)
        {
            content.Setup(x => x.GetCultureName(culture)).Returns(cultureName);
        }

        _contentService.Setup(x => x.GetById(key)).Returns(content.Object);
    }

    private PublishedUrlInfoProvider CreateSut()
    {
        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.ApplicationVirtualPath).Returns("/");

        var domainCache = new Mock<IDomainCache>();
        domainCache.Setup(x => x.GetAll(It.IsAny<bool>())).Returns(() => _domains);

        var umbracoContext = new Mock<IUmbracoContext>();
        umbracoContext.SetupGet(x => x.Domains).Returns(domainCache.Object);
        IUmbracoContext umbracoContextInstance = umbracoContext.Object;

        var umbracoContextAccessor = new Mock<IUmbracoContextAccessor>();
        umbracoContextAccessor.Setup(x => x.TryGetUmbracoContext(out umbracoContextInstance)).Returns(true);

        return new PublishedUrlInfoProvider(
            _urlProvider.Object,
            _languageService.Object,
            _router.Object,
            umbracoContextAccessor.Object,
            _textService.Object,
            NullLogger<PublishedUrlInfoProvider>.Instance,
            new UriUtility(hostingEnvironment.Object),
            Mock.Of<IVariationContextAccessor>(),
            _navigationQueryService.Object,
            _publishStatus,
            _publishedContentCache.Object,
            _contentService.Object);
    }

    private static ILanguage CreateLanguage(string isoCode, string? cultureName = null)
    {
        var language = new Mock<ILanguage>();
        language.SetupGet(x => x.IsoCode).Returns(isoCode);
        language.SetupGet(x => x.CultureName).Returns(cultureName ?? isoCode);
        return language.Object;
    }

    private static IContent CreateContent(bool variesByCulture, bool trashed = false, string[]? publishedCultures = null)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.SetupGet(x => x.Variations).Returns(variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing);

        var content = new Mock<IContent>();
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        content.SetupGet(x => x.Key).Returns(_contentKey);
        content.SetupGet(x => x.Id).Returns(ContentId);
        content.SetupGet(x => x.Path).Returns($"-1,{ContentId}");
        content.SetupGet(x => x.Trashed).Returns(trashed);
        content
            .Setup(x => x.IsCulturePublished(It.IsAny<string>()))
            .Returns((string culture) => publishedCultures is null || publishedCultures.Contains(culture, StringComparer.InvariantCultureIgnoreCase));
        return content.Object;
    }

    private static IPublishedContent CreatePublishedContent(Guid key, string name, params (string Culture, string Name)[] cultureNames)
        => CreatePublishedContent(9999, key, name, cultureNames);

    private static IPublishedContent CreatePublishedContent(int id, Guid key, string name, params (string Culture, string Name)[] cultureNames)
    {
        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(x => x.Variations).Returns(cultureNames.Length > 0 ? ContentVariation.Culture : ContentVariation.Nothing);

        var cultures = cultureNames.Length > 0
            ? cultureNames.ToDictionary(x => x.Culture, x => new PublishedCultureInfo(x.Culture, x.Name, null, DateTime.UtcNow))
            : new Dictionary<string, PublishedCultureInfo> { [string.Empty] = new(string.Empty, name, null, DateTime.UtcNow) };

        var content = new Mock<IPublishedContent>();
        content.SetupGet(x => x.Id).Returns(id);
        content.SetupGet(x => x.Key).Returns(key);
        content.SetupGet(x => x.Name).Returns(name);
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        content.SetupGet(x => x.Cultures).Returns(cultures);
        return content.Object;
    }

    private static IPublishedRequest CreateRequest(int? resolvedContentId, IPublishedContent? resolvedContent = null, bool ignoreCollisions = false)
    {
        var request = new Mock<IPublishedRequest>();
        if (resolvedContentId is not null)
        {
            request.SetupGet(x => x.PublishedContent).Returns(resolvedContent ?? CreatePublishedContent(resolvedContentId.Value, Guid.NewGuid(), "Resolved"));
        }

        request.SetupGet(x => x.IgnorePublishedContentCollisions).Returns(ignoreCollisions);
        return request.Object;
    }

    private sealed class FakePublishStatusQueryService : IPublishStatusQueryService
    {
        public HashSet<Guid> Unpublished { get; } = [];

        public HashSet<(Guid Key, string Culture)> UnpublishedCultures { get; } = [];

        public bool IsDocumentPublished(Guid documentKey, string culture)
            => Unpublished.Contains(documentKey) is false && UnpublishedCultures.Contains((documentKey, culture)) is false;

        public bool IsDocumentPublishedInAnyCulture(Guid documentKey) => Unpublished.Contains(documentKey) is false;

        public bool HasPublishedAncestorPath(Guid documentKey) => true;
    }
}
