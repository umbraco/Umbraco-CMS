using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization for server events related to partial views.
/// </summary>
public class PartialViewEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PartialViewEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">
    /// The <see cref="IAuthorizationService"/> instance used to check permissions for partial view events.
    /// </param>
    public PartialViewEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.PartialView];

    protected override string Policy => AuthorizationPolicies.TreeAccessPartialViews;
}
