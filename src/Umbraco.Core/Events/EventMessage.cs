namespace Umbraco.Cms.Core.Events;

/// <summary>
///     An event message
/// </summary>
public sealed class EventMessage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventMessage" /> class.
    /// </summary>
    public EventMessage(string category, string message, EventMessageType messageType = EventMessageType.Default)
    {
        Category = category;
        Message = message;
        MessageType = messageType;
    }

    public string Category { get; }

    public string Message { get; }

    public EventMessageType MessageType { get; }

    /// <summary>
    ///     This is used to track if this message should be used as a default message so that Umbraco doesn't also append it's
    ///     own default messages
    /// </summary>
    public bool IsDefaultEventMessage { get; set; }
}
