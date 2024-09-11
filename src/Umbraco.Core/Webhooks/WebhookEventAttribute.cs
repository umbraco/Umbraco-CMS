namespace Umbraco.Cms.Core.Webhooks;

[AttributeUsage(AttributeTargets.Class)]
public class WebhookEventAttribute : Attribute
{
    public WebhookEventAttribute(string name)
    : this(name, Constants.WebhookEvents.Types.Other)
    {
    }

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
