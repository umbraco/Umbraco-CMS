using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

    /// <summary>
    /// Provides authorization logic for media-related events in the server events system.
    /// </summary>
public class MediaEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MediaEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service used to authorize media events.</param>
    public MediaEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Media];

    protected override string Policy => AuthorizationPolicies.TreeAccessMediaOrMediaTypes;
}
