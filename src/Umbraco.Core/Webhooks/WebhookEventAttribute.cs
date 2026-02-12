namespace Umbraco.Cms.Core.Webhooks;

/// <summary>
/// Attribute used to decorate webhook event classes with metadata such as name and event type.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class WebhookEventAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventAttribute"/> class with the specified name.
    /// </summary>
    /// <param name="name">The friendly name of the webhook event.</param>
    public WebhookEventAttribute(string name)
    : this(name, Constants.WebhookEvents.Types.Other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookEventAttribute"/> class with the specified name and event type.
    /// </summary>
    /// <param name="name">The friendly name of the webhook event.</param>
    /// <param name="eventType">The type/category of the webhook event.</param>
    public WebhookEventAttribute(string name, string eventType)
    {
        Name = name;
        EventType = eventType;
    }

    /// <summary>
    ///     Gets the friendly name of the event.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    ///     Gets the type of event.
    /// </summary>
    public string? EventType { get; }
}
