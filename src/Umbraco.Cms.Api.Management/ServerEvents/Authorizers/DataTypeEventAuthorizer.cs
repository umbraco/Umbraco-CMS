using Microsoft.AspNetCore.Authorization;
using Umbraco.Cms.Core;
using Umbraco.Cms.Web.Common.Authorization;

namespace Umbraco.Cms.Api.Management.ServerEvents.Authorizers;

/// <summary>
/// Provides authorization logic for server events that are related to data types.
/// </summary>
public class DataTypeEventAuthorizer : EventSourcePolicyAuthorizer
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataTypeEventAuthorizer"/> class.
    /// </summary>
    /// <param name="authorizationService">An <see cref="IAuthorizationService"/> instance used to perform permission checks for data type events.</param>
    public DataTypeEventAuthorizer(IAuthorizationService authorizationService) : base(authorizationService)
    {
    }

    /// <summary>
    /// Gets the collection of event sources that this authorizer can authorize.
    /// </summary>
    public override IEnumerable<string> AuthorizableEventSources => [Constants.ServerEvents.EventSource.DataType];

    protected override string Policy => AuthorizationPolicies.TreeAccessDataTypes;
}
