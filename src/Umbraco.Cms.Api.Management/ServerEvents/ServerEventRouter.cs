using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Api.Management.ServerEvents.Models;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.Routing;

public class ServerEventRouter : IServerEventRouter
{
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly IAuthorizationService _authorizationService;

    private readonly FrozenDictionary<EventSource, string> _groupMappings;

    public ServerEventRouter(
        IHubContext<ServerEventHub, IServerEventHub> eventHub,
        IAuthorizationService authorizationService)
    {
        _eventHub = eventHub;
        _authorizationService = authorizationService;

        _groupMappings = new Dictionary<EventSource, string>()
        {
            { EventSource.Document, AuthorizationPolicies.TreeAccessDocuments },
            { EventSource.DocumentType, AuthorizationPolicies.TreeAccessDocumentTypes },
            { EventSource.Media, AuthorizationPolicies.TreeAccessMediaOrMediaTypes },
            { EventSource.MediaType, AuthorizationPolicies.TreeAccessMediaTypes },
            { EventSource.Member, AuthorizationPolicies.TreeAccessMembersOrMemberTypes },
            { EventSource.MemberType, AuthorizationPolicies.TreeAccessMemberTypes },
        }.ToFrozenDictionary();
    }

    public Task RouteEventAsync(ServerEvent serverEvent) => throw new NotImplementedException();

    public async Task AssignToGroups(ClaimsPrincipal user, string connectionId)
    {
        foreach (KeyValuePair<EventSource, string> mapping in _groupMappings)
        {
            AuthorizationResult result = await _authorizationService.AuthorizeAsync(user, mapping.Value);
            if (result.Succeeded)
            {
                await _eventHub.Groups.AddToGroupAsync(connectionId, mapping.Key.ToString());
            }
        }
    }
}
