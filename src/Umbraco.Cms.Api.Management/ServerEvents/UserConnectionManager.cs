namespace Umbraco.Cms.Api.Management.ServerEvents;

internal sealed class UserConnectionManager : IUserConnectionManager
{
    // We use a normal dictionary instead of ConcurrentDictionary, since we need to lock the set anyways.
    private readonly Dictionary<Guid, HashSet<string>> _connections = new();
    private readonly object _lock = new();

    public ISet<string> GetConnections(Guid userKey)
    {
        lock (_lock)
        {
            return _connections.TryGetValue(userKey, out HashSet<string>? connections) ? connections : [];
        }
    }

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
