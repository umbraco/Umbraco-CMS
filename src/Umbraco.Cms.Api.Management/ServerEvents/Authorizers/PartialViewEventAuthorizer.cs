using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class PartialViewEventAuthorizer : EventSourcePolicyAuthorizer
{
    public PartialViewEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.PartialView];

    protected override string Policy => AuthorizationPolicies.TreeAccessPartialViews;
}
