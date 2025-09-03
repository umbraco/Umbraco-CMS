using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class DocumentBlueprintEventAuthorizer : EventSourcePolicyAuthorizer
{
    public DocumentBlueprintEventAuthorizer(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DocumentBlueprint];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes;
}
