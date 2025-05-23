using System.Security.Claims;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

internal sealed class ServerEventAuthorizationService : IServerEventAuthorizationService
{
    private readonly EventSourceAuthorizerCollection _eventSourceAuthorizers;
    private Dictionary<string, List<IEventSourceAuthorizer>>? _groupedAuthorizersByEventSource;

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
