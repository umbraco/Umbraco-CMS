using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Authorizes server events related to document types in the Umbraco CMS API.
/// </summary>
public class DocumentTypeEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of <see cref="DocumentTypeEventAuthorizer"/>.
    /// </summary>
    /// <param name="authorizationService">Service used to authorize document type events.</param>
    public DocumentTypeEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event source identifiers that this authorizer is able to authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DocumentType];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocumentTypes;
}
