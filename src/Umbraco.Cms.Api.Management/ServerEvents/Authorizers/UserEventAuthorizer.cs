using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for user-related events in the server events management system.
/// </summary>
public class UserEventAuthorizer : EventSourcePolicyAuthorizer
{
    public UserEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.User];

    protected override string Policy => AuthorizationPolicies.SectionAccessUsers;
}
