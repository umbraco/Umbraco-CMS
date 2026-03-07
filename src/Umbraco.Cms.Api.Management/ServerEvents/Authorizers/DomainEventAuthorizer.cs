using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

    /// <summary>
    /// Provides authorization logic for handling domain events in the server events system.
    /// </summary>
public class DomainEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">The service used to authorize domain events.</param>
    public DomainEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.Domain];

    protected override string Policy => AuthorizationPolicies.TreeAccessDocuments;
}
