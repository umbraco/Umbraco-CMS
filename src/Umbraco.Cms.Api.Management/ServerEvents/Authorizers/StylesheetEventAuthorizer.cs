using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

    /// <summary>
    /// Provides authorization logic for stylesheet-related server events in the Umbraco CMS API.
    /// </summary>
public class StylesheetEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StylesheetEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize stylesheet-related actions.</param>
    public StylesheetEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Stylesheet];

    protected override string Policy => AuthorizationPolicies.TreeAccessStylesheets;
}
