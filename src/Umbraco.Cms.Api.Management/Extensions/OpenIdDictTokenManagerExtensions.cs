using OpenIddict.Abstractions;

namespace Umbraco.Extensions;

public static class OpenIdDictTokenManagerExtensions
{
    public static async Task RevokeUmbracoUserTokens(this IOpenIddictTokenManager openIddictTokenManager, Guid userKey)
    {
        var tokens = await openIddictTokenManager.FindBySubjectAsync(userKey.ToString()).ToArrayAsync();
        if (tokens.Length == 0)
        {
            return;
        }

        foreach (var token in tokens)
        {
            await openIddictTokenManager.DeleteAsync(token);
        }
    }
}
