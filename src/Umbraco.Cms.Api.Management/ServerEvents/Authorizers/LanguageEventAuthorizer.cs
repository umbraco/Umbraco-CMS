using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

public class LanguageEventAuthorizer : EventSourcePolicyAuthorizer
{
    public LanguageEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Language];

    protected override string Policy => AuthorizationPolicies.TreeAccessLanguages;
}
