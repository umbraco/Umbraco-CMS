using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class UserConnectionManager : IUserConnectionManager
{
    /// <summary>
    /// Maps each connected user's key to their active SignalR connection ids.
    /// </summary>
    /// <remarks>
    /// We use a normal dictionary instead of ConcurrentDictionary, since we need to lock the set anyway.
    /// </remarks>
    private readonly Dictionary<Guid, HashSet<string>> _connections = new();

    /// <summary>
    /// Maps each connected user's key to the event sources they are authorized for, captured at connect time.
    /// </summary>
    private readonly Dictionary<Guid, HashSet<string>> _authorizedEventSources = new();

    private readonly Lock _lock = new();

    /// <inheritdoc/>
    public ISet<string> GetConnections(Guid userKey)
    {
        lock (_lock)
        {
            return _connections.TryGetValue(userKey, out HashSet<string>? connections) ? connections : [];
        }
    }

    /// <inheritdoc/>
    public void SetAuthorizedEventSources(Guid userKey, IEnumerable<string> eventSources)
    {
        lock (_lock)
        {
            _authorizedEventSources[userKey] = [.. eventSources];
        }
    }

    /// <inheritdoc/>
    public IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> GetConnectionsAuthorizedFor(string eventSource)
    {
        lock (_lock)
        {
            var result = new Dictionary<Guid, IReadOnlyCollection<string>>();
            foreach ((Guid userKey, HashSet<string> connections) in _connections)
            {
                if (_authorizedEventSources.TryGetValue(userKey, out HashSet<string>? sources)
                    && sources.Contains(eventSource))
                {
                    result[userKey] = connections.ToArray();
                }
            }

            return result;
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
                _authorizedEventSources.Remove(userKey);
            }
        }
    }
}
