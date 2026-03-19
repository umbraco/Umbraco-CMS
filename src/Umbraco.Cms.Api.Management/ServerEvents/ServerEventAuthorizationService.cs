using System.Security.Claims;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

internal sealed class ServerEventAuthorizationService : IServerEventAuthorizationService
{
    private readonly EventSourceAuthorizerCollection _eventSourceAuthorizers;
    private Dictionary<string, List<IEventSourceAuthorizer>>? _groupedAuthorizersByEventSource;

    /// <summary>
    /// Initializes a new instance of the <see cref="ServerEventAuthorizationService"/> class, which manages authorization for server events.
    /// </summary>
    /// <param name="eventSourceAuthorizers">A collection of authorizers used to determine access permissions for server event sources.</param>
    public ServerEventAuthorizationService(EventSourceAuthorizerCollection eventSourceAuthorizers)
    {
        _eventSourceAuthorizers = eventSourceAuthorizers;
    }

    private Dictionary<string, List<IEventSourceAuthorizer>> GroupedAuthorizersByEventSource
    {
        get
        {
            if (_groupedAuthorizersByEventSource is not null)
            {
                return _groupedAuthorizersByEventSource;
            }

            _groupedAuthorizersByEventSource = GetGroupedAuthorizersByEventSource();
            return _groupedAuthorizersByEventSource;
        }
    }

    private IEnumerable<string> AuthorizableEventSources => GroupedAuthorizersByEventSource.Keys;

    private Dictionary<string, List<IEventSourceAuthorizer>> GetGroupedAuthorizersByEventSource()
    {
        var groupedAuthorizers = new Dictionary<string, List<IEventSourceAuthorizer>>();

        foreach (IEventSourceAuthorizer eventSourceAuthorizer in _eventSourceAuthorizers)
        {
            foreach (var eventSource in eventSourceAuthorizer.AuthorizableEventSources)
            {
                if (groupedAuthorizers.TryGetValue(eventSource, out List<IEventSourceAuthorizer>? authorizers) is false)
                {
                    authorizers = [eventSourceAuthorizer];
                    groupedAuthorizers[eventSource] = authorizers;
                }

                authorizers.Add(eventSourceAuthorizer);
            }
        }

        return groupedAuthorizers;
    }

    /// <summary>
    /// Asynchronously determines which event sources the specified user is authorized to access by evaluating all configured event source authorizers.
    /// Returns a result indicating the event sources for which the user is authorized or unauthorized.
    /// </summary>
    /// <param name="user">The <see cref="ClaimsPrincipal"/> representing the user to authorize.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a <see cref="SeverEventAuthorizationResult"/> with lists of authorized and unauthorized event sources.</returns>
    public async Task<SeverEventAuthorizationResult> AuthorizeAsync(ClaimsPrincipal user)
    {
        var authorizedEventSources = new List<string>();
        var unauthorizedEventSources = new List<string>();

        foreach (var eventSource in AuthorizableEventSources)
        {
            if (GroupedAuthorizersByEventSource.TryGetValue(eventSource, out List<IEventSourceAuthorizer>? authorizers) is false || authorizers.Count == 0)
            {
                throw new InvalidOperationException($"No authorizers found for event source {eventSource}");
            }

            // There may have been registered multiple authorizers for the same event source, in that case if any authorizer fails, the user is unauthorized.
            var isAuthorized = false;
            foreach (IEventSourceAuthorizer authorizer in authorizers)
            {
                isAuthorized = await authorizer.AuthorizeAsync(user, eventSource);
                if (isAuthorized is false)
                {
                    break;
                }
            }

            if (isAuthorized)
            {
                authorizedEventSources.Add(eventSource);
            }
            else
            {
                unauthorizedEventSources.Add(eventSource);
            }
        }

        return new SeverEventAuthorizationResult
        {
            AuthorizedEventSources = authorizedEventSources,
            UnauthorizedEventSources = unauthorizedEventSources,
        };
    }
}
