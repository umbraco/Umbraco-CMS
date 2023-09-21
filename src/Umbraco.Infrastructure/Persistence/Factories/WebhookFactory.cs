using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto, IEnumerable<EntityKey2WebhookDto>? entityKey2WebhookDtos = null, IEnumerable<Event2WebhookDto>? event2WebhookDtos = null)
    {
        var entity = new Webhook(
            dto.Url,
            dto.Enabled,
            entityKey2WebhookDtos?.Select(x => x.EntityKey).ToArray(),
            event2WebhookDtos?.Select(x => x.Event).ToArray());

        try
        {
            entity.DisableChangeTracking();

            entity.Id = dto.Id;
            entity.Key = dto.Key;

            // reset dirty initial properties (U4-1946)
            entity.ResetDirtyProperties(false);
            return entity;
        }
        finally
        {
            entity.EnableChangeTracking();
        }
    }

    public static WebhookDto BuildDto(Webhook webhook)
    {
        var dto = new WebhookDto
        {
            Url = webhook.Url,
            Key = webhook.Key,
            Enabled = webhook.Enabled
        };
        if (webhook.HasIdentity)
        {
            dto.Id = webhook.Id;
        }

        return dto;
    }

    public static IEnumerable<EntityKey2WebhookDto> BuildEntityKey2WebhookDto(Webhook webhook, int webhookId) =>
        webhook.EntityKeys.Select(x => new EntityKey2WebhookDto
        {
            EntityKey = x,
            WebhookId = webhookId,
        });

    public static IEnumerable<Event2WebhookDto> BuildEvent2WebhookDto(Webhook webhook, int webhookId) =>
        webhook.Events.Select(x => new Event2WebhookDto
        {
            Event = x,
            WebhookId = webhookId,
        });
}
