namespace Umbraco.Cms.Core.Models.ServerEvents;

/// <summary>
///     Server-side context used when routing a <see cref="ServerEvent" /> to decide which connected
///     users are permitted to receive it. This is never serialized to clients and is not part of the
///     SignalR payload; it exists purely to gate delivery.
/// </summary>
public sealed class ServerEventRoutingContext
{
    /// <summary>
    ///     Gets the tree path (comma-separated node identifiers) of the entity the event concerns.
    ///     Used to gate document and media events by the recipient's start-node access. It is
    ///     <c>null</c> for event sources that have no per-entity access boundary.
    /// </summary>
    public string? EntityPath { get; init; }
}
