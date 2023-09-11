using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto, IEnumerable<WebhookEntityKeyDto>? entityKey2WebhookDtos = null)
    {
        var entity = new Webhook(dto.Url, Enum.Parse<WebhookEvent>(dto.Event), dto.Enabled, entityKey2WebhookDtos?.Select(x => x.EntityKey).ToArray());
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
            Event = webhook.Event.ToString(),
            Key = webhook.Key,
            Enabled = webhook.Enabled
        };
        if (webhook.HasIdentity)
        {
            dto.Id = webhook.Id;
        }

        return dto;
    }

    public static IEnumerable<WebhookEntityKeyDto> BuildEntityKey2WebhookDtos(Webhook webhook, int webhookId) =>
        webhook.EntityKeys.Select(x => new WebhookEntityKeyDto
        {
            EntityKey = x,
            WebhookId = webhookId
        });
}
