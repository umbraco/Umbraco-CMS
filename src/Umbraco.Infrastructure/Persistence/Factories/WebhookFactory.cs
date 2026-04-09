using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookFactory
{
    /// <summary>
    /// Creates a <see cref="Webhook"/> entity from the specified <see cref="WebhookDto"/> and optional related data transfer objects.
    /// </summary>
    /// <param name="dto">The <see cref="WebhookDto"/> containing the core webhook data.</param>
    /// <param name="entityKey2WebhookDtos">Optional collection of <see cref="Webhook2ContentTypeKeysDto"/> objects representing content type keys associated with the webhook.</param>
    /// <param name="event2WebhookDtos">Optional collection of <see cref="Webhook2EventsDto"/> objects representing events associated with the webhook.</param>
    /// <param name="headersWebhookDtos">Optional collection of <see cref="Webhook2HeadersDto"/> objects representing headers associated with the webhook.</param>
    /// <returns>A <see cref="Webhook"/> entity constructed from the provided DTOs.</returns>
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
            Name = dto.Name,
            Description = dto.Description,
        };

        return entity;
    }

    /// <summary>
    /// Creates a new <see cref="WebhookDto"/> instance by copying the properties from the specified <see cref="IWebhook"/>.
    /// </summary>
    /// <param name="webhook">The <see cref="IWebhook"/> instance to convert.</param>
    /// <returns>A <see cref="WebhookDto"/> populated with values from the given <paramref name="webhook"/>.</returns>
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
        };

        return dto;
    }

    /// <summary>
    /// Creates a collection of <see cref="Webhook2ContentTypeKeysDto"/> objects that map the specified webhook to its associated content type keys.
    /// </summary>
    /// <param name="webhook">The webhook instance whose content type keys will be mapped.</param>
    /// <returns>An enumerable of <see cref="Webhook2ContentTypeKeysDto"/> representing the association between the webhook and each content type key.</returns>
    public static IEnumerable<Webhook2ContentTypeKeysDto> BuildEntityKey2WebhookDto(IWebhook webhook) =>
        webhook.ContentTypeKeys.Select(x => new Webhook2ContentTypeKeysDto
        {
            ContentTypeKey = x,
            WebhookId = webhook.Id,
        });

    /// <summary>
    /// Creates a collection of <see cref="Webhook2EventsDto"/> objects for each event in the specified <see cref="IWebhook"/>.
    /// </summary>
    /// <param name="webhook">The webhook instance whose events will be converted.</param>
    /// <returns>An enumerable of <see cref="Webhook2EventsDto"/> representing each event associated with the webhook.</returns>
    public static IEnumerable<Webhook2EventsDto> BuildEvent2WebhookDto(IWebhook webhook) =>
        webhook.Events.Select(x => new Webhook2EventsDto
        {
            Event = x,
            WebhookId = webhook.Id,
        });

    /// <summary>
    /// Creates a collection of <see cref="Webhook2HeadersDto"/> objects from the headers of the specified <see cref="IWebhook"/>.
    /// </summary>
    /// <param name="webhook">The webhook whose headers will be converted.</param>
    /// <returns>An <see cref="IEnumerable{Webhook2HeadersDto}"/> representing the headers of the webhook.</returns>
    public static IEnumerable<Webhook2HeadersDto> BuildHeaders2WebhookDtos(IWebhook webhook) =>
        webhook.Headers.Select(x => new Webhook2HeadersDto
        {
            Key = x.Key,
            Value = x.Value,
            WebhookId = webhook.Id,
        });
}
