using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security.Authorization;

/// <summary>
///     Authorizes media access.
/// </summary>
public interface IMediaPermissionAuthorizer
{
    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="mediaKey">The key of the media item to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, Guid mediaKey)
        => IsDeniedAsync(currentUser, mediaKey.Yield());

    /// <summary>
    ///     Authorizes whether the current user has access to the specified media item(s).
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="mediaKeys">The keys of the media items to check for.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAsync(IUser currentUser, IEnumerable<Guid> mediaKeys);

    /// <summary>
    ///     Authorizes whether the current user has access to the root item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRootLevelAsync(IUser currentUser);

    /// <summary>
    ///     Authorizes whether the current user has access to the recycle bin item.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <returns>Returns <c>true</c> if authorization is successful, otherwise <c>false</c>.</returns>
    Task<bool> IsDeniedAtRecycleBinLevelAsync(IUser currentUser);

    /// <summary>
    ///     Filters the specified media keys to only those the user has access to.
    /// </summary>
    /// <param name="currentUser">The current user.</param>
    /// <param name="mediaKeys">The keys of the media items to filter.</param>
    /// <returns>Returns the keys of media items the user has access to.</returns>
    /// <remarks>
    ///     The default implementation falls back to calling <see cref="IsDeniedAsync(IUser, IEnumerable{Guid})"/>
    ///     for each key individually. Override this method for better performance with batch authorization.
    /// </remarks>
    // TODO (V18): Remove default implementation and make this method required.
    async Task<ISet<Guid>> FilterAuthorizedAsync(IUser currentUser, IEnumerable<Guid> mediaKeys)
    {
        var results = await Task.WhenAll(mediaKeys.Select(async key =>
            (key, isAuthorized: await IsDeniedAsync(currentUser, [key]) == false)));

        return results.Where(r => r.isAuthorized).Select(r => r.key).ToHashSet();
    }
}
