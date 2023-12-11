using Microsoft.AspNetCore.Authorization;

namespace Umbraco.Cms.Api.Management.Security.Authorization.Feature;

/// <summary>
///     Authorizes Umbraco features.
/// </summary>
public interface IFeatureAuthorizer
{
    /// <summary>
    ///     Authorizes the current action.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedAsync(AuthorizationHandlerContext context);
}
