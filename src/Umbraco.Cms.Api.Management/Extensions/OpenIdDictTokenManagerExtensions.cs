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

    /// <summary>
    /// Revokes the OpenID Connect tokens issued to the specified client for the specified Umbraco user.
    /// </summary>
    /// <param name="openIddictTokenManager">The OpenID Connect token manager.</param>
    /// <param name="openIddictApplicationManager">The OpenID Connect application manager used to resolve the client.</param>
    /// <param name="userKey">The unique identifier of the Umbraco user whose tokens will be revoked.</param>
    /// <param name="clientId">The client ID of the application whose tokens will be revoked.</param>
    /// <returns>A task that represents the asynchronous revoke operation.</returns>
    public static async Task RevokeUmbracoUserTokens(
        this IOpenIddictTokenManager openIddictTokenManager,
        IOpenIddictApplicationManager openIddictApplicationManager,
        Guid userKey,
        string clientId)
    {
        var application = await openIddictApplicationManager.FindByClientIdAsync(clientId);
        if (application is null)
        {
            return;
        }

        // Tokens reference the application by its store identifier, not by its client ID.
        var applicationId = await openIddictApplicationManager.GetIdAsync(application);
        if (applicationId is null)
        {
            return;
        }

        var tokens = await openIddictTokenManager.FindAsync(userKey.ToString(), applicationId, status: null, type: null).ToArrayAsync();

        foreach (var token in tokens)
        {
            await openIddictTokenManager.DeleteAsync(token);
        }
    }
}
