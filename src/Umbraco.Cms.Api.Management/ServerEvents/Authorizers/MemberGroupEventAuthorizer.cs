using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class MemberGroupEventAuthorizer : EventSourcePolicyAuthorizer
{
    public MemberGroupEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.MemberGroup];

    protected override string Policy => AuthorizationPolicies.TreeAccessMemberGroups;
}
