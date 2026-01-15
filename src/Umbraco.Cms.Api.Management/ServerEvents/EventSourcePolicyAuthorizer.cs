using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

public abstract class EventSourcePolicyAuthorizer : IEventSourceAuthorizer
{
    private readonly IAuthorizationService _authorizationService;

    public EventSourcePolicyAuthorizer(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    public abstract IEnumerable<string> AuthorizableEventSources { get; }

    protected abstract string Policy { get; }

    public async Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string eventSource)
    {
        AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, Policy);
        return result.Succeeded;
    }
}
