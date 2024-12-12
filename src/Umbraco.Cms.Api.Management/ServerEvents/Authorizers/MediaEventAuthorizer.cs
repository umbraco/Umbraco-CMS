using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class MediaEventAuthorizer : EventSourcePolicyAuthorizer
{
    public MediaEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizedEventSources => [Constants.ServerEvents.EventSource.Media];

    protected override string Policy => AuthorizationPolicies.TreeAccessMediaOrMediaTypes;
}
