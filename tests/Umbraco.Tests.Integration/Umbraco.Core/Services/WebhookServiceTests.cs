using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class WebhookServiceTests : UmbracoIntegrationTest
{
    private IWebhookService WebhookService => GetRequiredService<IWebhookService>();

    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentPublish, "00000000-0000-0000-0000-010000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentDelete, "00000000-0000-0000-0000-000200000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.ContentUnpublish, "00000000-0000-0000-0000-300000000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaDelete, "00000000-0000-0000-0000-000004000000")]
    [TestCase("https://example.com", Constants.WebhookEvents.Aliases.MediaSave, "00000000-0000-0000-0000-000000500000")]
    public async Task Can_Create_And_Get(string url, string webhookEvent, Guid key)
    {
        IWebhook webhook = new Webhook(url, true, [key], [webhookEvent]);
        var createdWebhook = await WebhookService.CreateAsync(webhook);
        webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

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
    public async Task Can_Create_And_Update_And_Get_With_Name_And_Description()
    {
        var contentTypeKey = Guid.NewGuid();
        const string Url = "https://example.com";
        const string Event = Constants.WebhookEvents.Aliases.ContentPublish;
        const string Name = "Example name";
        const string Description = "Example description";
        IWebhook webhook = new Webhook(Url, true, [contentTypeKey], [Event])
        {
            Name = Name,
            Description = Description
        };
        var createdWebhook = await WebhookService.CreateAsync(webhook);
        webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(webhook);
            Assert.AreEqual(1, webhook.Events.Length);
            Assert.IsTrue(webhook.Events.Contains(Event));
            Assert.AreEqual(Url, webhook.Url);
            Assert.AreEqual(Name, webhook.Name);
            Assert.AreEqual(Description, webhook.Description);
            Assert.IsTrue(webhook.ContentTypeKeys.Contains(contentTypeKey));
        });

        const string UpdatedName = "Updated name";
        const string UpdatedDescription = "Updated description";

        webhook.Name = UpdatedName;
        webhook.Description = UpdatedDescription;

        await WebhookService.UpdateAsync(webhook);

        var updatedWebhook = await WebhookService.GetAsync(webhook.Key);

        Assert.Multiple(() =>
        {
            Assert.IsNotNull(updatedWebhook);
            Assert.AreEqual(UpdatedName, updatedWebhook.Name);
            Assert.AreEqual(UpdatedDescription, updatedWebhook.Description);
        });
    }

    [Test]
    public async Task Can_Get_All()
    {
        var createdWebhookOne = await WebhookService.CreateAsync(new Webhook("https://example.com", true, [Guid.NewGuid()], [Constants.WebhookEvents.Aliases.ContentPublish]));
        var createdWebhookTwo = await WebhookService.CreateAsync(new Webhook("https://example.com", true, [Guid.NewGuid()], [Constants.WebhookEvents.Aliases.ContentDelete]));
        var createdWebhookThree = await WebhookService.CreateAsync(new Webhook("https://example.com", true, [Guid.NewGuid()], [Constants.WebhookEvents.Aliases.ContentUnpublish]));
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
        var createdWebhook = await WebhookService.CreateAsync(new Webhook(url, true, [key], [webhookEvent]));
        var webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.IsNotNull(webhook);
        await WebhookService.DeleteAsync(webhook.Key);
        var deletedWebhook = await WebhookService.GetAsync(createdWebhook.Result.Key);
        Assert.IsNull(deletedWebhook);
    }

    [Test]
    public async Task Can_Create_With_No_EntityKeys()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: [Constants.WebhookEvents.Aliases.ContentPublish]));
        var webhook = await WebhookService.GetAsync(createdWebhook.Result.Key);

        Assert.IsNotNull(webhook);
        Assert.IsEmpty(webhook.ContentTypeKeys);
    }

    [Test]
    public async Task Can_Update()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", events: [Constants.WebhookEvents.Aliases.ContentPublish]));
        createdWebhook.Result.Events = [Constants.WebhookEvents.Aliases.ContentDelete];
        await WebhookService.UpdateAsync(createdWebhook.Result);

        var updatedWebhook = await WebhookService.GetAsync(createdWebhook.Result.Key);
        Assert.IsNotNull(updatedWebhook);
        Assert.AreEqual(1, updatedWebhook.Events.Length);
        Assert.IsTrue(updatedWebhook.Events.Contains(Constants.WebhookEvents.Aliases.ContentDelete));
    }

    [Test]
    public async Task Can_Get_By_EventName()
    {
        var webhook1 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: [Constants.WebhookEvents.Aliases.ContentPublish]));
        var webhook2 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: [Constants.WebhookEvents.Aliases.ContentUnpublish]));
        var webhook3 = await WebhookService.CreateAsync(new Webhook("https://example.com", events: [Constants.WebhookEvents.Aliases.ContentUnpublish]));

        var result = await WebhookService.GetByAliasAsync(Constants.WebhookEvents.Aliases.ContentUnpublish);

        Assert.IsNotEmpty(result);
        Assert.AreEqual(2, result.Count());
    }
}
