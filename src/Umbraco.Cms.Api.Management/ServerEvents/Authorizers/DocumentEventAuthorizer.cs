using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for document-related events in the server event system.
/// </summary>
public class DocumentEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">The service used to authorize document-related events.</param>
    public DocumentEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }


    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Document];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocuments;
}
