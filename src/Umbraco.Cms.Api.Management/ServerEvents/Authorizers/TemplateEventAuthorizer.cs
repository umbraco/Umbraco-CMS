using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for handling template-related events in the server events system.
/// </summary>
public class TemplateEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">Service used to check user permissions.</param>
    public TemplateEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Template];

    protected override string Policy => AuthorizationPolicies.TreeAccessTemplates;
}
