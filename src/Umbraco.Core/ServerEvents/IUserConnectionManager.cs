namespace Umbraco.Cms.Core.ServerEvents;

/// <summary>
/// A manager that tracks connection ids for users.
/// </summary>
public interface IUserConnectionManager
{
    /// <summary>
    /// Get all connections held by a user.
    /// </summary>
    /// <param name="userKey">The key of the user to get connections for.</param>
    /// <returns>The users connections.</returns>
    ISet<string> GetConnections(Guid userKey);

    /// <summary>
    /// Records the event sources a user is authorized for, as determined at connect time. This is the
    /// authorization consulted when routing entity-scoped events by connection id (as opposed to the
    /// SignalR group broadcast).
    /// </summary>
    /// <param name="userKey">The key of the user.</param>
    /// <param name="eventSources">The event sources the user is authorized for.</param>
    /// <remarks>
    /// The default is a no-op so a custom implementation that does not track this still connects; such
    /// an implementation must also override <see cref="GetConnectionsAuthorizedFor"/> for entity-scoped
    /// events to be delivered.
    /// </remarks>
    // TODO (V19): Remove this default implementation.
    void SetAuthorizedEventSources(Guid userKey, IEnumerable<string> eventSources)
        => throw new NotImplementedException($"Implementations of {nameof(IUserConnectionManager)} must override {nameof(SetAuthorizedEventSources)}.");

    /// <summary>
    /// Gets the connections whose user is authorized for the given event source, keyed by user.
    /// </summary>
    /// <param name="eventSource">The event source to filter by.</param>
    /// <returns>A snapshot mapping each authorized user's key to their current connection ids.</returns>
    /// <remarks>
    /// Returning an empty snapshot here would masquerade as "nobody authorized" and silently drop
    /// entity-scoped server events, so this default throws instead — implementations of
    /// <see cref="IUserConnectionManager"/> must override it.
    /// </remarks>
    // TODO (V19): Remove this default implementation.
    IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> GetConnectionsAuthorizedFor(string eventSource)
        => throw new NotImplementedException($"Implementations of {nameof(IUserConnectionManager)} must override {nameof(GetConnectionsAuthorizedFor)}.");

    /// <summary>
    /// Add a connection to a user.
    /// </summary>
    /// <param name="userKey">The key of the user to add the connection to.</param>
    /// <param name="connectionId">Connection id to add.</param>
    void AddConnection(Guid userKey, string connectionId);

    /// <summary>
    /// Removes a connection from a user.
    /// </summary>
    /// <param name="userKey">The user key to remove the connection from.</param>
    /// <param name="connectionId">The connection id to remove</param>
    void RemoveConnection(Guid userKey, string connectionId);
}
