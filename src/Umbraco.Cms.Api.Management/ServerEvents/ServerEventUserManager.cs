using System.Collections.Frozen;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Umbraco.Cms.Core.Models.ServerEvents;
using Umbraco.Cms.Core.ServerEvents;
using Umbraco.Cms.Web.Common.Authorization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <inheritdoc />
internal sealed class ServerEventUserManager : IServerEventUserManager
{
    private readonly IUserConnectionManager _userConnectionManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHubContext<ServerEventHub, IServerEventHub> _eventHub;
    private readonly FrozenDictionary<EventSource, string> _groupMappings;

    public ServerEventUserManager(
        IUserConnectionManager userConnectionManager,
        IAuthorizationService authorizationService,
        IHubContext<ServerEventHub, IServerEventHub> eventHub)
    {
        _userConnectionManager = userConnectionManager;
        _authorizationService = authorizationService;
        _eventHub = eventHub;

        _groupMappings = new Dictionary<EventSource, string>
        {
            { EventSource.Document, AuthorizationPolicies.TreeAccessDocuments },
            { EventSource.DocumentType, AuthorizationPolicies.TreeAccessDocumentTypes },
            { EventSource.Media, AuthorizationPolicies.TreeAccessMediaOrMediaTypes },
            { EventSource.MediaType, AuthorizationPolicies.TreeAccessMediaTypes },
            { EventSource.Member, AuthorizationPolicies.TreeAccessMembersOrMemberTypes },
            { EventSource.MemberType, AuthorizationPolicies.TreeAccessMemberTypes },
            { EventSource.MemberGroup, AuthorizationPolicies.TreeAccessMemberGroups },
            { EventSource.DataType, AuthorizationPolicies.TreeAccessDataTypes },
            { EventSource.Language, AuthorizationPolicies.TreeAccessLanguages },
            { EventSource.Script, AuthorizationPolicies.TreeAccessScripts },
            { EventSource.Stylesheet, AuthorizationPolicies.TreeAccessStylesheets },
            { EventSource.Template, AuthorizationPolicies.TreeAccessTemplates },
            { EventSource.DictionaryItem, AuthorizationPolicies.TreeAccessDictionary },
            { EventSource.Domain, AuthorizationPolicies.TreeAccessDocuments },
            { EventSource.PartialView, AuthorizationPolicies.TreeAccessPartialViews },
            { EventSource.PublicAccessEntry, AuthorizationPolicies.TreeAccessDocuments },
            { EventSource.Relation, AuthorizationPolicies.TreeAccessDocuments },
            { EventSource.RelationType, AuthorizationPolicies.TreeAccessRelationTypes },
            { EventSource.UserGroup, AuthorizationPolicies.SectionAccessUsers },
            { EventSource.User, AuthorizationPolicies.SectionAccessUsers },
            { EventSource.Webhook, AuthorizationPolicies.TreeAccessWebhooks },
        }.ToFrozenDictionary();
    }

    /// <inheritdoc />
    public async Task AssignToGroupsAsync(ClaimsPrincipal user, string connectionId)
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

    /// <inheritdoc />
    public async Task RefreshGroupsAsync(ClaimsPrincipal user)
    {
        Guid? userKey = user.Identity?.GetUserKey();

        // If we can't resolve the user key from the principal something is quite wrong, and we shouldn't continue.
        if (userKey is null)
        {
            throw new InvalidOperationException("Unable to resolve user key.");
        }

        // Ensure that all the users connections are removed from all groups.
        ISet<string> connections = _userConnectionManager.GetConnections(userKey.Value);

        // If there's no connection there's nothing to do.
        if (connections.Count == 0)
        {
            return;
        }

        foreach (KeyValuePair<EventSource, string> mapping in _groupMappings)
        {
            AuthorizationResult authorizationResult = await _authorizationService.AuthorizeAsync(user, mapping.Value);

            // It's safe to add an existing connection to a group
            if (authorizationResult.Succeeded)
            {
                foreach (var connection in connections)
                {
                    await _eventHub.Groups.AddToGroupAsync(connection, mapping.Key.ToString());
                }

                continue;
            }

            foreach (var connection in connections)
            {
                await _eventHub.Groups.RemoveFromGroupAsync(connection, mapping.Key.ToString());
            }
        }
    }
}
