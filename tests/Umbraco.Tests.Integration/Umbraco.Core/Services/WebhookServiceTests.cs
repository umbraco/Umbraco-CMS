using NUnit.Framework;
using Umbraco.Cms.Core;
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
    [TestCase("https://example.com", Constants.WebhookEvents.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Can_Create_And_Get(string url, string webhookEvent, Guid key)
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, true, new[] { key }, new[] { webhookEvent }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(webhook);
            Assert.AreEqual(1, webhook.Events.Length);
            Assert.IsTrue(webhook.Events.Contains(webhookEvent));
            Assert.AreEqual(url, webhook.Url);
            Assert.IsTrue(webhook.EntityKeys.Contains(key));
        });
    }

    [Test]
    public async Task Can_Get_Multiple()
    {
        var createdWebhookOne = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.ContentPublish }));
        var createdWebhookTwo = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.ContentDelete }));
        var keys = new List<Guid> { createdWebhookOne.Key, createdWebhookTwo.Key };
        var webhooks = await WebhookService.GetMultipleAsync(keys);

        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(webhooks);
            Assert.IsNotNull(webhooks.FirstOrDefault(x => x.Key == createdWebhookOne.Key));
            Assert.IsNotNull(webhooks.FirstOrDefault(x => x.Key == createdWebhookTwo.Key));
        });
    }

    [Test]
    public async Task Can_Get_All()
    {
        var createdWebhookOne = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.ContentPublish }));
        var createdWebhookTwo = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.ContentDelete }));
        var createdWebhookThree = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.ContentUnpublish }));
        var webhooks = await WebhookService.GetAllAsync();

        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(webhooks);
            Assert.IsNotNull(webhooks.FirstOrDefault(x => x.Key == createdWebhookOne.Key));
            Assert.IsNotNull(webhooks.FirstOrDefault(x => x.Key == createdWebhookTwo.Key));
            Assert.IsNotNull(webhooks.FirstOrDefault(x => x.Key == createdWebhookThree.Key));
        });
    }

    [Test]
    [TestCase("https://example.com", Constants.WebhookEvents.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Can_Delete(string url, string webhookEvent, Guid key)
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, true, new[] { key }, new[] { webhookEvent }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Key);

        Assert.IsNotNull(webhook);
        await WebhookService.DeleteAsync(webhook.Key);
        var deletedWebhook = await WebhookService.GetAsync(createdWebhook.Key);
        Assert.IsNull(deletedWebhook);
    }

    [Test]
    public async Task Can_Create_With_No_EntityKeys()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.ContentPublish }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Key);

        Assert.IsNotNull(webhook);
        Assert.IsEmpty(webhook.EntityKeys);
    }

    [Test]
    public async Task Can_Update()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.ContentPublish }));
        createdWebhook.Events = new[] { Constants.WebhookEvents.ContentDelete };
        await WebhookService.UpdateAsync(createdWebhook);

        var updatedWebhook = await WebhookService.GetAsync(createdWebhook.Key);
        Assert.IsNotNull(updatedWebhook);
        Assert.AreEqual(1, updatedWebhook.Events.Length);
        Assert.IsTrue(updatedWebhook.Events.Contains(Constants.WebhookEvents.ContentDelete));
    }

    [Test]
    public async Task Can_Get_By_EventName()
    {
        var webhook1 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.ContentPublish }));
        var webhook2 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.ContentUnpublish }));
        var webhook3 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.ContentUnpublish }));

        var result = await WebhookService.GetByEventName(Constants.WebhookEvents.ContentUnpublish);

        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count());
    }
}
