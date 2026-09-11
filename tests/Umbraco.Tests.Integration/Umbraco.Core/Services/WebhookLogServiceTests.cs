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
                Assert.That(allWebhookLogsPaged, Is.Not.Null);
                Assert.That(allWebhookLogsPaged.Items, Is.Not.Empty);
                Assert.That(allWebhookLogsPaged.Items.Count(), Is.EqualTo(1));
                var webhookLog = allWebhookLogsPaged.Items.First();
                Assert.That(webhookLog.Date.ToString(CultureInfo.InvariantCulture), Is.EqualTo(createdWebhookLog.Date.ToString(CultureInfo.InvariantCulture)));
                Assert.That(webhookLog.EventAlias, Is.EqualTo(createdWebhookLog.EventAlias));
                Assert.That(webhookLog.RequestBody, Is.EqualTo(createdWebhookLog.RequestBody));
                Assert.That(webhookLog.ResponseBody, Is.EqualTo(createdWebhookLog.ResponseBody));
                Assert.That(webhookLog.StatusCode, Is.EqualTo(createdWebhookLog.StatusCode));
                Assert.That(webhookLog.RetryCount, Is.EqualTo(createdWebhookLog.RetryCount));
                Assert.That(webhookLog.Key, Is.EqualTo(createdWebhookLog.Key));
            }
        });
    }
}
