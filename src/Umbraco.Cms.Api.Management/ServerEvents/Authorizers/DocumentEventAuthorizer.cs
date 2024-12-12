using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class DocumentEventAuthorizer : EventSourcePolicyAuthorizer
{
    public DocumentEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }


    public override IEnumerable<string> AuthorizedEventSources => [Constants.ServerEvents.EventSource.Document];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocuments;
}
