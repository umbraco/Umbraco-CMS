using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Authorizes events related to member groups within the server events system.
/// </summary>
public class MemberGroupEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberGroupEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize actions on member group events.</param>
    public MemberGroupEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.MemberGroup];

    protected override string Policy => AuthorizationPolicies.TreeAccessMemberGroups;
}
