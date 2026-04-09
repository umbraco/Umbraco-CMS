using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for server events related to public access entries.
/// </summary>
public class PublicAccessEntryEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicAccessEntryEventAuthorizer"/> class, which handles authorization for public access entries.
    /// </summary>
    /// <param name="authorizationService">An <see cref="IAuthorizationService"/> used to authorize access.</param>
    public PublicAccessEntryEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event source identifiers that this authorizer is able to authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.PublicAccessEntry];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocuments;
}
