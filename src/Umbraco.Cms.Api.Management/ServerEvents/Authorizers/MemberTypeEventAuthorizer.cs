using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for server events that pertain to member types.
/// </summary>
public class MemberTypeEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MemberTypeEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service to use for authorization checks.</param>
    public MemberTypeEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.MemberType];

    protected override string Policy => AuthorizationPolicies.TreeAccessMemberTypes;
}
