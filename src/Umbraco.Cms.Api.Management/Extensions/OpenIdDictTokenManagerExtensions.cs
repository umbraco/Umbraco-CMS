using OpenIddict.Abstractions;

namespace Umbraco.Extensions;

/// <summary>
/// Contains extension methods for the management and manipulation of OpenIdDict tokens in the Umbraco CMS environment.
/// </summary>
public static class OpenIdDictTokenManagerExtensions
{
    /// <summary>
    /// Revokes all OpenID Connect tokens associated with the specified Umbraco user.
    /// </summary>
    /// <param name="openIddictTokenManager">The OpenID Connect token manager.</param>
    /// <param name="userKey">The unique identifier of the Umbraco user whose tokens will be revoked.</param>
    /// <returns>A task that represents the asynchronous revoke operation.</returns>
    public static async Task RevokeUmbracoUserTokens(this IOpenIddictTokenManager openIddictTokenManager, Guid userKey)
    {
        var tokens = await openIddictTokenManager.FindBySubjectAsync(userKey.ToString()).ToArrayAsync();

        foreach (var token in tokens)
        {
            await openIddictTokenManager.DeleteAsync(token);
        }
    }
}
