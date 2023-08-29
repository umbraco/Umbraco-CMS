using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
public class WebhookServiceTests : UmbracoIntegrationTest
{
    private IWebHookService WebhookService => GetRequiredService<IWebHookService>();

    [Test]
    [TestCase("https://example.com", WebhookEvent.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", WebhookEvent.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", WebhookEvent.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", WebhookEvent.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", WebhookEvent.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Enum_Stored_as_string(string url, WebhookEvent webhookEvent, Guid key)
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, webhookEvent, key));
        var webhook = await WebhookService.GetAsync(createdWebhook.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(webhook);
            Assert.AreEqual(webhookEvent, webhook.Event);
            Assert.AreEqual(url, webhook.Url);
            Assert.AreEqual(key, webhook.EntityKey);
        });
    }
}
