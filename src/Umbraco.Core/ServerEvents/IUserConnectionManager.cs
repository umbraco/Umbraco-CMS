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
    /// Gets a snapshot of every tracked user connection, keyed by user.
    /// </summary>
    /// <returns>A snapshot mapping each user key to their current connection ids.</returns>
    /// <remarks>
    /// Returning an empty snapshot here would masquerade as "no users connected" and silently drop
    /// entity-scoped server events, so this default throws instead — implementations of
    /// <see cref="IUserConnectionManager"/> must override it.
    /// </remarks>
    IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> GetAllConnections()
        => throw new NotImplementedException($"Implementations of {nameof(IUserConnectionManager)} must override {nameof(GetAllConnections)}.");

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
