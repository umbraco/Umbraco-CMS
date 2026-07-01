using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for server events that pertain to dictionary items.
/// </summary>
public class DictionaryItemEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DictionaryItemEventAuthorizer"/> class with the specified authorization service.
    /// </summary>
    /// <param name="authorizationService">An <see cref="IAuthorizationService"/> used to check permissions for dictionary item events.</param>
    public DictionaryItemEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DictionaryItem];

    protected override string Policy => AuthorizationPolicies.TreeAccessDictionary;
}
