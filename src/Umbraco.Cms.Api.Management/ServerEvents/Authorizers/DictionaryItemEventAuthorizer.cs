using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class DictionaryItemEventAuthorizer : EventSourcePolicyAuthorizer
{
    public DictionaryItemEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DictionaryItem];

    protected override string Policy => AuthorizationPolicies.TreeAccessDictionary;
}
