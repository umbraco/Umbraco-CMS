using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Service for firing webhooks in response to events.
/// </summary>
public interface IWebhookFiringService
{
    /// <summary>
    /// Fires a webhook asynchronously.
    /// </summary>
    /// <param name="webhook">The <see cref="IWebhook"/> to fire.</param>
    /// <param name="eventAlias">The alias of the event that triggered the webhook.</param>
    /// <param name="payload">The payload to send with the webhook request.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task FireAsync(IWebhook webhook, string eventAlias, object? payload, CancellationToken cancellationToken);
}
