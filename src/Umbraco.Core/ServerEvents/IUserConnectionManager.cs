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
