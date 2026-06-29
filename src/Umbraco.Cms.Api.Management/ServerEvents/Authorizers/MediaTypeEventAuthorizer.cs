using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Authorizes events related to media types in the server events system.
/// </summary>
public class MediaTypeEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaTypeEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> used to perform permission checks for media type events.</param>
    public MediaTypeEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.MediaType];

    protected override string Policy => AuthorizationPolicies.TreeAccessMediaTypes;
}
