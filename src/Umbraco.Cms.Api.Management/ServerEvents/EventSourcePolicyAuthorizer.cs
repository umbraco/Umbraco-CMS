using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core.ServerEvents;

namespace Umbraco.Cms.Api.Management.ServerEvents;

/// <summary>
/// Provides authorization logic for event sources within the server events system, determining access based on defined policies.
/// </summary>
public abstract class EventSourcePolicyAuthorizer : IEventSourceAuthorizer
{
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventSourcePolicyAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service used for policy checks.</param>
    public EventSourcePolicyAuthorizer(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets a collection of event source identifiers that can be authorized by this policy authorizer.
    /// </summary>
    public abstract IEnumerable<string> AuthorizableEventSources { get; }

    protected abstract string Policy { get; }

    /// <summary>
    /// Asynchronously authorizes the specified principal for the given event source.
    /// </summary>
    /// <param name="principal">The claims principal to authorize.</param>
    /// <param name="eventSource">The event source for which authorization is requested.</param>
    /// <returns>A task that represents the asynchronous authorization operation. The task result contains true if authorization succeeded; otherwise, false.</returns>
    public async Task<bool> AuthorizeAsync(ClaimsPrincipal principal, string eventSource)
    {
        AuthorizationResult result = await _authorizationService.AuthorizeAsync(principal, Policy);
        return result.Succeeded;
    }
}
