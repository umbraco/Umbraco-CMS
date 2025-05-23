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
internal sealed class WebhookLogServiceTests : UmbracoIntegrationTest
{
    private IWebhookLogService WebhookLogService => GetRequiredService<IWebhookLogService>();

    [Test]
    public async Task Can_Create_And_Get()
    {
        var webHookKey = Guid.NewGuid();
        var createdWebhookLog = await WebhookLogService.CreateAsync(new WebhookLog
        {
            Date = DateTime.UtcNow,
            EventAlias = Constants.WebhookEvents.Aliases.ContentPublish,
            RequestBody = "Test Request Body",
            ResponseBody = "Test response body",
            StatusCode = "200",
            RetryCount = 0,
            Key = Guid.NewGuid(),
            WebhookKey = webHookKey
        });


        var allWebhookLogsPaged = await WebhookLogService.Get();
        var specificWebhookLogsPaged = await WebhookLogService.Get(webHookKey);

        Assert.Multiple(() =>
        {
            AssertGetResult(createdWebhookLog, allWebhookLogsPaged);
            AssertGetResult(createdWebhookLog, specificWebhookLogsPaged);

            static void AssertGetResult(WebhookLog createdWebhookLog, PagedModel<WebhookLog> allWebhookLogsPaged)
            {
                Assert.IsNotNull(allWebhookLogsPaged);
                Assert.IsNotEmpty(allWebhookLogsPaged.Items);
                Assert.AreEqual(1, allWebhookLogsPaged.Items.Count());
                var webhookLog = allWebhookLogsPaged.Items.First();
                Assert.AreEqual(createdWebhookLog.Date.ToString(CultureInfo.InvariantCulture), webhookLog.Date.ToString(CultureInfo.InvariantCulture));
                Assert.AreEqual(createdWebhookLog.EventAlias, webhookLog.EventAlias);
                Assert.AreEqual(createdWebhookLog.RequestBody, webhookLog.RequestBody);
                Assert.AreEqual(createdWebhookLog.ResponseBody, webhookLog.ResponseBody);
                Assert.AreEqual(createdWebhookLog.StatusCode, webhookLog.StatusCode);
                Assert.AreEqual(createdWebhookLog.RetryCount, webhookLog.RetryCount);
                Assert.AreEqual(createdWebhookLog.Key, webhookLog.Key);
            }
        });
    }
}
