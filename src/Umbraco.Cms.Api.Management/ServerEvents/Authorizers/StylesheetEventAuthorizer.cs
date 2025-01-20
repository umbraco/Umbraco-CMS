using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class StylesheetEventAuthorizer : EventSourcePolicyAuthorizer
{
    public StylesheetEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Stylesheet];

    protected override string Policy => AuthorizationPolicies.TreeAccessStylesheets;
}
