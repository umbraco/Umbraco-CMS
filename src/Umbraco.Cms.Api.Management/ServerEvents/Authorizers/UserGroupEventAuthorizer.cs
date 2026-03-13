using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for server events related to user groups.
/// </summary>
public class UserGroupEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserGroupEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">An <see cref="IAuthorizationService"/> instance used to check user group permissions.</param>
    public UserGroupEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.UserGroup];

    protected override string Policy => AuthorizationPolicies.SectionAccessUsers;
}
