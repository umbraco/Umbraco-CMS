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
    private IWebhookService WebhookService => GetRequiredService<IWebhookService>();

    [Test]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Can_Create_And_Get(string url, string webhookEvent, Guid key)
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, true, new[] { key }, new[] { webhookEvent }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(webhook);
            Assert.AreEqual(1, webhook.Events.Length);
            Assert.IsTrue(webhook.Events.Contains(webhookEvent));
            Assert.AreEqual(url, webhook.Url);
            Assert.IsTrue(webhook.ContentTypeKeys.Contains(key));
        });
    }

    [Test]
    public async Task Can_Get_All()
    {
        var createdWebhookOne = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var createdWebhookTwo = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentDelete }));
        var createdWebhookThree = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentUnpublish }));
        var webhooks = await WebhookService.GetAllAsync(0, int.MaxValue);

        Assert.Multiple(() =>
        {
            Assert.IsNotEmpty(webhooks.Items);
            Assert.IsNotNull(webhooks.Items.FirstOrDefault(x => x.Key == createdWebhookOne.Result.Key));
            Assert.IsNotNull(webhooks.Items.FirstOrDefault(x => x.Key == createdWebhookTwo.Result.Key));
            Assert.IsNotNull(webhooks.Items.FirstOrDefault(x => x.Key == createdWebhookThree.Result.Key));
        });
    }

    [Test]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Can_Delete(string url, string webhookEvent, Guid key)
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, true, new[] { key }, new[] { webhookEvent }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.IsNotNull(webhook);
        await WebhookService.DeleteAsync(webhook.Key);
        var deletedWebhook = await WebhookService.GetAsync(createdWebhook.Result.Key);
        Assert.IsNull(deletedWebhook);
    }

    [Test]
    public async Task Can_Create_With_No_EntityKeys()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.IsNotNull(webhook);
        Assert.IsEmpty(webhook.ContentTypeKeys);
    }

    [Test]
    public async Task Can_Update()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        createdWebhook.Result.Events = new[] { Constants.WebhookEvents.Aliases.ContentDelete };
        await WebhookService.UpdateAsync(createdWebhook.Result);

        var updatedWebhook = await WebhookService.GetAsync(createdWebhook.Result.Key);
        Assert.IsNotNull(updatedWebhook);
        Assert.AreEqual(1, updatedWebhook.Events.Length);
        Assert.IsTrue(updatedWebhook.Events.Contains(Constants.WebhookEvents.Aliases.ContentDelete));
    }

    [Test]
    public async Task Can_Get_By_EventName()
    {
        var webhook1 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var webhook2 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.Aliases.ContentUnpublish }));
        var webhook3 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: new[] { Constants.WebhookEvents.Aliases.ContentUnpublish }));

        var result = await WebhookService.GetByAliasAsync(Constants.WebhookEvents.Aliases.ContentUnpublish);

        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count());
    }
}
