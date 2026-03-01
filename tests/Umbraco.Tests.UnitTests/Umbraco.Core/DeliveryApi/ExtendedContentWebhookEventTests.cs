using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Webhooks.Events;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.DeliveryApi;

[TestFixture]
public class ExtendedContentWebhookEventTests : DeliveryApiTests
{
    private Mock<IPublishedContentCache> _contentCacheMock = null!;
    private Mock<IApiContentResponseBuilder> _apiContentBuilderMock = null!;
    private Mock<IContent> _contentMock = null!;
    private Mock<IPublishedContent> _publishedContentMock = null!;
    private TestExtendedContentPublishedWebhookEvent _publishedWebhookEvent = null!;
    private TestExtendedContentSavedWebhookEvent _savedWebhookEvent = null!;

    [SetUp]
    public override void Setup()
    {
        base.Setup();

        _contentCacheMock = new Mock<IPublishedContentCache>();
        _apiContentBuilderMock = new Mock<IApiContentResponseBuilder>();
        _contentMock = new Mock<IContent>();
        _publishedContentMock = new Mock<IPublishedContent>();

        var webhookFiringServiceMock = new Mock<IWebhookFiringService>();
        var webhookServiceMock = new Mock<IWebhookService>();
        var webhookSettingsMock = new Mock<IOptionsMonitor<WebhookSettings>>();
        webhookSettingsMock.SetupGet(m => m.CurrentValue).Returns(new WebhookSettings());
        var serverRoleAccessorMock = new Mock<IServerRoleAccessor>();

        _publishedWebhookEvent = new TestExtendedContentPublishedWebhookEvent(
            webhookFiringServiceMock.Object,
            webhookServiceMock.Object,
            webhookSettingsMock.Object,
            serverRoleAccessorMock.Object,
            _apiContentBuilderMock.Object,
            _contentCacheMock.Object,
            CreateOutputExpansionStrategyAccessor(),
            CreateVariationContextAccessor());

        _savedWebhookEvent = new TestExtendedContentSavedWebhookEvent(
            webhookFiringServiceMock.Object,
            webhookServiceMock.Object,
            webhookSettingsMock.Object,
            serverRoleAccessorMock.Object,
            _apiContentBuilderMock.Object,
            _contentCacheMock.Object,
            CreateVariationContextAccessor(),
            CreateOutputExpansionStrategyAccessor());
    }

    [Test]
    public void ExtendedContentPublishedWebhookEvent_InvariantContent_IncludesPropertiesInPayload()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        _contentMock.SetupGet(c => c.Key).Returns(contentKey);

        var prop = new PublishedElementPropertyBase(DeliveryApiPropertyType, _publishedContentMock.Object, false, PropertyCacheLevel.None, new VariationContext(), Mock.Of<ICacheManager>());

        var contentTypeMock = new Mock<IPublishedContentType>();
        contentTypeMock.SetupGet(c => c.Alias).Returns("testPage");
        contentTypeMock.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        ConfigurePublishedContentMock(_publishedContentMock, contentKey, "Test Page", "test-page", contentTypeMock.Object, [prop]);

        // For invariant content, Cultures only has empty-string entry - which GetCultures filters out
        var apiContentResponse = new ApiContentResponse(
            contentKey,
            "Test Page",
            "testPage",
            DateTime.UtcNow,
            DateTime.UtcNow,
            new ApiContentRoute("/test-page/", new ApiContentStartItem(contentKey, "test-page")),
            new Dictionary<string, object?> { { "deliveryApi", "property value" } },
            new Dictionary<string, IApiContentRoute>()); // empty cultures = invariant content

        _contentCacheMock.Setup(c => c.GetById(It.IsAny<Guid>())).Returns(_publishedContentMock.Object);
        _apiContentBuilderMock.Setup(b => b.Build(_publishedContentMock.Object)).Returns(apiContentResponse);

        // Act
        var payload = _publishedWebhookEvent.InvokeConvertEntityToRequestPayload(_contentMock.Object);

        // Assert
        Assert.IsNotNull(payload);

        var payloadType = payload!.GetType();

        // Verify Properties is included in payload
        var propertiesProp = payloadType.GetProperty("Properties");
        Assert.IsNotNull(propertiesProp, "Payload should include 'Properties' field");

        var properties = propertiesProp!.GetValue(payload) as IDictionary<string, object?>;
        Assert.IsNotNull(properties);
        Assert.IsTrue(properties!.ContainsKey("deliveryApi"), "Properties should contain the property value");
        Assert.AreEqual("property value", properties["deliveryApi"]);

        // Verify Cultures is empty for invariant content
        var culturesProp = payloadType.GetProperty("Cultures");
        Assert.IsNotNull(culturesProp, "Payload should include 'Cultures' field");

        var cultures = culturesProp!.GetValue(payload) as Dictionary<string, object>;
        Assert.IsNotNull(cultures);
        Assert.IsEmpty(cultures!, "Cultures should be empty for invariant content");
    }

    [Test]
    public void ExtendedContentSavedWebhookEvent_InvariantContent_IncludesPropertiesInPayload()
    {
        // Arrange
        var contentKey = Guid.NewGuid();
        _contentMock.SetupGet(c => c.Key).Returns(contentKey);

        var prop = new PublishedElementPropertyBase(DeliveryApiPropertyType, _publishedContentMock.Object, false, PropertyCacheLevel.None, new VariationContext(), Mock.Of<ICacheManager>());

        var contentTypeMock = new Mock<IPublishedContentType>();
        contentTypeMock.SetupGet(c => c.Alias).Returns("testPage");
        contentTypeMock.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);
        contentTypeMock.SetupGet(c => c.Variations).Returns(ContentVariation.Nothing);

        ConfigurePublishedContentMock(_publishedContentMock, contentKey, "Test Page", "test-page", contentTypeMock.Object, [prop]);

        var apiContentResponse = new ApiContentResponse(
            contentKey,
            "Test Page",
            "testPage",
            DateTime.UtcNow,
            DateTime.UtcNow,
            new ApiContentRoute("/test-page/", new ApiContentStartItem(contentKey, "test-page")),
            new Dictionary<string, object?> { { "deliveryApi", "property value" } },
            new Dictionary<string, IApiContentRoute>()); // empty cultures = invariant content

        _contentCacheMock.Setup(c => c.GetById(true, contentKey)).Returns(_publishedContentMock.Object);
        _apiContentBuilderMock.Setup(b => b.Build(_publishedContentMock.Object)).Returns(apiContentResponse);

        // Act
        var payload = _savedWebhookEvent.InvokeConvertEntityToRequestPayload(_contentMock.Object);

        // Assert
        Assert.IsNotNull(payload);

        var payloadType = payload!.GetType();

        // Verify Properties is included in payload
        var propertiesProp = payloadType.GetProperty("Properties");
        Assert.IsNotNull(propertiesProp, "Payload should include 'Properties' field");

        var properties = propertiesProp!.GetValue(payload) as IDictionary<string, object?>;
        Assert.IsNotNull(properties);
        Assert.IsTrue(properties!.ContainsKey("deliveryApi"), "Properties should contain the property value");
        Assert.AreEqual("property value", properties["deliveryApi"]);

        // Verify Cultures is empty for invariant content
        var culturesProp = payloadType.GetProperty("Cultures");
        Assert.IsNotNull(culturesProp, "Payload should include 'Cultures' field");

        var cultures = culturesProp!.GetValue(payload) as Dictionary<string, object>;
        Assert.IsNotNull(cultures);
        Assert.IsEmpty(cultures!, "Cultures should be empty for invariant content");
    }

    /// <summary>
    /// Test subclass to expose the protected ConvertEntityToRequestPayload method.
    /// </summary>
    private sealed class TestExtendedContentPublishedWebhookEvent : ExtendedContentPublishedWebhookEvent
    {
        public TestExtendedContentPublishedWebhookEvent(
            IWebhookFiringService webhookFiringService,
            IWebhookService webhookService,
            IOptionsMonitor<WebhookSettings> webhookSettings,
            IServerRoleAccessor serverRoleAccessor,
            IApiContentResponseBuilder apiContentBuilder,
            IPublishedContentCache publishedContentCache,
            IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor,
            IVariationContextAccessor variationContextAccessor)
            : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor, apiContentBuilder, publishedContentCache, outputExpansionStrategyAccessor, variationContextAccessor)
        {
        }

        public object? InvokeConvertEntityToRequestPayload(IContent entity) => ConvertEntityToRequestPayload(entity);
    }

    /// <summary>
    /// Test subclass to expose the protected ConvertEntityToRequestPayload method.
    /// </summary>
    private sealed class TestExtendedContentSavedWebhookEvent : ExtendedContentSavedWebhookEvent
    {
        public TestExtendedContentSavedWebhookEvent(
            IWebhookFiringService webhookFiringService,
            IWebhookService webhookService,
            IOptionsMonitor<WebhookSettings> webhookSettings,
            IServerRoleAccessor serverRoleAccessor,
            IApiContentResponseBuilder apiContentBuilder,
            IPublishedContentCache contentCache,
            IVariationContextAccessor variationContextAccessor,
            IOutputExpansionStrategyAccessor outputExpansionStrategyAccessor)
            : base(webhookFiringService, webhookService, webhookSettings, serverRoleAccessor, apiContentBuilder, contentCache, variationContextAccessor, outputExpansionStrategyAccessor)
        {
        }

        public object? InvokeConvertEntityToRequestPayload(IContent entity) => ConvertEntityToRequestPayload(entity);
    }
}
