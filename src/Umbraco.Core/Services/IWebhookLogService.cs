using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

public interface IWebhookLogService
{
    Task<WebhookLog> CreateAsync(WebhookLog webhookLog);

    Task<PagedModel<WebhookLog>> Get(int skip = 0, int take = int.MaxValue);

    // TODO (V16): Remove the default implementation on this method.
    async Task<PagedModel<WebhookLog>> Get(Guid webhookKey, int skip = 0, int take = int.MaxValue)
    {
        // This is very inefficient as the filter/skip/take is in-memory, but it will return the correct data.
        // As it's only here to avoid a breaking change on the interface that is unlikely to have a custom
        // implementation, this seems reasonable.
        PagedModel<WebhookLog> allLogs = await Get(0, int.MaxValue);
        var logsForId = allLogs.Items.Where(x => x.WebhookKey == webhookKey).ToList();
        return new PagedModel<WebhookLog>(logsForId.Count, logsForId.Skip(skip).Take(take));
    }
}
