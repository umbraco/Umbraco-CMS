using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookLogFactory
{
    /// <summary>
    /// Creates a <see cref="WebhookLogDto"/> from the given <see cref="WebhookLog"/> instance.
    /// </summary>
    /// <param name="log">The <see cref="WebhookLog"/> instance to convert.</param>
    /// <returns>A <see cref="WebhookLogDto"/> representing the provided <paramref name="log"/>.</returns>
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
            ExceptionOccured = log.ExceptionOccured,
        };

    /// <summary>
    /// Creates a <see cref="WebhookLog"/> entity from the specified <see cref="WebhookLogDto"/>.
    /// </summary>
    /// <param name="dto">The <see cref="WebhookLogDto"/> instance to convert.</param>
    /// <returns>A new <see cref="WebhookLog"/> entity populated with values from the <paramref name="dto"/>.</returns>
    public static WebhookLog DtoToEntity(WebhookLogDto dto) =>
        new()
        {
            Date = dto.Date.EnsureUtc(),
            EventAlias = dto.EventAlias,
            RequestBody = dto.RequestBody,
            ResponseBody = dto.ResponseBody,
            RetryCount = dto.RetryCount,
            StatusCode = dto.StatusCode,
            IsSuccessStatusCode = Regex.IsMatch(dto.StatusCode, "^.*\\(2(\\d{2})\\)$"),
            Key = dto.Key,
            Id = dto.Id,
            Url = dto.Url,
            RequestHeaders = dto.RequestHeaders,
            ResponseHeaders = dto.ResponseHeaders,
            WebhookKey = dto.WebhookKey,
            ExceptionOccured = dto.ExceptionOccured,
        };
}
