using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class PublicAccessEntryEventAuthorizer : EventSourcePolicyAuthorizer
{
    public PublicAccessEntryEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.PublicAccessEntry];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocuments;
}
