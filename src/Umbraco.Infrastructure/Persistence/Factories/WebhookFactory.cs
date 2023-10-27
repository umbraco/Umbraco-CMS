using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto, IEnumerable<EntityKey2WebhookDto>? entityKey2WebhookDtos = null, IEnumerable<Event2WebhookDto>? event2WebhookDtos = null, IEnumerable<Headers2WebhookDto>? headersWebhookDtos = null)
    {
        var entity = new Webhook(
            dto.Url,
            dto.Enabled,
            entityKey2WebhookDtos?.Select(x => x.EntityKey).ToArray(),
            event2WebhookDtos?.Select(x => x.Event).ToArray(),
            headersWebhookDtos?.ToDictionary(x => x.Key, x => x.Value))
        {
            Id = dto.Id,
            Key = dto.Key,
        };

        return entity;
    }

    public static WebhookDto BuildDto(Webhook webhook)
    {
        var dto = new WebhookDto
        {
            Url = webhook.Url,
            Key = webhook.Key,
            Enabled = webhook.Enabled,
            Id = webhook.Id,
        };

        return dto;
    }

    public static IEnumerable<EntityKey2WebhookDto> BuildEntityKey2WebhookDto(Webhook webhook) =>
        webhook.EntityKeys.Select(x => new EntityKey2WebhookDto
        {
            EntityKey = x,
            WebhookId = webhook.Id,
        });

    public static IEnumerable<Event2WebhookDto> BuildEvent2WebhookDto(Webhook webhook) =>
        webhook.Events.Select(x => new Event2WebhookDto
        {
            Event = x,
            WebhookId = webhook.Id,
        });

    public static IEnumerable<Headers2WebhookDto> BuildHeaders2WebhookDtos(Webhook webhook) =>
        webhook.Headers.Select(x => new Headers2WebhookDto
        {
            Key = x.Key,
            Value = x.Value,
            WebhookId = webhook.Id,
        });
}
