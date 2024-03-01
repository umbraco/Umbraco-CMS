namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes Umbraco features.
/// </summary>
public interface IFeatureAuthorizer
{
    /// <summary>
    ///     Authorizes the current action.
    /// </summary>
    /// <param name="type">The type to check if is disabled.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(Type type);
}
