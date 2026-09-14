using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class WebhookRequestServiceTests : UmbracoIntegrationTest
{
    private IWebhookRequestService WebhookRequestService => GetRequiredService<IWebhookRequestService>();

    private IWebhookService WebhookService => GetRequiredService<IWebhookService>();

    [Test]
    public async Task Can_Create_And_Get()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var created = await WebhookRequestService.CreateAsync(createdWebhook.Result.Key, Constants.WebhookEvents.Aliases.ContentPublish, null);
        var webhooks = await WebhookRequestService.GetAllAsync();
        var webhook = webhooks.First(x => x.Id == created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(webhook.Id, Is.EqualTo(created.Id));
            Assert.That(webhook.EventAlias, Is.EqualTo(created.EventAlias));
            Assert.That(webhook.RetryCount, Is.EqualTo(created.RetryCount));
            Assert.That(webhook.RequestObject, Is.EqualTo(created.RequestObject));
            Assert.That(webhook.WebhookKey, Is.EqualTo(created.WebhookKey));
        });
    }

    [Test]
    public async Task Can_Update()
    {
        var newRetryCount = 4;
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var created = await WebhookRequestService.CreateAsync(createdWebhook.Result.Key, Constants.WebhookEvents.Aliases.ContentPublish, null);
        created.RetryCount = newRetryCount;
        await WebhookRequestService.UpdateAsync(created);
        var webhooks = await WebhookRequestService.GetAllAsync();
        var webhook = webhooks.First(x => x.Id == created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(webhook.RetryCount, Is.EqualTo(newRetryCount));
        });
    }

    [Test]
    public async Task Can_Delete()
    {
        var createdWebhook = await WebhookService.CreateAsync(new Webhook("https://example.com", true, new[] { Guid.NewGuid() }, new[] { Constants.WebhookEvents.Aliases.ContentPublish }));
        var created = await WebhookRequestService.CreateAsync(createdWebhook.Result.Key, Constants.WebhookEvents.Aliases.ContentPublish, null);
        await WebhookRequestService.DeleteAsync(created);
        var webhooks = await WebhookRequestService.GetAllAsync();
        var webhook = webhooks.FirstOrDefault(x => x.Id == created.Id);

        Assert.Multiple(() =>
        {
            Assert.That(webhook, Is.Null);
        });
    }
}
