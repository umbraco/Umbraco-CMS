using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class RelationEventAuthorizer : EventSourcePolicyAuthorizer
{
    public RelationEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Relation];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentsOrMediaOrMembersOrContentTypes;
}
