using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto, IEnumerable<Webhook2ContentTypeKeysDto>? entityKey2WebhookDtos = null, IEnumerable<Webhook2EventsDto>? event2WebhookDtos = null, IEnumerable<Webhook2HeadersDto>? headersWebhookDtos = null)
    {
        var entity = new Webhook(
            dto.Url,
            dto.Enabled,
            entityKey2WebhookDtos?.Select(x => x.ContentTypeKey).ToArray(),
            event2WebhookDtos?.Select(x => x.Event).ToArray(),
            headersWebhookDtos?.ToDictionary(x => x.Key, x => x.Value))
        {
            Id = dto.Id,
            Key = dto.Key,
        };

        return entity;
    }

    public static WebhookDto BuildDto(IWebhook webhook)
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

    public static IEnumerable<Webhook2ContentTypeKeysDto> BuildEntityKey2WebhookDto(IWebhook webhook) =>
        webhook.ContentTypeKeys.Select(x => new Webhook2ContentTypeKeysDto
        {
            ContentTypeKey = x,
            WebhookId = webhook.Id,
        });

    public static IEnumerable<Webhook2EventsDto> BuildEvent2WebhookDto(IWebhook webhook) =>
        webhook.Events.Select(x => new Webhook2EventsDto
        {
            Event = x,
            WebhookId = webhook.Id,
        });

    public static IEnumerable<Webhook2HeadersDto> BuildHeaders2WebhookDtos(IWebhook webhook) =>
        webhook.Headers.Select(x => new Webhook2HeadersDto
        {
            Key = x.Key,
            Value = x.Value,
            WebhookId = webhook.Id,
        });
}
