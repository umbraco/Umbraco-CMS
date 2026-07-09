using Umbraco.Extensions;

namespace Umbraco.Cms.Core;

/// <summary>
///     Provides utility methods for working with URIs.
/// </summary>
public static class UriUtilityCore
{
    /// <summary>
    ///     Determines whether the specified URI string contains a scheme (e.g., "http://").
    /// </summary>
    /// <param name="uri">The URI string to check.</param>
    /// <returns><c>true</c> if the URI has a scheme; otherwise, <c>false</c>.</returns>
    public static bool HasScheme(string uri) => uri.IndexOf("://", StringComparison.Ordinal) > 0;

    /// <summary>
    ///     Ensures the URI string starts with a scheme, using HTTP as the default.
    /// </summary>
    /// <param name="uri">The URI string.</param>
    /// <returns>The URI string with a scheme prefix.</returns>
    public static string StartWithScheme(string uri) => StartWithScheme(uri, null);

    /// <summary>
    ///     Ensures the URI string starts with the specified scheme.
    /// </summary>
    /// <param name="uri">The URI string.</param>
    /// <param name="scheme">The scheme to use, or null to use HTTP.</param>
    /// <returns>The URI string with a scheme prefix.</returns>
    public static string StartWithScheme(string uri, string? scheme) =>
        HasScheme(uri) ? uri : string.Format("{0}://{1}", scheme ?? Uri.UriSchemeHttp, uri);

    /// <summary>
    ///     Ensures the path portion of the URI ends with a slash.
    /// </summary>
    /// <param name="uri">The URI string.</param>
    /// <returns>The URI string with the path ending in a slash.</returns>
    public static string EndPathWithSlash(string uri)
    {
        ReadOnlySpan<char> uriSpan = uri.AsSpan();
        var pos = IndexOfPathEnd(uriSpan);

        var path = (pos > 0 ? uriSpan[..pos] : uriSpan).ToString();
        path = path.EnsureEndsWith('/');

        if (pos > 0)
        {
            return string.Concat(path, uriSpan[pos..]);
        }

        return path;
    }

    /// <summary>
    ///     Removes the trailing slash from the path portion of the URI.
    /// </summary>
    /// <param name="uri">The URI string.</param>
    /// <returns>The URI string with the trailing slash removed from the path.</returns>
    public static string TrimPathEndSlash(string uri)
    {
        ReadOnlySpan<char> uriSpan = uri.AsSpan();
        var pos = IndexOfPathEnd(uriSpan);

        var path = (pos > 0 ? uriSpan[..pos] : uriSpan).ToString();
        path = path.TrimEnd(Constants.CharArrays.ForwardSlash);

        if (pos > 0)
        {
            return string.Concat(path, uriSpan[pos..]);
        }

        return path;
    }

    private static int IndexOfPathEnd(ReadOnlySpan<char> uri)
    {
        var pos1 = Math.Max(0, uri.IndexOf('?'));
        var pos2 = Math.Max(0, uri.IndexOf('#'));
        return pos1 == 0 && pos2 == 0 ? 0
            : pos1 == 0 ? pos2
            : pos2 == 0 ? pos1
            : Math.Min(pos1, pos2);
    }
}
