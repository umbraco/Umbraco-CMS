using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
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
            EventAlias = Constants.WebhookEvents.Aliases.ContentPublish,
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
            var webhookLog = webhookLogsPaged.Items.First();
            Assert.AreEqual(createdWebhookLog.Date.ToString(CultureInfo.InvariantCulture), webhookLog.Date.ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual(createdWebhookLog.EventAlias, webhookLog.EventAlias);
            Assert.AreEqual(createdWebhookLog.RequestBody, webhookLog.RequestBody);
            Assert.AreEqual(createdWebhookLog.ResponseBody, webhookLog.ResponseBody);
            Assert.AreEqual(createdWebhookLog.StatusCode, webhookLog.StatusCode);
            Assert.AreEqual(createdWebhookLog.RetryCount, webhookLog.RetryCount);
            Assert.AreEqual(createdWebhookLog.Key, webhookLog.Key);
        });
    }
}
