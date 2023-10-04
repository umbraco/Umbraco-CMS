using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class WebhookLogServiceTests : UmbracoIntegrationTest
{
    private IWebhookLogService WebhookLogService => GetRequiredService<IWebhookLogService>();

    [Test]
    public async Task Can_Create_And_Get()
    {
        var createdWebhookLog = await WebhookLogService.CreateAsync(new WebhookLog
        {
            Date = DateTime.UtcNow,
            EventName = Constants.WebhookEvents.ContentPublish,
            RequestBody = "Test Request Body",
            ResponseBody = "Test response body",
            StatusCode = "200",
            RetryCount = 0,
            Key = Guid.NewGuid(),
        });


        var webhookLogsPaged = await WebhookLogService.Get();

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(webhookLogsPaged);
            Assert.IsNotEmpty(webhookLogsPaged.Items);
            Assert.AreEqual(1, webhookLogsPaged.Items.Count());
            var webHookLog = webhookLogsPaged.Items.First();
            Assert.AreEqual(createdWebhookLog.Date, webHookLog.Date);
            Assert.AreEqual(createdWebhookLog.EventName, webHookLog.EventName);
            Assert.AreEqual(createdWebhookLog.RequestBody, webHookLog.RequestBody);
            Assert.AreEqual(createdWebhookLog.ResponseBody, webHookLog.ResponseBody);
            Assert.AreEqual(createdWebhookLog.StatusCode, webHookLog.StatusCode);
            Assert.AreEqual(createdWebhookLog.RetryCount, webHookLog.RetryCount);
            Assert.AreEqual(createdWebhookLog.Key, webHookLog.Key);
        });
    }
}
