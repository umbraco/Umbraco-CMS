using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for handling relation-related events in the Umbraco CMS server events system.
/// </summary>
public class RelationEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RelationEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">An <see cref="IAuthorizationService"/> instance used to perform permission checks for relation events.</param>
    public RelationEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Relation];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentsOrMediaOrMembersOrContentTypes;
}
