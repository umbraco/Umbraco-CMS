using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Routing;

[TestFixture]
public class PublishedUrlInfoProviderTests
{
    private static readonly Guid _contentKey = new("2b3f9a4d-0c1e-4b6a-9c2d-1e2f3a4b5c6d");
    private const int ContentId = 1234;

    private static readonly string[] _installedCultures = ["en-US", "da-DK"];

    private Mock<IPublishedUrlProvider> _urlProvider = null!;
    private Mock<ILanguageService> _languageService = null!;
    private Mock<IPublishedRouter> _router = null!;
    private Mock<ILocalizedTextService> _textService = null!;
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
        _languageService.Setup(x => x.GetAllAsync()).ReturnsAsync(_installedCultures.Select(CreateLanguage).ToArray());
        _languageService.Setup(x => x.GetDefaultIsoCodeAsync()).ReturnsAsync("en-US");

        // Default routing result: the generated URL routes back to the same content (no collision).
        _routeResult = CreateRequest(ContentId);
        _router = new Mock<IPublishedRouter>();
        _router.Setup(x => x.CreateRequestAsync(It.IsAny<Uri>())).ReturnsAsync(Mock.Of<IPublishedRequestBuilder>());
        _router
            .Setup(x => x.RouteRequestAsync(It.IsAny<IPublishedRequestBuilder>(), It.IsAny<RouteRequestOptions>()))
            .ReturnsAsync(() => _routeResult);

        _textService = new Mock<ILocalizedTextService>();
        _textService
            .Setup(x => x.Localize(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CultureInfo?>(), It.IsAny<IDictionary<string, string?>?>()))
            .Returns((string? _, string? alias, CultureInfo? _, IDictionary<string, string?>? _) => alias ?? string.Empty);
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
    public async Task Cannot_Get_Url_For_Unroutable_Content_And_Reports_Message()
    {
        _urlProvider
            .Setup(x => x.GetUrl(It.IsAny<Guid>(), It.IsAny<UrlMode>(), "da-DK", It.IsAny<Uri?>()))
            .Returns(Constants.Routing.Unroutable);

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        UrlInfo info = result.Single();
        Assert.IsNull(info.Url);
        Assert.AreEqual("getUrlException", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Route_Collides_And_Reports_Message()
    {
        // The generated URL routes back to a different content item.
        _routeResult = CreateRequest(resolvedContentId: 9999);

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        UrlInfo info = result.Single();
        Assert.IsNull(info.Url);
        Assert.AreEqual("routeError", info.Message);
    }

    [Test]
    public async Task Cannot_Get_Url_When_Route_Is_Unresolvable_And_Reports_Message()
    {
        // The generated URL does not route back to any content item.
        _routeResult = CreateRequest(resolvedContentId: null);

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        UrlInfo info = result.Single();
        Assert.IsNull(info.Url);
        Assert.AreEqual("routeErrorCannotRoute", info.Message);
    }

    [Test]
    public async Task Can_Get_Url_When_Collisions_Are_Ignored()
    {
        // Routes back to different content, but the request opts out of collision checks.
        _routeResult = CreateRequest(resolvedContentId: 9999, ignoreCollisions: true);

        ISet<UrlInfo> result = await CreateSut().GetAllAsync(CreateContent(variesByCulture: true), "da-DK");

        UrlInfo info = result.Single();
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

    private PublishedUrlInfoProvider CreateSut()
    {
        var hostingEnvironment = new Mock<IHostingEnvironment>();
        hostingEnvironment.Setup(x => x.ApplicationVirtualPath).Returns("/");

        return new PublishedUrlInfoProvider(
            _urlProvider.Object,
            _languageService.Object,
            _router.Object,
            Mock.Of<IUmbracoContextAccessor>(),
            _textService.Object,
            NullLogger<PublishedUrlInfoProvider>.Instance,
            new UriUtility(hostingEnvironment.Object),
            Mock.Of<IVariationContextAccessor>());
    }

    private static ILanguage CreateLanguage(string isoCode)
    {
        var language = new Mock<ILanguage>();
        language.SetupGet(x => x.IsoCode).Returns(isoCode);
        return language.Object;
    }

    private static IContent CreateContent(bool variesByCulture, bool trashed = false)
    {
        var contentType = new Mock<ISimpleContentType>();
        contentType.SetupGet(x => x.Variations).Returns(variesByCulture ? ContentVariation.Culture : ContentVariation.Nothing);

        var content = new Mock<IContent>();
        content.SetupGet(x => x.ContentType).Returns(contentType.Object);
        content.SetupGet(x => x.Key).Returns(_contentKey);
        content.SetupGet(x => x.Id).Returns(ContentId);
        content.SetupGet(x => x.Trashed).Returns(trashed);
        return content.Object;
    }

    private static IPublishedRequest CreateRequest(int? resolvedContentId, bool ignoreCollisions = false)
    {
        var request = new Mock<IPublishedRequest>();
        if (resolvedContentId is not null)
        {
            var publishedContent = new Mock<IPublishedContent>();
            publishedContent.SetupGet(x => x.Id).Returns(resolvedContentId.Value);
            publishedContent.SetupGet(x => x.Key).Returns(Guid.NewGuid());
            request.SetupGet(x => x.PublishedContent).Returns(publishedContent.Object);
        }

        request.SetupGet(x => x.IgnorePublishedContentCollisions).Returns(ignoreCollisions);
        return request.Object;
    }
}
