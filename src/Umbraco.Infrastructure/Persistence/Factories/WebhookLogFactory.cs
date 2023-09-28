using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookLogFactory
{
    public static WebhookLogDto CreateDto(WebhookLog log) =>
        new()
        {
            Date = log.Date,
            EventName = log.EventName,
            RequestBody = log.RequestBody,
            ResponseBody = log.ResponseBody,
            RetryCount = log.RetryCount,
            StatusCode = log.StatusCode,
            Key = log.Key,
            Id = log.Id,
        };
}
