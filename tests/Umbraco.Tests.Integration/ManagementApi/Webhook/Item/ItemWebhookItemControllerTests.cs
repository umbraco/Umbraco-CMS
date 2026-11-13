using System.Linq.Expressions;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Controllers.Webhook.Item;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.Integration.ManagementApi.Webhook.Item;

public class ItemWebhookItemControllerTests : ManagementApiUserGroupTestBase<ItemWebhookItemController>
{
    private IWebhookService WebhookService => GetRequiredService<IWebhookService>();

    private Guid _webhookKey;

    [SetUp]
    public async Task SetUp()
    {
        var webhook = new Core.Models.Webhook(
            "https://example.com/hook?token=secret",
            true,
            events: [Constants.WebhookEvents.Aliases.ContentPublish]);
        var response = await WebhookService.CreateAsync(webhook);
        _webhookKey = response.Result.Key;
    }

    protected override Expression<Func<ItemWebhookItemController, object>> MethodSelector =>
        x => x.Item(CancellationToken.None, new HashSet<Guid> { _webhookKey });
}
