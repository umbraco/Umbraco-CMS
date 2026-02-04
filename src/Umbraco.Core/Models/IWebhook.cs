using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a webhook configuration for sending HTTP callbacks when events occur.
/// </summary>
public interface IWebhook : IEntity
{
    // TODO (V16): Remove the default implementations from this interface.

    /// <summary>
    ///     Gets or sets the display name of the webhook.
    /// </summary>
    string? Name
    {
        get { return null; }
        set { }
    }

    /// <summary>
    ///     Gets or sets the description of the webhook.
    /// </summary>
    string? Description
    {
        get { return null; }
        set { }
    }

    /// <summary>
    ///     Gets or sets the URL to which the webhook payload will be sent.
    /// </summary>
    string Url { get; set; }

    /// <summary>
    ///     Gets or sets the array of event names that this webhook subscribes to.
    /// </summary>
    string[] Events { get; set; }

    /// <summary>
    ///     Gets or sets the array of content type keys that this webhook is filtered by.
    /// </summary>
    Guid[] ContentTypeKeys { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the webhook is enabled.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets the custom HTTP headers to include with the webhook request.
    /// </summary>
    IDictionary<string, string> Headers { get; set; }
}
