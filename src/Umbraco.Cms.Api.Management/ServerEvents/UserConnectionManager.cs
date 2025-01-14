using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class UserConnectionManager : IUserConnectionManager
{
    // We use a normal dictionary instead of ConcurrentDictionary, since we need to lock the set anyways.
    private readonly Dictionary<Guid, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    /// <inheritdoc/>
    public ISet<string> GetConnections(Guid userKey)
    {
        lock (_lock)
        {
            return _connections.TryGetValue(userKey, out HashSet<string>? connections) ? connections : [];
        }
    }

    /// <inheritdoc/>
    public void AddConnection(Guid userKey, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userKey, out HashSet<string>? connections) is false)
            {
                connections = [];
                _connections[userKey] = connections;
            }

            connections.Add(connectionId);
        }
    }

    /// <inheritdoc/>
    public void RemoveConnection(Guid userKey, string connectionId)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(userKey, out HashSet<string>? connections) is false)
            {
                return;
            }

            connections.Remove(connectionId);
            if (connections.Count == 0)
            {
                _connections.Remove(userKey);
            }
        }
    }
}
