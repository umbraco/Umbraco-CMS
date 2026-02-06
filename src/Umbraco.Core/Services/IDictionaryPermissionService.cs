using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services.AuthorizationStatus;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Manages permissions for dictionary item access.
/// </summary>
public interface IDictionaryPermissionService
{
    /// <summary>
    ///     Authorizes that a user has access to specific cultures for dictionary operations.
    /// </summary>
    /// <param name="user">The user to authorize.</param>
    /// <param name="culturesToCheck">The collection of culture codes to check access for.</param>
    /// <returns>A task resolving into a <see cref="DictionaryAuthorizationStatus" /> indicating the authorization result.</returns>
    Task<DictionaryAuthorizationStatus> AuthorizeCultureAccessAsync(IUser user, ISet<string> culturesToCheck);
}
