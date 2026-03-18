using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
/// Factory for creating webhook log entries.
/// </summary>
public interface IWebhookLogFactory
{
    /// <summary>
    /// Creates a new <see cref="WebhookLog"/> entry based on the webhook request and response.
    /// </summary>
    /// <param name="eventAlias">The alias of the event that triggered the webhook.</param>
    /// <param name="requestMessage">The HTTP request message sent to the webhook endpoint.</param>
    /// <param name="httpResponseMessage">The HTTP response message received, or <c>null</c> if no response was received.</param>
    /// <param name="retryCount">The number of retry attempts made.</param>
    /// <param name="exception">The exception that occurred during the request, or <c>null</c> if successful.</param>
    /// <param name="webhook">The <see cref="IWebhook"/> that was fired.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the created <see cref="WebhookLog"/>.</returns>
    Task<WebhookLog> CreateAsync(
        string eventAlias,
        HttpRequestMessage requestMessage,
        HttpResponseMessage? httpResponseMessage,
        int retryCount,
        Exception? exception,
        IWebhook webhook,
        CancellationToken cancellationToken);
}
