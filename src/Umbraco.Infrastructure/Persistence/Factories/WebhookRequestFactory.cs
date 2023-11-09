using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookRequestFactory
{
    public static WebhookRequestDto CreateDto(WebhookRequest webhookRequest) =>
        new()
        {
            Alias = webhookRequest.EventAlias,
            Id = webhookRequest.Id,
            WebhookKey = webhookRequest.WebhookKey,
            RequestObject = webhookRequest.RequestObject,
            RetryCount = webhookRequest.RetryCount,
        };

    public static WebhookRequest CreateModel(WebhookRequestDto webhookRequestDto) =>
        new()
        {
            EventAlias = webhookRequestDto.Alias,
            Id = webhookRequestDto.Id,
            WebhookKey = webhookRequestDto.WebhookKey,
            RequestObject = webhookRequestDto.RequestObject,
            RetryCount = webhookRequestDto.RetryCount,
        };
}
