// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Web;
using Umbraco.Cms.Core;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="Uri" />.
/// </summary>
public static class UriExtensions
{
    /// <summary>
    ///     Rewrites the path of uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="path">The new path, which must begin with a slash.</param>
    /// <returns>The rewritten uri.</returns>
    /// <remarks>Everything else remains unchanged, except for the fragment which is removed.</remarks>
    public static Uri Rewrite(this Uri uri, string path)
    {
        if (path.StartsWith("/") == false)
        {
            throw new ArgumentException("Path must start with a slash.", "path");
        }

        return uri.IsAbsoluteUri
            ? new Uri(uri.GetLeftPart(UriPartial.Authority) + path + uri.Query)
            : new Uri(path + uri.GetSafeQuery(), UriKind.Relative);
    }

    /// <summary>
    ///     Rewrites the path and query of a uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="path">The new path, which must begin with a slash.</param>
    /// <param name="query">The new query, which must be empty or begin with a question mark.</param>
    /// <returns>The rewritten uri.</returns>
    /// <remarks>Everything else remains unchanged, except for the fragment which is removed.</remarks>
    public static Uri Rewrite(this Uri uri, string path, string query)
    {
        if (path.StartsWith("/") == false)
        {
            throw new ArgumentException("Path must start with a slash.", "path");
        }

        if (query.Length > 0 && query.StartsWith("?") == false)
        {
            throw new ArgumentException("Query must start with a question mark.", "query");
        }

        if (query == "?")
        {
            query = string.Empty;
        }

        return uri.IsAbsoluteUri
            ? new Uri(uri.GetLeftPart(UriPartial.Authority) + path + query)
            : new Uri(path + query, UriKind.Relative);
    }

    /// <summary>
    ///     Gets the absolute path of the uri, even if the uri is relative.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The absolute path of the uri.</returns>
    /// <remarks>Default uri.AbsolutePath does not support relative uris.</remarks>
    public static string GetSafeAbsolutePath(this Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            return uri.AbsolutePath;
        }

        // cannot get .AbsolutePath on relative uri (InvalidOperation)
        var s = uri.OriginalString;

        // TODO: Shouldn't this just use Uri.GetLeftPart?
        var posq = s.IndexOf("?", StringComparison.Ordinal);
        var posf = s.IndexOf("#", StringComparison.Ordinal);
        var pos = posq > 0 ? posq : posf > 0 ? posf : 0;
        var path = pos > 0 ? s.Substring(0, pos) : s;
        return path;
    }

    /// <summary>
    ///     Gets the decoded, absolute path of the uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The absolute path of the uri.</returns>
    /// <remarks>Only for absolute uris.</remarks>
    public static string GetAbsolutePathDecoded(this Uri uri) => HttpUtility.UrlDecode(uri.AbsolutePath);

    /// <summary>
    ///     Gets the decoded, absolute path of the uri, even if the uri is relative.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The absolute path of the uri.</returns>
    /// <remarks>Default uri.AbsolutePath does not support relative uris.</remarks>
    public static string GetSafeAbsolutePathDecoded(this Uri uri) => WebUtility.UrlDecode(uri.GetSafeAbsolutePath());

    /// <summary>
    ///     Rewrites the path of the uri so it ends with a slash.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The rewritten uri.</returns>
    /// <remarks>Everything else remains unchanged.</remarks>
    public static Uri EndPathWithSlash(this Uri uri)
    {
        var path = uri.GetSafeAbsolutePath();
        if (uri.IsAbsoluteUri)
        {
            if (path != "/" && path.EndsWith("/") == false)
            {
                uri = new Uri(uri.GetLeftPart(UriPartial.Authority) + path + "/" + uri.Query);
            }

            return uri;
        }

        if (path != "/" && path.EndsWith("/") == false)
        {
            uri = new Uri(path + "/" + uri.Query, UriKind.Relative);
        }

        return uri;
    }

    /// <summary>
    ///     Rewrites the path of the uri so it does not end with a slash.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The rewritten uri.</returns>
    /// <remarks>Everything else remains unchanged.</remarks>
    public static Uri TrimPathEndSlash(this Uri uri)
    {
        var path = uri.GetSafeAbsolutePath();
        if (uri.IsAbsoluteUri)
        {
            if (path != "/")
            {
                uri = new Uri(uri.GetLeftPart(UriPartial.Authority) + path.TrimEnd(Constants.CharArrays.ForwardSlash) +
                              uri.Query);
            }
        }
        else
        {
            if (path != "/")
            {
                uri = new Uri(path.TrimEnd(Constants.CharArrays.ForwardSlash) + uri.Query, UriKind.Relative);
            }
        }

        return uri;
    }

    /// <summary>
    ///     Transforms a relative uri into an absolute uri.
    /// </summary>
    /// <param name="uri">The relative uri.</param>
    /// <param name="baseUri">The base absolute uri.</param>
    /// <returns>The absolute uri.</returns>
    public static Uri MakeAbsolute(this Uri uri, Uri baseUri)
    {
        if (uri.IsAbsoluteUri)
        {
            throw new ArgumentException("Uri is already absolute.", "uri");
        }

        return new Uri(baseUri.GetLeftPart(UriPartial.Authority) + uri.GetSafeAbsolutePath() + uri.GetSafeQuery());
    }

    /// <summary>
    ///     Removes the port from the uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <returns>The same uri, without its port.</returns>
    public static Uri WithoutPort(this Uri uri) =>
        new Uri(uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Port, UriFormat.UriEscaped));

    private static string? GetSafeQuery(this Uri uri)
    {
        if (uri.IsAbsoluteUri)
        {
            return uri.Query;
        }

        // cannot get .Query on relative uri (InvalidOperation)
        var s = uri.OriginalString;
        var posq = s.IndexOf("?", StringComparison.Ordinal);
        var posf = s.IndexOf("#", StringComparison.Ordinal);
        var query = posq < 0 ? null : (posf < 0 ? s.Substring(posq) : s.Substring(posq, posf - posq));

        return query;
    }

    /// <summary>
    ///     Replaces the host of a uri.
    /// </summary>
    /// <param name="uri">The uri.</param>
    /// <param name="host">A replacement host.</param>
    /// <returns>The same uri, with its host replaced.</returns>
    public static Uri ReplaceHost(this Uri uri, string host) => new UriBuilder(uri) { Host = host }.Uri;
}
