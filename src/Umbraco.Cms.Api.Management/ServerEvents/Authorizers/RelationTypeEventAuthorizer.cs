using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

    /// <summary>
    /// Provides authorization logic for server events that pertain to relation types.
    /// </summary>
public class RelationTypeEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationTypeEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">The authorization service used to authorize actions.</param>
    public RelationTypeEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.RelationType];

    protected override string Policy => AuthorizationPolicies.TreeAccessRelationTypes;
}
