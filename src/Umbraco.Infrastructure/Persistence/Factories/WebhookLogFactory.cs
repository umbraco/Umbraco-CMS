using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Webhooks;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookLogFactory
{
    public static WebhookLogDto CreateDto(WebhookLog log) =>
        new()
        {
            Date = log.Date,
            EventAlias = log.EventAlias,
            RequestBody = log.RequestBody ?? string.Empty,
            ResponseBody = log.ResponseBody,
            RetryCount = log.RetryCount,
            StatusCode = log.StatusCode,
            Key = log.Key,
            Id = log.Id,
            Url = log.Url,
            RequestHeaders = log.RequestHeaders,
            ResponseHeaders = log.ResponseHeaders,
            WebhookKey = log.WebhookKey,
        };

    public static WebhookLog DtoToEntity(WebhookLogDto dto) =>
        new()
        {
            Date = dto.Date,
            EventAlias = dto.EventAlias,
            RequestBody = dto.RequestBody,
            ResponseBody = dto.ResponseBody,
            RetryCount = dto.RetryCount,
            StatusCode = dto.StatusCode,
            Key = dto.Key,
            Id = dto.Id,
            Url = dto.Url,
            RequestHeaders = dto.RequestHeaders,
            ResponseHeaders = dto.ResponseHeaders,
            WebhookKey = dto.WebhookKey,
        };
}
