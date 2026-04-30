using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Persistence.Factories;

internal static class WebhookRequestFactory
{
    /// <summary>
    /// Creates a <see cref="WebhookRequestDto"/> from the given <see cref="WebhookRequest"/>.
    /// </summary>
    /// <param name="webhookRequest">The <see cref="WebhookRequest"/> to convert.</param>
    /// <returns>A <see cref="WebhookRequestDto"/> representing the given <paramref name="webhookRequest"/>.</returns>
    public static WebhookRequestDto CreateDto(WebhookRequest webhookRequest) =>
        new()
        {
            Alias = webhookRequest.EventAlias,
            Id = webhookRequest.Id,
            WebhookKey = webhookRequest.WebhookKey,
            RequestObject = webhookRequest.RequestObject,
            RetryCount = webhookRequest.RetryCount,
        };

    /// <summary>
    /// Creates a <see cref="WebhookRequest"/> model from the given <see cref="WebhookRequestDto"/>.
    /// </summary>
    /// <param name="webhookRequestDto">The data transfer object containing webhook request data.</param>
    /// <returns>A <see cref="WebhookRequest"/> model populated with data from the DTO.</returns>
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
