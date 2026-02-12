using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class PublishedContentCacheTests : DeliveryApiTests
{
    private readonly Guid _contentOneId = Guid.Parse("19AEAC73-DB4E-4CFC-AB06-0AD14A89A613");

    private readonly Guid _contentTwoId = Guid.Parse("4EF11E1E-FB50-4627-8A86-E10ED6F4DCE4");

    private readonly Guid _contentThreeId = Guid.Parse("013387EE-57AF-4ABD-B03C-F991B0722CCA");

    private readonly Guid _contentFourId = Guid.Parse("76D02780-F7D9-49E9-B5C3-3B992ED98F59");

    private IPublishedContentCache _contentCache;
    private IDocumentUrlService _documentUrlService;

    [SetUp]
    public override void Setup()
    {
        var contentTypeOneMock = new Mock<IPublishedContentType>();
        contentTypeOneMock.SetupGet(m => m.Alias).Returns("theContentType");
        var contentOneMock = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(contentOneMock, _contentOneId, "Content One", "content-one", contentTypeOneMock.Object, Array.Empty<IPublishedProperty>());

        var contentTypeTwoMock = new Mock<IPublishedContentType>();
        contentTypeTwoMock.SetupGet(m => m.Alias).Returns("theOtherContentType");
        var contentTwoMock = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(contentTwoMock, _contentTwoId, "Content Two", "content-two", contentTypeTwoMock.Object, Array.Empty<IPublishedProperty>());

        var contentTypeThreeMock = new Mock<IPublishedContentType>();
        contentTypeThreeMock.SetupGet(m => m.Alias).Returns("theThirdContentType");
        var contentThreeMock = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(contentThreeMock, _contentThreeId, "Content Three", "content-three", contentTypeThreeMock.Object, Array.Empty<IPublishedProperty>());

        var contentTypeFourMock = new Mock<IPublishedContentType>();
        contentTypeFourMock.SetupGet(m => m.Alias).Returns("theFourthContentType");
        var contentFourMock = new Mock<IPublishedContent>();
        ConfigurePublishedContentMock(contentFourMock, _contentFourId, "Content Four", "content-four", contentTypeFourMock.Object, Array.Empty<IPublishedProperty>());

        var documentUrlService = new Mock<IDocumentUrlService>();
        documentUrlService
            .Setup(x => x.GetDocumentKeyByRoute("/content-one", It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns(_contentOneId);
        documentUrlService
            .Setup(x => x.GetDocumentKeyByRoute("/content-two", It.IsAny<string?>(), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns(_contentTwoId);
        documentUrlService
            .Setup(x => x.GetDocumentKeyByRoute("/content-three", It.IsAny<string?>(), 1234, It.IsAny<bool>()))
            .Returns(_contentThreeId);
        documentUrlService
            .Setup(x => x.GetDocumentKeyByRoute("/content-four", It.IsIn("en-US", "da-DK"), It.IsAny<int?>(), It.IsAny<bool>()))
            .Returns(_contentFourId);

        var contentCacheMock = new Mock<IPublishedContentCache>();
        contentCacheMock
            .Setup(m => m.GetById(It.IsAny<bool>(), _contentOneId))
            .Returns(contentOneMock.Object);
        contentCacheMock
            .Setup(m => m.GetById(It.IsAny<bool>(), _contentTwoId))
            .Returns(contentTwoMock.Object);
        contentCacheMock
            .Setup(m => m.GetById(It.IsAny<bool>(), _contentThreeId))
            .Returns(contentThreeMock.Object);
        contentCacheMock
            .Setup(m => m.GetById(It.IsAny<bool>(), _contentFourId))
            .Returns(contentFourMock.Object);

        _contentCache = contentCacheMock.Object;
        _documentUrlService = documentUrlService.Object;
    }

    [Test]
    public void PublishedContentCache_CanGetById()
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings());
        var content = publishedContentCache.GetById(_contentOneId);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentOneId, content.Key);
        Assert.AreEqual("content-one", content.UrlSegment);
        Assert.AreEqual("theContentType", content.ContentType.Alias);
    }

    [Test]
    public void PublishedContentCache_CanGetByRoute()
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings());
        var content = publishedContentCache.GetByRoute("/content-two");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentTwoId, content.Key);
        Assert.AreEqual("content-two", content.UrlSegment);
        Assert.AreEqual("theOtherContentType", content.ContentType.Alias);
    }

    [Test]
    public void PublishedContentCache_CanGetByRoute_WithStartNodeIdPrefix()
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings());
        var content = publishedContentCache.GetByRoute("1234/content-three");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentThreeId, content.Key);
        Assert.AreEqual("content-three", content.UrlSegment);
        Assert.AreEqual("theThirdContentType", content.ContentType.Alias);
    }

    [Test]
    public void PublishedContentCache_CanGetByIds()
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings());
        var content = publishedContentCache.GetByIds(new[] { _contentOneId, _contentTwoId }).ToArray();
        Assert.AreEqual(2, content.Length);
        Assert.AreEqual(_contentOneId, content.First().Key);
        Assert.AreEqual(_contentTwoId, content.Last().Key);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PublishedContentCache_GetById_SupportsDisallowList(bool denied)
    {
        var denyList = denied ? new[] { "theOtherContentType" } : null;
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetById(_contentTwoId);

        if (denied)
        {
            Assert.IsNull(content);
        }
        else
        {
            Assert.IsNotNull(content);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PublishedContentCache_GetByRoute_SupportsDisallowList(bool denied)
    {
        var denyList = denied ? new[] { "theContentType" } : null;
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetByRoute("/content-one");

        if (denied)
        {
            Assert.IsNull(content);
        }
        else
        {
            Assert.IsNotNull(content);
        }
    }

    [TestCase("theContentType")]
    [TestCase("theOtherContentType")]
    public void PublishedContentCache_GetByIds_SupportsDisallowList(string deniedContentType)
    {
        var denyList = new[] { deniedContentType };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetByIds(new[] { _contentOneId, _contentTwoId }).ToArray();

        Assert.AreEqual(1, content.Length);
        if (deniedContentType == "theContentType")
        {
            Assert.AreEqual(_contentTwoId, content.First().Key);
        }
        else
        {
            Assert.AreEqual(_contentOneId, content.First().Key);
        }
    }

    [Test]
    public void PublishedContentCache_GetById_CanRetrieveContentTypesOutsideTheDisallowList()
    {
        var denyList = new[] { "theContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetById(_contentTwoId);
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentTwoId, content.Key);
        Assert.AreEqual("content-two", content.UrlSegment);
        Assert.AreEqual("theOtherContentType", content.ContentType.Alias);
    }

    [Test]
    public void PublishedContentCache_GetByRoute_CanRetrieveContentTypesOutsideTheDisallowList()
    {
        var denyList = new[] { "theOtherContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentOneId, content.Key);
        Assert.AreEqual("content-one", content.UrlSegment);
        Assert.AreEqual("theContentType", content.ContentType.Alias);
    }

    [TestCase("en-US")]
    [TestCase("da-DK")]
    public void PublishedContentCache_GetByRoute_CanRetrieveForCulture(string culture)
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(), culture);
        var content = publishedContentCache.GetByRoute("/content-four");
        Assert.IsNotNull(content);
        Assert.AreEqual(_contentFourId, content.Key);
        Assert.AreEqual("content-four", content.UrlSegment);
        Assert.AreEqual("theFourthContentType", content.ContentType.Alias);
    }

    [TestCase("de-DE")]
    [TestCase(null)]
    public void PublishedContentCache_GetByRoute_CannotRetrieveForMissingOrUnknownCulture(string? culture)
    {
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(), culture);
        var content = publishedContentCache.GetByRoute("/content-four");
        Assert.IsNull(content);
    }

    [Test]
    public void PublishedContentCache_GetByIds_CanDenyAllRequestedContent()
    {
        var denyList = new[] { "theContentType", "theOtherContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetByIds(new[] { _contentOneId, _contentTwoId }).ToArray();
        Assert.IsEmpty(content);
    }

    [Test]
    public void PublishedContentCache_DisallowListIsCaseInsensitive()
    {
        var denyList = new[] { "THEcontentTYPE" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList));
        var content = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNull(content);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PublishedContentCache_GetById_SupportsAllowList(bool allowed)
    {
        var allowList = allowed ? new[] { "theOtherContentType" } : new[] { "someOtherType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(allowedContentTypeAliases: allowList));
        var content = publishedContentCache.GetById(_contentTwoId);

        if (allowed)
        {
            Assert.IsNotNull(content);
        }
        else
        {
            Assert.IsNull(content);
        }
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PublishedContentCache_GetByRoute_SupportsAllowList(bool allowed)
    {
        var allowList = allowed ? new[] { "theContentType" } : new[] { "someOtherType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(allowedContentTypeAliases: allowList));
        var content = publishedContentCache.GetByRoute("/content-one");

        if (allowed)
        {
            Assert.IsNotNull(content);
        }
        else
        {
            Assert.IsNull(content);
        }
    }

    [TestCase("theContentType")]
    [TestCase("theOtherContentType")]
    public void PublishedContentCache_GetByIds_SupportsAllowList(string allowedContentType)
    {
        var allowList = new[] { allowedContentType };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(allowedContentTypeAliases: allowList));
        var content = publishedContentCache.GetByIds(new[] { _contentOneId, _contentTwoId }).ToArray();

        Assert.AreEqual(1, content.Length);
        if (allowedContentType == "theContentType")
        {
            Assert.AreEqual(_contentOneId, content.First().Key);
        }
        else
        {
            Assert.AreEqual(_contentTwoId, content.First().Key);
        }
    }

    [Test]
    public void PublishedContentCache_GetByIds_AllowListCanAllowMultipleContentTypes()
    {
        var allowList = new[] { "theContentType", "theOtherContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(allowedContentTypeAliases: allowList));
        var content = publishedContentCache.GetByIds(new[] { _contentOneId, _contentTwoId }).ToArray();
        Assert.AreEqual(2, content.Length);
    }

    [Test]
    public void PublishedContentCache_AllowListIsCaseInsensitive()
    {
        var allowList = new[] { "THEcontentTYPE" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(allowedContentTypeAliases: allowList));
        var content = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNotNull(content);
    }

    [Test]
    public void PublishedContentCache_AllowListTakesPrecedenceOverDisallowList()
    {
        var denyList = new[] { "theContentType" };
        var allowList = new[] { "theContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList, allowList));
        var content = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNotNull(content);
    }

    [Test]
    public void PublishedContentCache_AllowListIgnoresDisallowListCompletely()
    {
        var denyList = new[] { "theOtherContentType" };
        var allowList = new[] { "theContentType" };
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList, allowList));

        var contentOne = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNotNull(contentOne);

        var contentTwo = publishedContentCache.GetById(_contentTwoId);
        Assert.IsNull(contentTwo);
    }

    [Test]
    public void PublishedContentCache_EmptyAllowListFallsBackToDisallowList()
    {
        var denyList = new[] { "theContentType" };
        string[] allowList = Array.Empty<string>();
        var publishedContentCache = CreateApiPublishedContentCache(CreateDeliveryApiSettings(denyList, allowList));

        var contentOne = publishedContentCache.GetByRoute("/content-one");
        Assert.IsNull(contentOne);

        var contentTwo = publishedContentCache.GetById(_contentTwoId);
        Assert.IsNotNull(contentTwo);
    }

    private IVariationContextAccessor CreateVariationContextAccessor(string? culture = null)
    {
        var mock = new Mock<IVariationContextAccessor>();
        mock.SetupGet(m => m.VariationContext).Returns(new VariationContext(culture));
        return mock.Object;
    }

    private IRequestPreviewService CreateRequestPreviewService(bool isPreview = false)
    {
        var previewServiceMock = new Mock<IRequestPreviewService>();
        previewServiceMock.Setup(m => m.IsPreview()).Returns(isPreview);
        return previewServiceMock.Object;
    }

    private IOptionsMonitor<DeliveryApiSettings> CreateDeliveryApiSettings(string[]? disallowedContentTypeAliases = null, string[]? allowedContentTypeAliases = null)
    {
        var deliveryApiSettings = new DeliveryApiSettings
        {
            DisallowedContentTypeAliases = new HashSet<string>(disallowedContentTypeAliases ?? Array.Empty<string>()),
            AllowedContentTypeAliases = new HashSet<string>(allowedContentTypeAliases ?? Array.Empty<string>()),
        };
        var deliveryApiOptionsMonitorMock = new Mock<IOptionsMonitor<DeliveryApiSettings>>();
        deliveryApiOptionsMonitorMock.SetupGet(s => s.CurrentValue).Returns(deliveryApiSettings);
        return deliveryApiOptionsMonitorMock.Object;
    }

    private ApiPublishedContentCache CreateApiPublishedContentCache(IOptionsMonitor<DeliveryApiSettings> deliveryApiSettings, string? culture = null)
        => new(CreateRequestPreviewService(), deliveryApiSettings, CreateApiDocumentUrlService(), _contentCache, CreateVariationContextAccessor(culture));

    private IApiDocumentUrlService CreateApiDocumentUrlService() => new ApiDocumentUrlService(_documentUrlService);
}
