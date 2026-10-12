using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for element-related events in the server event system.
/// </summary>
public class ElementEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ElementEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">The service used to authorize element-related events.</param>
    public ElementEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Element];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentsOrElementsOrMediaOrMembers;
}
