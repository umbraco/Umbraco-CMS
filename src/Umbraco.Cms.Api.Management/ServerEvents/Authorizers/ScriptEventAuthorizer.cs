using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for script-related server events.
/// </summary>
public class ScriptEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScriptEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">The <see cref="IAuthorizationService"/> used to authorize script event requests.</param>
    public ScriptEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Script];

    protected override string Policy => AuthorizationPolicies.TreeAccessScripts;
}
