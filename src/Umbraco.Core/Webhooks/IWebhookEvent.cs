namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// Represents a webhook event that can be triggered by the CMS.
/// </summary>
public interface IWebhookEvent
{
    /// <summary>
    /// Gets the friendly display name of the webhook event.
    /// </summary>
    string EventName { get; }

    /// <summary>
    /// Gets the type/category of the webhook event.
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Gets the unique alias used to identify this webhook event.
    /// </summary>
    string Alias { get; }
}
