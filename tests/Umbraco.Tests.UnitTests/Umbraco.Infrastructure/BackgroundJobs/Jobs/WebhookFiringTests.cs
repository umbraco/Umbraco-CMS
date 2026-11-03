// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs.Jobs.DistributedJobs;
using Umbraco.Cms.Tests.Common;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.BackgroundJobs.Jobs;

[TestFixture]
public class WebhookFiringTests
{
    private const int MaximumRetries = 5;

    private Mock<IWebhookRequestService> _webhookRequestService = null!;
    private Mock<IWebhookLogService> _webhookLogService = null!;
    private Mock<IWebhookService> _webhookService = null!;
    private CapturingHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _webhookRequestService = new Mock<IWebhookRequestService>();
        _webhookLogService = new Mock<IWebhookLogService>();
        _webhookLogService
            .Setup(x => x.CreateAsync(It.IsAny<WebhookLog>()))
            .ReturnsAsync((WebhookLog log) => log);
        _webhookService = new Mock<IWebhookService>();
        _handler = new CapturingHandler();
    }

    [Test]
    public async Task Custom_Content_Header_Is_Sent_As_Content_Header_And_Replaces_Default()
    {
        WebhookRequest request = SetupQueuedRequest(new Dictionary<string, string>
        {
            ["Content-Type"] = "application/vnd.custom+json",
            ["X-Custom"] = "custom-value",
        });

        await CreateJob().ExecuteAsync();

        Assert.Multiple(() =>
        {
            Assert.That(_handler.ContentTypeValues, Is.EqualTo(new[] { "application/vnd.custom+json" }));
            Assert.That(_handler.RequestHeaders["X-Custom"], Is.EqualTo(new[] { "custom-value" }));
            Assert.That(_handler.Body, Is.EqualTo("{}"));
        });
        _webhookLogService.Verify(x => x.CreateAsync(It.Is<WebhookLog>(l => l.ExceptionOccured == false)), Times.Once);
        _webhookRequestService.Verify(x => x.DeleteAsync(request), Times.Once);
    }

    [Test]
    public async Task Request_That_Fails_Before_Sending_Is_Logged_And_Retried()
    {
        WebhookRequest request = SetupQueuedRequest(new Dictionary<string, string>
        {
            ["Invalid Header"] = "value",
        });

        await CreateJob().ExecuteAsync();

        Assert.That(_handler.CallCount, Is.EqualTo(0));
        _webhookLogService.Verify(x => x.CreateAsync(It.Is<WebhookLog>(l => l.ExceptionOccured)), Times.Once);
        _webhookRequestService.Verify(x => x.UpdateAsync(It.Is<WebhookRequest>(r => r == request && r.RetryCount == 1)), Times.Once);
        _webhookRequestService.Verify(x => x.DeleteAsync(It.IsAny<WebhookRequest>()), Times.Never);
    }

    private WebhookRequest SetupQueuedRequest(IDictionary<string, string> headers)
    {
        var webhook = new Webhook("https://example.com/webhook", true, headers: headers) { Key = Guid.NewGuid() };
        var request = new WebhookRequest
        {
            WebhookKey = webhook.Key,
            EventAlias = Constants.WebhookEvents.Aliases.ContentPublish,
            RequestObject = "{}",
            RetryCount = 0,
        };

        _webhookService.Setup(x => x.GetAsync(webhook.Key)).ReturnsAsync(webhook);
        _webhookRequestService.Setup(x => x.GetAllAsync()).ReturnsAsync(new[] { request });
        return request;
    }

    private WebhookFiring CreateJob()
    {
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
            .Returns(Mock.Of<ICoreScope>());

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(() => new HttpClient(_handler, disposeHandler: false));

        return new WebhookFiring(
            NullLogger<WebhookFiring>.Instance,
            _webhookRequestService.Object,
            new WebhookLogFactory(),
            _webhookLogService.Object,
            _webhookService.Object,
            new TestOptionsMonitor<WebhookSettings>(new WebhookSettings { Enabled = true, MaximumRetries = MaximumRetries }),
            scopeProvider.Object,
            httpClientFactory.Object);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        public string[] ContentTypeValues { get; private set; } = [];

        public Dictionary<string, string[]> RequestHeaders { get; } = new(StringComparer.OrdinalIgnoreCase);

        public string? Body { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CallCount++;
            foreach (KeyValuePair<string, IEnumerable<string>> header in request.Headers)
            {
                RequestHeaders[header.Key] = header.Value.ToArray();
            }

            if (request.Content is not null)
            {
                ContentTypeValues = request.Content.Headers.TryGetValues("Content-Type", out IEnumerable<string>? values)
                    ? values.ToArray()
                    : [];
                Body = await request.Content.ReadAsStringAsync(cancellationToken);
            }

            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(string.Empty) };
        }
    }
}
