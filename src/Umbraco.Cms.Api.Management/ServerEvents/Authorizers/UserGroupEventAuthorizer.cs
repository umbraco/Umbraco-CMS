using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class UserGroupEventAuthorizer : EventSourcePolicyAuthorizer
{
    public UserGroupEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.UserGroup];

    protected override string Policy => AuthorizationPolicies.SectionAccessUsers;
}
