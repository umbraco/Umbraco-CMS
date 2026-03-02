using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes dictionary access based on cultures.
/// </summary>
public interface IDictionaryPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified cultures for dictionary operations.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="culturesToCheck">The collection of cultures to check authorization for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsAuthorizedForCultures(IUser currentUser, ISet<string> culturesToCheck);
}
