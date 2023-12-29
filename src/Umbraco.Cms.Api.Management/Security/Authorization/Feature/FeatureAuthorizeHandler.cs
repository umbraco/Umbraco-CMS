using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Feature;

/// <summary>
///     Authorizes that the controller is an authorized Umbraco feature.
/// </summary>
public class FeatureAuthorizeHandler : MustSatisfyRequirementAuthorizationHandler<FeatureAuthorizeRequirement>
{
    private readonly IFeatureAuthorizer _featureAuthorizer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FeatureAuthorizeHandler" /> class.
    /// </summary>
    /// <param name="featureAuthorizer">Authorizer for Umbraco features.</param>
    public FeatureAuthorizeHandler(IFeatureAuthorizer featureAuthorizer)
        => _featureAuthorizer = featureAuthorizer;

    /// <inheritdoc />
    protected override async Task<bool> IsAuthorized(AuthorizationHandlerContext context, FeatureAuthorizeRequirement requirement)
        => await _featureAuthorizer.IsAuthorizedAsync(context);
}
