using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto)
    {
        var entity = new Webhook(dto.Url, dto.Event, dto.EntityKey);

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
            Event = webhook.Event,
            EntityKey = webhook.EntityKey,
            Key = webhook.Key,
        };
        if (webhook.HasIdentity)
        {
            dto.Id = webhook.Id;
        }

        return dto;
    }
}
