using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventEntityAccessService : IServerEventEntityAccessService
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly IUserService _userService;
    private readonly ServerEventEntityAccessFilterCollection _filters;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventEntityAccessService"/> class.
    /// </summary>
    /// <param name="userConnectionManager">Tracks the currently connected users and their connection ids.</param>
    /// <param name="userService">Resolves the <see cref="Umbraco.Cms.Core.Models.Membership.IUser"/> for a connected user's key.</param>
    /// <param name="filters">The registered entity access filters that decide, per source, whether a user may receive an event.</param>
    public ServerEventEntityAccessService(
        IUserConnectionManager userConnectionManager,
        IUserService userService,
        ServerEventEntityAccessFilterCollection filters)
    {
        _userConnectionManager = userConnectionManager;
        _userService = userService;
        _filters = filters;
    }

    private Dictionary<string, List<IServerEventEntityAccessFilter>> FiltersByEventSource =>
        field ??= GroupFiltersByEventSource();

    /// <inheritdoc />
    public bool AppliesTo(string eventSource) => FiltersByEventSource.ContainsKey(eventSource);

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetAuthorizedConnectionsAsync(string eventSource, ServerEventRoutingContext context)
    {
        if (FiltersByEventSource.TryGetValue(eventSource, out List<IServerEventEntityAccessFilter>? filters) is false)
        {
            return [];
        }

        IReadOnlyDictionary<Guid, IReadOnlyCollection<string>> connectionsByUser = _userConnectionManager.GetAllConnections();
        if (connectionsByUser.Count == 0)
        {
            return [];
        }

        // Resolve all connected users in a single (cache-backed) call rather than one per user.
        IEnumerable<IUser> users = await _userService.GetAsync(connectionsByUser.Keys);

        List<string> authorizedConnections = [];

        foreach (IUser user in users)
        {
            if (connectionsByUser.TryGetValue(user.Key, out IReadOnlyCollection<string>? connections) is false
                || connections.Count == 0)
            {
                continue;
            }

            if (await HasAccessAsync(filters, user, context))
            {
                authorizedConnections.AddRange(connections);
            }
        }

        return authorizedConnections;
    }

    /// <summary>
    /// Determines whether the user is granted access by the filters registered for an event source.
    /// </summary>
    /// <remarks>
    /// When more than one filter is registered for a source, the user must satisfy them all.
    /// </remarks>
    /// <param name="filters">The filters registered for the event source.</param>
    /// <param name="user">The user to check.</param>
    /// <param name="context">The routing context describing the entity the event concerns.</param>
    /// <returns><c>true</c> if the user is granted access by all filters; otherwise <c>false</c>.</returns>
    private static async Task<bool> HasAccessAsync(List<IServerEventEntityAccessFilter> filters, IUser user, ServerEventRoutingContext context)
    {
        foreach (IServerEventEntityAccessFilter filter in filters)
        {
            if (await filter.HasAccessAsync(user, context) is false)
            {
                return false;
            }
        }

        return true;
    }

    private Dictionary<string, List<IServerEventEntityAccessFilter>> GroupFiltersByEventSource()
    {
        var grouped = new Dictionary<string, List<IServerEventEntityAccessFilter>>();

        foreach (IServerEventEntityAccessFilter filter in _filters)
        {
            foreach (var eventSource in filter.FilteredEventSources)
            {
                if (grouped.TryGetValue(eventSource, out List<IServerEventEntityAccessFilter>? filters) is false)
                {
                    filters = [];
                    grouped[eventSource] = filters;
                }

                filters.Add(filter);
            }
        }

        return grouped;
    }
}
