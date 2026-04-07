using Umbraco.Cms.Api.Management.ViewModels.Webhook;
using Umbraco.Cms.Api.Management.ViewModels.Webhook.Logs;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for webhooks.
/// </summary>
public interface IWebhookPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="WebhookResponseModel"/> from the specified <see cref="IWebhook"/> instance.
    /// </summary>
    /// <param name="webhook">The <see cref="IWebhook"/> to convert.</param>
    /// <returns>A <see cref="WebhookResponseModel"/> representing the webhook.</returns>
    WebhookResponseModel CreateResponseModel(IWebhook webhook);

    /// <summary>
    /// Creates an <see cref="IWebhook"/> instance from the specified request model.
    /// </summary>
    /// <param name="webhookRequestModel">The request model containing the webhook details.</param>
    /// <returns>The created <see cref="IWebhook"/> instance.</returns>
    IWebhook CreateWebhook(CreateWebhookRequestModel webhookRequestModel);

    /// <summary>
    /// Creates or updates a webhook using the specified update request model and the key of an existing webhook.
    /// </summary>
    /// <param name="webhookRequestModel">The request model containing the updated webhook details.</param>
    /// <param name="existingWebhookKey">The unique key identifying the existing webhook to update.</param>
    /// <returns>The updated <see cref="IWebhook"/> instance.</returns>
    IWebhook CreateWebhook(UpdateWebhookRequestModel webhookRequestModel, Guid existingWebhookKey);

    /// <summary>
    /// Creates a response model from the given webhook log.
    /// </summary>
    /// <param name="webhookLog">The webhook log to create the response model from.</param>
    /// <returns>A <see cref="WebhookLogResponseModel"/> representing the webhook log.</returns>
    WebhookLogResponseModel CreateResponseModel(WebhookLog webhookLog) => new();
}
