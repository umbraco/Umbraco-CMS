using Umbraco.Cms.Core;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Common.DependencyInjection;

/// <summary>
///     The default <see cref="IOpenIddictPathsToHandleProvider"/>, yielding the paths Umbraco itself needs
///     OpenIddict to handle.
/// </summary>
/// <remarks>
///     Derive from this class and override <see cref="GetPathsToHandle"/> to add your own paths while
///     retaining the Umbraco ones.
/// </remarks>
public class OpenIddictPathsToHandleProvider : IOpenIddictPathsToHandleProvider
{
    /// <summary>
    ///     Gets the back-office path segment, as an absolute path with a trailing slash.
    /// </summary>
    protected static string BackOfficePathSegment { get; } = Constants.System.DefaultUmbracoPath
        .TrimStart(Constants.CharArrays.Tilde)
        .EnsureStartsWith('/')
        .EnsureEndsWith('/');

    /// <inheritdoc/>
    public virtual IEnumerable<string> GetPathsToHandle()
    {
        yield return BackOfficePathSegment;
        yield return "/.well-known/openid-configuration";
        yield return "/.well-known/jwks";
    }
}
