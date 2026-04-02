using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Authorizes server events related to document blueprints.
/// </summary>
public class DocumentBlueprintEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentBlueprintEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">
    /// An <see cref="IAuthorizationService"/> instance used to check permissions for document blueprint events.
    /// </param>
    public DocumentBlueprintEventAuthorizer(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DocumentBlueprint];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentsOrDocumentTypes;
}
