namespace Umbraco.Cms.Core.Models.ServerEvents;

/// <summary>
///     Represents a server-sent event that can be pushed to connected clients.
/// </summary>
public class ServerEvent
{
    /// <summary>
    ///     Gets or sets the type of the event.
    /// </summary>
    public required string EventType { get; set; }

    /// <summary>
    ///     Gets or sets the source of the event.
    /// </summary>
    public required string EventSource { get; set; }

    /// <summary>
    ///     Gets or sets the unique key associated with the event.
    /// </summary>
    public Guid Key { get; set; }
}
