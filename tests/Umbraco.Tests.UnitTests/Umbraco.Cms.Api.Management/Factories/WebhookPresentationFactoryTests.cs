using System.Globalization;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Factories;
using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Factories;

[TestFixture]
public class WebhookPresentationFactoryTests
{
    private const string ToggleDebugLocalizedText = "Toggle debug mode for more information.";

    private Mock<IHostingEnvironment> _hostingEnvironment = null!;
    private Mock<ILocalizedTextService> _localizedTextService = null!;
    private WebhookEventCollection _webhookEventCollection = null!;
    private WebhookPresentationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _hostingEnvironment = new Mock<IHostingEnvironment>();
        _localizedTextService = new Mock<ILocalizedTextService>();
        _localizedTextService
            .Setup(x => x.Localize(
                "webhooks",
                "toggleDebug",
                It.IsAny<CultureInfo?>(),
                It.IsAny<IDictionary<string, string?>?>()))
            .Returns(ToggleDebugLocalizedText);

        _webhookEventCollection = new WebhookEventCollection(Array.Empty<IWebhookEvent>);

        _factory = new WebhookPresentationFactory(
            _webhookEventCollection,
            _hostingEnvironment.Object,
            _localizedTextService.Object);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Passes_Through_StatusCode_When_Debug_Mode_Is_Off()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(statusCode: "NotFound (404)");

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual("NotFound (404)", result.StatusCode);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Passes_Through_StatusCode_When_Debug_Mode_Is_On()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(true);
        WebhookLog log = CreateWebhookLog(statusCode: "InternalServerError (500)");

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual("InternalServerError (500)", result.StatusCode);
    }

    [TestCase("OK (200)", 200)]
    [TestCase("NotFound (404)", 404)]
    [TestCase("InternalServerError (500)", 500)]
    public void CreateResponseModel_For_WebhookLog_Parses_HttpStatusCode_From_StatusCode(string statusCode, int expected)
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(statusCode: statusCode);

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual(expected, result.HttpStatusCode);
    }

    [TestCase("ConnectionError")]
    [TestCase("")]
    public void CreateResponseModel_For_WebhookLog_HttpStatusCode_Is_Null_When_No_Numeric_Code(string statusCode)
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(statusCode: statusCode);

        var result = _factory.CreateResponseModel(log);

        Assert.IsNull(result.HttpStatusCode);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Passes_Through_ExceptionOccured_When_Debug_Mode_Is_Off()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(exceptionOccured: true);

        var result = _factory.CreateResponseModel(log);

        Assert.IsTrue(result.ExceptionOccured);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Replaces_ResponseBody_With_ToggleDebug_Hint_When_Debug_Mode_Is_Off()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(responseBody: "the actual response body");

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual(ToggleDebugLocalizedText, result.ResponseBody);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Masks_ResponseHeaders_When_Debug_Mode_Is_Off()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        WebhookLog log = CreateWebhookLog(responseHeaders: "Content-Type: application/json");

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual(string.Empty, result.ResponseHeaders);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Passes_Through_ResponseBody_And_ResponseHeaders_When_Debug_Mode_Is_On()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(true);
        WebhookLog log = CreateWebhookLog(
            responseBody: "the actual response body",
            responseHeaders: "Content-Type: application/json");

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual("the actual response body", result.ResponseBody);
        Assert.AreEqual("Content-Type: application/json", result.ResponseHeaders);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Maps_All_Always_Visible_Fields()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(false);
        var key = Guid.NewGuid();
        var webhookKey = Guid.NewGuid();
        var date = new DateTime(2026, 4, 15, 10, 23, 41, DateTimeKind.Utc);
        WebhookLog log = CreateWebhookLog(
            key: key,
            webhookKey: webhookKey,
            url: "https://example.test/hook",
            eventAlias: "Umbraco.ContentPublish",
            date: date,
            retryCount: 2,
            requestBody: "the request body",
            requestHeaders: "X-Test: yes",
            isSuccessStatusCode: false);

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual(key, result.Key);
        Assert.AreEqual(webhookKey, result.WebhookKey);
        Assert.AreEqual("https://example.test/hook", result.Url);
        Assert.AreEqual("Umbraco.ContentPublish", result.EventAlias);
        Assert.AreEqual(date, result.Date);
        Assert.AreEqual(2, result.RetryCount);
        Assert.AreEqual("the request body", result.RequestBody);
        Assert.AreEqual("X-Test: yes", result.RequestHeaders);
        Assert.IsFalse(result.IsSuccessStatusCode);
    }

    [Test]
    public void CreateResponseModel_For_WebhookLog_Defaults_RequestBody_To_Empty_String_When_Null()
    {
        _hostingEnvironment.SetupGet(x => x.IsDebugMode).Returns(true);
        WebhookLog log = CreateWebhookLog(requestBody: null);

        var result = _factory.CreateResponseModel(log);

        Assert.AreEqual(string.Empty, result.RequestBody);
    }

    [Test]
    public void CreateResponseModel_For_Webhook_Maps_Event_To_WebhookEventResponseModel_With_Known_Event()
    {
        var knownEvent = new Mock<IWebhookEvent>();
        knownEvent.SetupGet(x => x.Alias).Returns("Umbraco.ContentPublish");
        knownEvent.SetupGet(x => x.EventName).Returns("Content Published");
        knownEvent.SetupGet(x => x.EventType).Returns("Content");

        _webhookEventCollection = new WebhookEventCollection(() => [knownEvent.Object]);
        _factory = new WebhookPresentationFactory(
            _webhookEventCollection,
            _hostingEnvironment.Object,
            _localizedTextService.Object);

        var webhook = new Webhook("https://example.test/hook", true, null, ["Umbraco.ContentPublish"]);

        var result = _factory.CreateResponseModel(webhook);

        Assert.AreEqual(1, result.Events.Count());
        WebhookEventResponseModel mappedEvent = result.Events.Single();
        Assert.AreEqual("Umbraco.ContentPublish", mappedEvent.Alias);
        Assert.AreEqual("Content Published", mappedEvent.EventName);
        Assert.AreEqual("Content", mappedEvent.EventType);
    }

    [Test]
    public void CreateResponseModel_For_Webhook_Falls_Back_To_Alias_And_Other_Type_When_Event_Is_Unknown()
    {
        var webhook = new Webhook("https://example.test/hook", true, null, ["Some.Unregistered.Event"]);

        var result = _factory.CreateResponseModel(webhook);

        WebhookEventResponseModel mappedEvent = result.Events.Single();
        Assert.AreEqual("Some.Unregistered.Event", mappedEvent.Alias);
        Assert.AreEqual("Some.Unregistered.Event", mappedEvent.EventName);
        Assert.AreEqual(Constants.WebhookEvents.Types.Other, mappedEvent.EventType);
    }

    private static WebhookLog CreateWebhookLog(
        Guid? key = null,
        Guid? webhookKey = null,
        string url = "https://example.test/hook",
        string eventAlias = "Umbraco.ContentPublish",
        DateTime? date = null,
        string statusCode = "OK (200)",
        int retryCount = 0,
        string? requestBody = "",
        string requestHeaders = "",
        string responseBody = "",
        string responseHeaders = "",
        bool exceptionOccured = false,
        bool isSuccessStatusCode = true)
        => new()
        {
            Key = key ?? Guid.NewGuid(),
            WebhookKey = webhookKey ?? Guid.NewGuid(),
            Url = url,
            EventAlias = eventAlias,
            Date = date ?? DateTime.UtcNow,
            StatusCode = statusCode,
            RetryCount = retryCount,
            RequestBody = requestBody,
            RequestHeaders = requestHeaders,
            ResponseBody = responseBody,
            ResponseHeaders = responseHeaders,
            ExceptionOccured = exceptionOccured,
            IsSuccessStatusCode = isSuccessStatusCode,
        };
}
