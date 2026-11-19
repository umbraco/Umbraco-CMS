using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Services;

[TestFixture]
public class WebhookServiceTests
{
    private WebhookService _webhookService;

    [SetUp]
    public void Setup()
        => _webhookService = new WebhookService(
            Mock.Of<ICoreScopeProvider>(MockBehavior.Strict),
            Mock.Of<IWebhookRepository>(MockBehavior.Strict),
            Mock.Of<IEventMessagesFactory>(MockBehavior.Strict));

    [TestCase("", "value")]
    [TestCase("Invalid Header", "value")]
    [TestCase("Invalid:Header", "value")]
    [TestCase("InvalidéHeader", "value")]
    [TestCase("X-Custom-Header", "line\r\nbreak")]
    [TestCase("X-Custom-Header", "null\0char")]
    [TestCase("X-Custom-Header", "café")]
    [TestCase("X-Custom-Header", "☃")]
    [TestCase("X-Custom-Header", null)]
    [TestCase("Host", "example.com")]
    [TestCase("content-length", "10")]
    [TestCase("Transfer-Encoding", "chunked")]
    [TestCase("Connection", "close")]
    [TestCase(Constants.WebhookEvents.HeaderNames.EventName, "value")]
    [TestCase(Constants.WebhookEvents.HeaderNames.RetryCount, "1")]
    public async Task Create_And_Update_Reject_Headers_That_Cannot_Be_Sent(string name, string? value)
    {
        var createResult = await _webhookService.CreateAsync(CreateWebhookWithHeader(name, value));
        var updateResult = await _webhookService.UpdateAsync(CreateWebhookWithHeader(name, value));

        Assert.Multiple(() =>
        {
            Assert.IsFalse(createResult.Success);
            Assert.AreEqual(WebhookOperationStatus.InvalidHeaders, createResult.Status);
            Assert.IsFalse(updateResult.Success);
            Assert.AreEqual(WebhookOperationStatus.InvalidHeaders, updateResult.Status);
        });
    }

    [TestCase("Content-Type", "application/vnd.custom+json")]
    [TestCase("X-Custom-Header", "value")]
    [TestCase("Authorization", "Bearer token")]
    [TestCase("X-Custom-Header", "tab\tand space")]
    public async Task Create_And_Update_Accept_Sendable_Headers(string name, string value)
    {
        var webhookService = CreateWebhookServiceThatPersists();

        var createResult = await webhookService.CreateAsync(CreateWebhookWithHeader(name, value));
        var updateResult = await webhookService.UpdateAsync(CreateWebhookWithHeader(name, value));

        Assert.Multiple(() =>
        {
            Assert.AreEqual(WebhookOperationStatus.Success, createResult.Status);
            Assert.AreEqual(WebhookOperationStatus.Success, updateResult.Status);
        });
    }

    private static WebhookService CreateWebhookServiceThatPersists()
    {
        var notificationPublisher = new Mock<IScopedNotificationPublisher>();
        notificationPublisher
            .Setup(x => x.PublishCancelableAsync(It.IsAny<ICancelableNotification>()))
            .ReturnsAsync(false);

        var scope = new Mock<ICoreScope>();
        scope.Setup(x => x.Notifications).Returns(notificationPublisher.Object);

        var scopeProvider = new Mock<ICoreScopeProvider>();
        scopeProvider
            .Setup(x => x.CreateCoreScope(
                It.IsAny<IsolationLevel>(),
                It.IsAny<RepositoryCacheMode>(),
                It.IsAny<IEventDispatcher>(),
                It.IsAny<IScopedNotificationPublisher>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>()))
            .Returns(scope.Object);

        var repository = new Mock<IWebhookRepository>();
        repository.Setup(x => x.CreateAsync(It.IsAny<IWebhook>())).ReturnsAsync((IWebhook webhook) => webhook);
        repository.Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync((Guid key) => new Webhook("https://example.com") { Key = key });

        return new WebhookService(scopeProvider.Object, repository.Object, Mock.Of<IEventMessagesFactory>());
    }

    private static Webhook CreateWebhookWithHeader(string name, string? value)
        => new("https://example.com", true, events: [Constants.WebhookEvents.Aliases.ContentPublish], headers: new Dictionary<string, string> { [name] = value! });
}
