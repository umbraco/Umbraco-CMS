using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    public static Webhook BuildEntity(WebhookDto dto) =>
        new(
            dto.Url,
            dto.Enabled,
            dto.Webhook2ContentTypeKeys.Select(x => x.ContentTypeKey).ToArray(),
            dto.Webhook2Events.Select(x => x.Event).ToArray(),
            dto.Webhook2Headers.ToDictionary(x => x.Key, x => x.Value))
        {
            Id = dto.Id,
            Key = dto.Key,
            Name = dto.Name,
            Description = dto.Description,
        };

    public static WebhookDto BuildDto(IWebhook webhook)
    {
        var dto = new WebhookDto
        {
            Id = webhook.Id,
            Key = webhook.Key,
            Name = webhook.Name,
            Description = webhook.Description,
            Url = webhook.Url,
            Enabled = webhook.Enabled,
            Webhook2ContentTypeKeys = BuildEntityKey2WebhookDto(webhook).ToList(),
            Webhook2Events = BuildEvent2WebhookDto(webhook).ToList(),
            Webhook2Headers = BuildHeaders2WebhookDtos(webhook).ToList(),
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
