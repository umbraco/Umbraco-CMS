using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookRequestFactory
{
    public static WebhookRequestDto CreateDto(WebhookRequest webhookRequest) =>
        new()
        {
            EventName = webhookRequest.EventName,
            Id = webhookRequest.Id,
            WebhookKey = webhookRequest.WebhookKey,
            RequestObject = webhookRequest.RequestObject,
            RetryCount = webhookRequest.RetryCount,
        };

    public static WebhookRequest CreateModel(WebhookRequestDto webhookRequestDto) =>
        new()
        {
            EventName = webhookRequestDto.EventName,
            Id = webhookRequestDto.Id,
            WebhookKey = webhookRequestDto.WebhookKey,
            RequestObject = webhookRequestDto.RequestObject,
            RetryCount = webhookRequestDto.RetryCount,
        };
}
